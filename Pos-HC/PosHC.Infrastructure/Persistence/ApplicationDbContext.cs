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
            modelBuilder.Entity<StaffUser>().Property(x => x.Username).HasMaxLength(100);
            modelBuilder.Entity<StaffUser>().Property(x => x.DisplayName).HasMaxLength(150);
            modelBuilder.Entity<StaffUser>().Property(x => x.Role).HasMaxLength(30);
            modelBuilder.Entity<ClinicSettings>().Property(x => x.Name).HasMaxLength(150);
            modelBuilder.Entity<ClinicSettings>().Property(x => x.Address).HasMaxLength(500);
            modelBuilder.Entity<ClinicSettings>().Property(x => x.Phone).HasMaxLength(50);
            modelBuilder.Entity<Invoice>().Property(x => x.VisitDescription).HasMaxLength(4000);
            modelBuilder.Entity<Invoice>().Property(x => x.Diagnosis).HasMaxLength(2000);
            modelBuilder.Entity<ClinicSettings>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<ClinicSettings>().HasOne<Doctor>().WithMany().HasForeignKey(x => x.DefaultDoctorId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Invoice>().HasIndex(x => new { x.PatientId, x.CreatedAt, x.Id });
            modelBuilder.Entity<StaffUser>().HasIndex(x => x.Username).IsUnique();
            modelBuilder.Entity<Invoice>().HasIndex(x => x.Number).IsUnique();
            modelBuilder.Entity<Invoice>().HasIndex(x => x.RequestId).IsUnique().HasFilter("[RequestId] IS NOT NULL");
            modelBuilder.Entity<Payment>().HasIndex(x => x.RequestId).IsUnique().HasFilter("[RequestId] IS NOT NULL");
            modelBuilder.Entity<CreditNote>().HasIndex(x => x.RequestId).IsUnique();
            modelBuilder.Entity<CashShift>().HasIndex(x => new { x.UserId, x.Currency }).IsUnique().HasFilter("[ClosedAt] IS NULL");
            modelBuilder.Entity<Appointment>().HasIndex(x => new { x.DoctorId, x.StartsAt });
            modelBuilder.Entity<Appointment>().HasOne<Patient>().WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Appointment>().HasOne<Doctor>().WithMany().HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DoctorAvailability>().HasOne<Doctor>().WithMany().HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CreditNote>().HasOne<Invoice>().WithMany().HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CashMovement>().HasOne<CashShift>().WithMany().HasForeignKey(x => x.CashShiftId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CashShift>().HasOne<StaffUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Payment>().HasOne<CashShift>().WithMany().HasForeignKey(x => x.CashShiftId).OnDelete(DeleteBehavior.Restrict);
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties())
                {
                    if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                    {
                        property.SetPrecision(20);
                        property.SetScale(4);
                    }
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc)));
                    }
                }
            }

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

        public DbSet<Payment> Payment => Set<Payment>();
        public DbSet<StaffUser> StaffUser => Set<StaffUser>();
        public DbSet<ClinicSettings> ClinicSettings => Set<ClinicSettings>();
        public DbSet<AuditEntry> AuditEntry => Set<AuditEntry>();
        public DbSet<Appointment> Appointment => Set<Appointment>();
        public DbSet<DoctorAvailability> DoctorAvailability => Set<DoctorAvailability>();
        public DbSet<CashShift> CashShift => Set<CashShift>();
        public DbSet<CashMovement> CashMovement => Set<CashMovement>();
        public DbSet<CreditNote> CreditNote => Set<CreditNote>();


    }
}
