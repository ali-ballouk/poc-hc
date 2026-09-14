using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Application.Services;
using PosHC.Domain.Entities;
using PosHC.Infrastructure.Persistence;
using PosHC.Infrastructure.Repositories;

namespace PosHC.Tests;

public class ClinicFeatureTests(ClinicDatabase fixture) : IClassFixture<ClinicDatabase>
{
    [Fact]
    public async Task Sql_sort_orders_all_matches_before_paging_and_resolves_doctor_names()
    {
        await using var db = fixture.Open();
        var marker = Guid.NewGuid().ToString();
        var doctors = new[] { "Zulu", "Alpha", "Mike" }.Select(name => new Doctor { FirstName = name, LastName = marker }).ToArray();
        db.AddRange(doctors);
        var availability = doctors.Select(d => new DoctorAvailability { DoctorId = d.Id, StartsAt = DateTime.UtcNow, EndsAt = DateTime.UtcNow.AddHours(1) }).ToArray();
        db.AddRange(availability);
        await db.SaveChangesAsync();
        var store = new ClinicStore(db);
        var first = await store.List<Doctor>(d => d.LastName == marker, 1, 0, default, "FirstName", "asc");
        var second = await store.List<Doctor>(d => d.LastName == marker, 1, 1, default, "FirstName", "asc");
        Assert.Equal("Alpha", first.Single().FirstName);
        Assert.Equal("Mike", second.Single().FirstName);
        var ids = availability.Select(a => a.Id).ToArray();
        var byName = await store.List<DoctorAvailability>(a => ids.Contains(a.Id), 1, 0, default, "DoctorName", "desc");
        Assert.Equal(doctors[0].Id, byName.Single().DoctorId);
    }
    private sealed class Staff(Guid id, string role = "Administrator") : ICurrentStaff
    {
        public Guid Id => id;
        public string Name => "Visit tester";
        public string Role => role;
    }
    private async Task<(Patient Patient, Doctor Doctor)> Seed(ApplicationDbContext db)
    {
        var settings = await db.ClinicSettings.SingleAsync();
        settings.SingleDoctorMode = false;
        settings.DefaultDoctorId = null;
        var patient = new Patient { Id = Guid.NewGuid(), FirstName = "Visit", LastName = "Patient" };
        var doctor = new Doctor { Id = Guid.NewGuid(), FirstName = "Clinic", LastName = "Doctor", Fee = 35 };
        db.AddRange(patient, doctor);
        await db.SaveChangesAsync();
        return (patient, doctor);
    }
    private (ClinicSettingsService Settings, BillingService Billing, PatientVisitService Visits) Services(ApplicationDbContext db, string role = "Administrator")
    {
        var store = new ClinicStore(db);
        var staff = new Staff(fixture.StaffId, role);
        var audit = new AuditService(store, staff);
        var settings = new ClinicSettingsService(store, audit);
        return (settings, new BillingService(store, settings, new CashShiftService(store, audit, staff), audit, staff),
            new PatientVisitService(store, new PatientVisitReader(db), audit, staff));
    }
    [Fact]
    public async Task Single_doctor_setting_validates_and_assigns_new_visits_without_rewriting_old_ones()
    {
        await using var db = fixture.Open();
        var (patient, doctor) = await Seed(db);
        var services = Services(db);
        await Assert.ThrowsAsync<BusinessException>(() => services.Settings.SaveAsync(new ClinicSettings { SingleDoctorMode = true }, default));
        var input = new CreateInvoiceDto { PatientId = patient.Id, DoctorId = doctor.Id, RequestId = Guid.NewGuid() };
        var original = await services.Billing.Create(input, default);
        await services.Settings.SaveAsync(new ClinicSettings { Name = "Solo clinic", SingleDoctorMode = true, DefaultDoctorId = doctor.Id }, default);
        var solo = new CreateInvoiceDto { PatientId = patient.Id, RequestId = Guid.NewGuid(), VisitDescription = " Follow-up ", Diagnosis = " Optional diagnosis " };
        var invoice = await services.Billing.Create(solo, default);
        Assert.Equal(doctor.Id, invoice.DoctorId);
        Assert.Equal(35, invoice.DoctorFee);
        Assert.Equal("Follow-up", invoice.VisitDescription);
        Assert.Equal("Optional diagnosis", invoice.Diagnosis);
        Assert.Equal(invoice.Id, (await services.Billing.Create(solo, default)).Id);
        Assert.Null(original.VisitDescription);
        Assert.Null(original.Diagnosis);
        await Assert.ThrowsAsync<BusinessException>(() => services.Billing.Create(new CreateInvoiceDto { PatientId = patient.Id, DoctorId = Guid.NewGuid() }, default));
        await services.Settings.SaveAsync(new ClinicSettings { Name = "Shared clinic", SingleDoctorMode = false }, default);
        Assert.Null((await services.Settings.GetAsync(default)).DefaultDoctorId);
        Assert.Equal(doctor.Id, original.DoctorId);
    }
    [Fact]
    public async Task History_is_patient_scoped_newest_first_and_notes_can_be_cleared_without_changing_billing()
    {
        await using var db = fixture.Open();
        var (patient, doctor) = await Seed(db);
        var services = Services(db);
        var first = await services.Billing.Create(new CreateInvoiceDto { PatientId = patient.Id, DoctorId = doctor.Id }, default);
        var second = await services.Billing.Create(new CreateInvoiceDto { PatientId = patient.Id, DoctorId = doctor.Id }, default);
        first.CreatedAt = DateTime.UtcNow.AddDays(-2);
        second.CreatedAt = DateTime.UtcNow.AddDays(-1);
        await db.SaveChangesAsync();
        var page = await services.Visits.GetPageAsync(patient.Id, 1, 1, default);
        Assert.Equal(2, page.Total);
        Assert.Equal(second.Id, Assert.Single(page.Items).Id);
        Assert.Equal(first.Id, Assert.Single((await services.Visits.GetPageAsync(patient.Id, 2, 1, default)).Items).Id);
        var total = second.Total;
        var updated = await services.Visits.UpdateAsync(patient.Id, second.Id, new("Follow-up notes", "Diagnosis"), default);
        Assert.Equal("Diagnosis", updated.Diagnosis);
        Assert.NotNull(updated.UpdatedAt);
        Assert.Equal("Visit tester", updated.UpdatedBy);
        Assert.Equal(total, second.Total);
        Assert.Contains(await db.AuditEntry.ToListAsync(), entry => entry.EntityId == second.Id.ToString() && entry.Action == "UpdateNotes");
        await Assert.ThrowsAsync<BusinessException>(() => services.Visits.UpdateAsync(Guid.NewGuid(), second.Id, new("wrong patient"), default));
        var cleared = await services.Visits.UpdateAsync(patient.Id, second.Id, new(" ", ""), default);
        Assert.Null(cleared.VisitDescription);
        Assert.Null(cleared.Diagnosis);
        Assert.DoesNotContain("VisitDescription", JsonSerializer.Serialize(second));
    }
    [Fact]
    public async Task Notes_reject_oversized_content_and_unauthorized_editors()
    {
        await using var db = fixture.Open();
        var (patient, doctor) = await Seed(db);
        var services = Services(db);
        var invoice = await services.Billing.Create(new CreateInvoiceDto { PatientId = patient.Id, DoctorId = doctor.Id }, default);
        await Assert.ThrowsAsync<BusinessException>(() => services.Visits.UpdateAsync(patient.Id, invoice.Id, new(new string('x', 4001)), default));
        await Assert.ThrowsAsync<BusinessException>(() => services.Visits.UpdateAsync(patient.Id, invoice.Id, new(null, new string('x', 2001)), default));
        await Assert.ThrowsAsync<BusinessException>(() => Services(db, "Cashier").Visits.UpdateAsync(patient.Id, invoice.Id, new("denied"), default));
        Assert.Null(invoice.VisitDescription);
        Assert.Null(invoice.Diagnosis);
    }
    [Fact]
    public async Task Solo_appointments_and_availability_use_configured_doctor_and_prevent_deactivation()
    {
        await using var db = fixture.Open();
        var (patient, doctor) = await Seed(db);
        await Services(db).Settings.SaveAsync(new ClinicSettings { Name = "Solo", SingleDoctorMode = true, DefaultDoctorId = doctor.Id }, default);
        var store = new ClinicStore(db);
        var audit = new AuditService(store, new Staff(fixture.StaffId));
        var doctors = new DoctorService(new POSHCRepository(db), store, audit);
        var start = DateTime.UtcNow.AddDays(3);
        var availability = await doctors.SaveAvailabilityAsync(new DoctorAvailability { StartsAt = start, EndsAt = start.AddHours(2) }, default);
        var appointment = await new AppointmentService(store, audit).SaveAsync(Guid.Empty,
            new Appointment { PatientId = patient.Id, StartsAt = start, EndsAt = start.AddMinutes(30) }, default);
        Assert.Equal(doctor.Id, availability.DoctorId);
        Assert.Equal(doctor.Id, appointment.DoctorId);
        await Assert.ThrowsAsync<BusinessException>(() => doctors.SaveAsync(doctor.Id, new Doctor { FirstName = "Clinic", LastName = "Doctor", IsActive = false }, default));
        Assert.True(doctor.IsActive);
    }
    [Fact]
    public async Task Upgrade_adds_optional_fields_to_existing_schema_and_is_repeatable()
    {
        await using var db = fixture.Open();
        await Seed(db);
        var before = await db.Invoice.CountAsync();
        await db.Database.ExecuteSqlRawAsync("""
            ALTER TABLE poshc.ClinicSettings DROP CONSTRAINT FK_ClinicSettings_Doctor_DefaultDoctorId;
            IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_ClinicSettings_DefaultDoctorId' AND object_id=OBJECT_ID('poshc.ClinicSettings')) DROP INDEX IX_ClinicSettings_DefaultDoctorId ON poshc.ClinicSettings;
            IF OBJECT_ID('poshc.DF_ClinicSettings_SingleDoctorMode', 'D') IS NOT NULL ALTER TABLE poshc.ClinicSettings DROP CONSTRAINT DF_ClinicSettings_SingleDoctorMode;
            ALTER TABLE poshc.ClinicSettings DROP COLUMN DefaultDoctorId, SingleDoctorMode;
            ALTER TABLE poshc.Invoice DROP COLUMN VisitDescription, Diagnosis, VisitNotesUpdatedAt, VisitNotesUpdatedBy;
            DELETE FROM poshc.SchemaVersion WHERE Version=2;
            """);
        db.ChangeTracker.Clear();
        Assert.True(await SchemaUpgrade.IsRequired(db));
        await SchemaUpgrade.Apply(db);
        Assert.False(await SchemaUpgrade.IsRequired(db));
        await SchemaUpgrade.Apply(db);
        Assert.Equal(before, await db.Invoice.CountAsync());
        Assert.False((await db.ClinicSettings.SingleAsync()).SingleDoctorMode);
    }
}
