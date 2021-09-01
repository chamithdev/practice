using Api.Extensions;
using Api.Interface;
using AutoMapper;
using CloudinaryDotNet.Actions;
using DA.Model;
using DA.Model.Dtos;
using DA.Model.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        private readonly IPhotoService photoService;

        public UsersController(IUserRepository repository, IMapper mapper, IPhotoService photoService)
        {
            this.repository = repository;
            this.mapper = mapper;
            this.photoService = photoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            IEnumerable<MemberDto> members = await this.repository.GetMembersAsync();
            return Ok(members);
        }

        
        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetUserByUsername(string username)
        {           
            MemberDto member  = await this.repository.GetMembersByUsernameAsync(username);
            return Ok(member);
        }


        [HttpPut]
        public async Task<ActionResult> UpdateMember(MemberUpdateDto member)
        {
            string userName = User.GetUsername();
            AppUser user = await this.repository.GetUserByUsernameAsync(userName);
            this.mapper.Map(member, user);
            this.repository.Update(user);
            if(await this.repository.SaveAllAsync())
            {
                return NoContent();
            }
            return BadRequest("Internal error");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            string userName = User.GetUsername();
            AppUser user = await this.repository.GetUserByUsernameAsync(userName);
            ImageUploadResult result = await this.photoService.AddPhotoAsync(file);
            if (result.Error  != null)
            {
                return BadRequest(result.Error.Message);
            }

            Photo photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,

            };
            if(user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }

            user.Photos.Add(photo);
            if(await this.repository.SaveAllAsync())
            {
                //return CreatedAtRoute("GetUser", userName, this.mapper.Map<PhotoDto>(photo));
                return Ok(this.mapper.Map<PhotoDto>(photo));
            }
            return BadRequest("Error adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")] 
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            string userName = User.GetUsername();
            AppUser user = await this.repository.GetUserByUsernameAsync(userName);
            Photo p = user.Photos.FirstOrDefault(p => p.Id == photoId);
            if (p.IsMain)
            {
                return BadRequest("This is main already");
            }

            Photo current = user.Photos.FirstOrDefault(p => p.IsMain);
            if(current != null)
            {
                current.IsMain = false;
            }
            p.IsMain = true;

            if( await this.repository.SaveAllAsync())
            {
                return NoContent();
            }
            return BadRequest("Faild to set main");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            string userName = User.GetUsername();
            AppUser user = await this.repository.GetUserByUsernameAsync(userName);
            Photo p = user.Photos.FirstOrDefault(p => p.Id == photoId);

            if(p == null)
            {
                return NotFound();
            }

            if (p.IsMain)
            {
                return BadRequest("This is the main photo");
            }

            if(!string.IsNullOrWhiteSpace(p.PublicId))
            {
                DeletionResult res = await this.photoService.DeletePhotoAsync(p.PublicId);
                if(res.Error != null)
                {
                    return BadRequest(res.Error.Message);
                }
                user.Photos.Remove(p);
                if (await this.repository.SaveAllAsync())
                {
                    return Ok();
                }

            }
          
            return BadRequest("Faild to set main");
        }

    }
}
