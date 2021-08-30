using DA.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{  
    public class UsersController : BaseApiController
    {
        private readonly DataContext db;

        public UsersController(DataContext db)
        {
            this.db = db;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
        {
            IEnumerable<AppUser> appUsers = await this.db.Users.ToListAsync();
            return Ok(appUsers);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<AppUser>> GetUser(int id)
        {
            AppUser appUser = await this.db.Users.FirstOrDefaultAsync(u => u.Id == id);
            return Ok(appUser);
        }
    }
}
