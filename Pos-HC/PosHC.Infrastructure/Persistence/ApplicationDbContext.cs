using Microsoft.EntityFrameworkCore;
using PosHC.Domain.Entities;

namespace PosHC.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("poshc");

            base.OnModelCreating(modelBuilder);
        }
        public DbSet<Doctor> Doctor => Set<Doctor>();
        public DbSet<Patient> Patient => Set<Patient>();

        
    }
}