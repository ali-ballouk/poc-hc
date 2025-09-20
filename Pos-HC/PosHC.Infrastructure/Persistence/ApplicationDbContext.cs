using Microsoft.EntityFrameworkCore;
using PosHC.Domain.Entities;


namespace PosHC.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Doctor> Doctor { get; set; }
    }
}
