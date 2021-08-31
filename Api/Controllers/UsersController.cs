using AutoMapper;
using DA.Model;
using DA.Model.Dtos;
using DA.Model.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{  
    [Authorize]
    public class UsersController : BaseApiController
    {
      
        private readonly IUserRepository repository;
        private readonly IMapper mapper;

        public UsersController(IUserRepository repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            IEnumerable<MemberDto> members = await this.repository.GetMembersAsync();
            return Ok(members);
        }

        //[HttpGet("{id}")]
        //public async Task<ActionResult<AppUser>> GetUser(int id)
        //{
        //    AppUser appUser = await this.repository.GetUserByIdAsync(id);
        //    return Ok(appUser);
        //}

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUserByUsername(string username)
        {           
            MemberDto member  = await this.repository.GetMembersByUsernameAsync(username);
            return Ok(member);
        }
    }
}
