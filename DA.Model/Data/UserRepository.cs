using AutoMapper;
using AutoMapper.QueryableExtensions;
using DA.Model.Dtos;
using DA.Model.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA.Model.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext db;
        private readonly IMapper mapper;

        public UserRepository(DataContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<MemberDto>> GetMembersAsync()
        {
            return await this.db.Users               
                .ProjectTo<MemberDto>(this.mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<MemberDto> GetMembersByUsernameAsync(string username)
        {
            return await this.db.Users
                .Where(u => u.UserName == username)
                .ProjectTo<MemberDto>(this.mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<AppUser>> GetUserAsync()
        {
            return await this.db.Users
                .Include(p => p.Photos)
                .ToListAsync();
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await this.db.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await this.db.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await this.db.SaveChangesAsync() > 0;
        }

        public void Update(AppUser user)
        {
            this.db.Entry(user).State = EntityState.Modified;
        }
    }
}
