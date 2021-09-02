using Api.Interface;
using AutoMapper;
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
        private readonly IMapper mapper;

        public AccountController(DataContext db, ITokenService tokenService, IMapper mapper)
        {
            this.db = db;
            this.tokenService = tokenService;
            this.mapper = mapper;
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
            AppUser user = this.mapper.Map<RegisterDto, AppUser>(registerDto);
            using (HMACSHA512 hmac = new HMACSHA512())
            {
                user.UserName = registerDto.Username;
                user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
                user.PasswordSalt = hmac.Key;

                this.db.Add(user);
                await this.db.SaveChangesAsync();

                return Ok(new UserDto { Username = user.UserName, Token = this.tokenService.CreateToken(user), KnownAs= user.KnownAs });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {

            AppUser user = await this.db.Users.Include(u => u.Photos).SingleOrDefaultAsync(x => x.UserName == loginDto.Username);
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
                    Token = this.tokenService.CreateToken(user),
                    PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain)?.Url,
                    KnownAs = user.KnownAs
                });

            }
        }



        private async Task<bool> UserExists(string userName)
        {
            return await this.db.Users.AnyAsync(x => x.UserName.ToLower().Equals(userName.ToLower()));
        }
    }
}
