using Api.Interface;
using DA.Model;
using DA.Model.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Api.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext db;
        private readonly ITokenService tokenService;

        public AccountController(DataContext db, ITokenService tokenService)
        {
            this.db = db;
            this.tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if(string.IsNullOrWhiteSpace(registerDto.Username) || string.IsNullOrWhiteSpace(registerDto.Password))
            {
                return BadRequest("Username and Passwords are required");
            }
            if(await this.UserExists(registerDto.Username))
            {
                return BadRequest("Username is taken");
            }
            using (HMACSHA512 hmac = new HMACSHA512())
            {
                AppUser user = new AppUser
                {
                    UserName = registerDto.Username,
                    PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                    PasswordSalt = hmac.Key
                };

                this.db.Add(user);
                await this.db.SaveChangesAsync();

                return Ok(new UserDto { Username = user.UserName, Token = this.tokenService.CreateToken(user) });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {

            AppUser user = await this.db.Users.SingleOrDefaultAsync(x => x.UserName == loginDto.Username);
            if(user == null)
            {
                return Unauthorized("Invalid login");
            }
           
            using (HMACSHA512 hmac = new HMACSHA512(user.PasswordSalt))
            {

                byte[] passwordSha = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

                for (int i = 0; i < passwordSha.Length; i++)
                {
                    if(passwordSha[i] != user.PasswordHash[i])
                    {
                        return Unauthorized("Invalid login");
                    }
                }

                return Ok( new UserDto
                {
                    Username = user.UserName,
                    Token = this.tokenService.CreateToken(user)
                });

            }
        }



        private async Task<bool> UserExists(string userName)
        {
            return await this.db.Users.AnyAsync(x => x.UserName.ToLower().Equals(userName.ToLower()));
        }
    }
}
