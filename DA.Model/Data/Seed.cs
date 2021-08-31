using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DA.Model.Data
{
    public class Seed
    {
        public static async Task SeedUsers(DataContext db)
        {
            if(await db.Users.AnyAsync())
            {
                return;
            }

            string userData = await System.IO.File.ReadAllTextAsync(@"C:\Projects\DotNet\DatingApp\DA.Model\Data\UserSeedData.json");
            List<AppUser> users = JsonSerializer.Deserialize<List<AppUser>>(userData);
            foreach (AppUser user in users)
            {
                using (HMACSHA512 hmac = new HMACSHA512())
                {
                    user.UserName = user.UserName.ToLower();
                    user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("abc123"));
                    user.PasswordHash = hmac.Key;

                    db.Users.Add(user);
                }
            }

            await db.SaveChangesAsync();

        }
    }
}
