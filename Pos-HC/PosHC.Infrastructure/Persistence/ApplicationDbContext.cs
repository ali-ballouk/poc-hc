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

            modelBuilder.Entity<CatalogItem>()
            .Property(v => v.Settings)
            .HasColumnType("nvarchar(max)");

            modelBuilder.Entity<CatalogItem>()
                .Property(v => v.Type)
                .HasConversion<int>(); // store enum as int
        }
        public DbSet<Doctor> Doctor => Set<Doctor>();
        public DbSet<Patient> Patient => Set<Patient>();

        public DbSet<CatalogItem> CatalogItem => Set<CatalogItem>();

        public DbSet<Invoice> Invoice => Set<Invoice>();

        public DbSet<PaymentType> PaymentType => Set<PaymentType>();


    }
}