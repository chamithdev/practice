using AutoMapper;
using DA.Model;
using DA.Model.Dtos;
using DA.Model.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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

        
        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUserByUsername(string username)
        {           
            MemberDto member  = await this.repository.GetMembersByUsernameAsync(username);
            return Ok(member);
        }


        [HttpPut]
        public async Task<ActionResult> UpdateMember(MemberUpdateDto member)
        {
            string userName = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            AppUser user = await this.repository.GetUserByUsernameAsync(userName);
            this.mapper.Map(member, user);
            this.repository.Update(user);
            if(await this.repository.SaveAllAsync())
            {
                return NoContent();
            }
            return BadRequest("Internal error");
        }

    }
}
