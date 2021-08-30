using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace DA.Model
{
    public class DataContext : DbContext
    {
        public DataContext([NotNull] DbContextOptions options) : base(options)
        {

        }

        protected DataContext()
        {
        }

        public DbSet<AppUser> Users { get; set; }
    }
}
