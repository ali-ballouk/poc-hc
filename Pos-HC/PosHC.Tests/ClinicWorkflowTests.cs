using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Application.Services;
using PosHC.Domain.Entities;
using PosHC.Infrastructure.Persistence;
using PosHC.Infrastructure.Repositories;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace PosHC.Tests;

public sealed class ClinicDatabase : IAsyncLifetime
{
    public string ConnectionString { get; private set; } = "";
    public Guid StaffId { get; } = Guid.NewGuid();
    public ApplicationDbContext Open() => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(ConnectionString).Options);
    public async Task InitializeAsync()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, "PosHCExternal.web")))
        {
            directory = directory.Parent;
        }

        var config = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(directory!.FullName, "PosHCExternal.web", "appsettings.json")));
        var source = Environment.GetEnvironmentVariable("POSHC_TEST_SQL") ?? config.RootElement.GetProperty("ConnectionStrings").GetProperty("DefaultConnection").GetString()!;
        var builder = new SqlConnectionStringBuilder(source) { InitialCatalog = "PosHC_Test_" + Guid.NewGuid().ToString("N") };
        ConnectionString = builder.ConnectionString;
        await using var db = Open();
        await SchemaUpgrade.Apply(db);
        await SchemaUpgrade.Apply(db);
        var settings = await db.ClinicSettings.SingleAsync();
        settings.LbpPerUsd = 89500;
        settings.ExchangeRateConfirmed = true;
        db.StaffUser.Add(new StaffUser { Id = StaffId, Username = "cashier", DisplayName = "Test cashier", Role = "Cashier", PasswordHash = "unused" });
        await db.SaveChangesAsync();
    }
    public async Task DisposeAsync()
    {
        if (!new SqlConnectionStringBuilder(ConnectionString).InitialCatalog.StartsWith("PosHC_Test_"))
        {
            throw new InvalidOperationException("Refusing to drop a non-test database.");
        }

        await using var db = Open();
        await db.Database.EnsureDeletedAsync();
    }
}
public class ClinicWorkflowTests(ClinicDatabase fixture) : IClassFixture<ClinicDatabase>
{
    private class Staff(Guid id) : ICurrentStaff
    {
        public Guid Id => id; public string Name => "test"; public string Role => "Administrator";
    }
    private record TestServices(BillingService Billing, ClinicSettingsService Settings, CashShiftService Shifts, DoctorService Doctors, AppointmentService Appointments, ClinicStore Store);

    private TestServices Services(ApplicationDbContext db)
    {
        var store = new ClinicStore(db);
        var staff = new Staff(fixture.StaffId);
        var audit = new AuditService(store, staff);
        var settings = new ClinicSettingsService(store, audit);
        var shifts = new CashShiftService(store, audit, staff);
        var doctors = new DoctorService(new POSHCRepository(db), store, audit);
        var appointments = new AppointmentService(store, audit);
        return new(new BillingService(store, settings, shifts, audit, staff), settings, shifts, doctors, appointments, store);
    }
    private async Task<CreateInvoiceDto> Input(ApplicationDbContext db, string currency = "USD")
    {
        var patient = new Patient { Id = Guid.NewGuid(), FirstName = "Test", LastName = Guid.NewGuid().ToString("N") };
        var doctor = new Doctor { Id = Guid.NewGuid(), FirstName = "Test", LastName = "Doctor", Fee = 20 };
        var item = new CatalogItem { Id = Guid.NewGuid(), Name = "Consultation", UnitPrice = 40, Type = ItemType.Service };
        db.AddRange(patient, doctor, item);
        await db.SaveChangesAsync();
        return new CreateInvoiceDto { PatientId = patient.Id, DoctorId = doctor.Id, Currency = currency, RequestId = Guid.NewGuid(), Items = [new() { CatalogItemId = item.Id, Quantity = 2 }] };
    }
    private static PaymentRequestDto Transfer(Guid id, decimal amount, Guid? request = null) => new() { InvoiceId = id, Amount = amount, PaymentTypeId = 3, RequestId = request ?? Guid.NewGuid(), Settings = new TransferPaymentSettings { Banke = "Test bank", ReferenceNumber = "TEST" } };
    [Fact]
    public async Task Invoice_snapshots_exchange_rate_and_duplicate_request_returns_same_total()
    {
        await using var db = fixture.Open();
        var services = Services(db);
        var billing = services.Billing;
        var store = services.Store;
        var input = await Input(db, "LBP");
        var invoice = await billing.Create(input, default);
        Assert.Equal(8950000, invoice.Total);
        var settings = await services.Settings.GetAsync(default);
        settings.LbpPerUsd = 90000;
        await store.Save();
        await using var second = fixture.Open();
        var repeated = await Services(second).Billing.Create(input, default);
        Assert.Equal(invoice.Id, repeated.Id);
        Assert.Equal(invoice.Total, repeated.Total);
        Assert.Equal(89500, repeated.ExchangeRate);
        settings.LbpPerUsd = 89500;
        await store.Save();
    }
    [Fact]
    public async Task Invalid_discount_is_rejected_without_saving_invoice()
    {
        await using var db = fixture.Open();
        var input = await Input(db);
        input.Discount = 101;
        var before = await db.Invoice.CountAsync();
        await Assert.ThrowsAsync<BusinessException>(() => Services(db).Billing.Create(input, default));
        Assert.Equal(before, await db.Invoice.CountAsync());
    }
    [Fact]
    public async Task Partial_payments_are_idempotent_and_cannot_overpay()
    {
        await using var db = fixture.Open();
        var billing = Services(db).Billing;
        var invoice = await billing.Create(await Input(db), default);
        var input = Transfer(invoice.Id, 40);
        var first = await billing.Collect(input, default);
        var repeated = await billing.Collect(input, default);
        Assert.Equal(first.Id, repeated.Id);
        Assert.Equal(60, (await billing.Balance(invoice.Id, default)).Balance);
        await Assert.ThrowsAsync<BusinessException>(() => billing.Collect(Transfer(invoice.Id, 61), default));
        await billing.Collect(Transfer(invoice.Id, 60), default);
        Assert.Equal("Paid", (await billing.Balance(invoice.Id, default)).PaymentStatus);
    }
    [Fact]
    public async Task Deferred_payment_does_not_settle_an_invoice()
    {
        await using var db = fixture.Open();
        var billing = Services(db).Billing;
        var invoice = await billing.Create(await Input(db), default);
        await billing.Collect(new()
        {
            InvoiceId = invoice.Id,
            PaymentTypeId = 4,
            Settings = new OnAccountPaymentSettings { AccountId = "test" }
        }, default);
        Assert.Equal(100, (await billing.Balance(invoice.Id, default)).Balance);
        Assert.Equal(0, await db.Payment.CountAsync(x => x.InvoiceId == invoice.Id));
    }
    [Fact]
    public async Task Refund_requires_credit_and_never_exceeds_refund_due()
    {
        await using var db = fixture.Open();
        var billing = Services(db).Billing;
        var invoice = await billing.Create(await Input(db), default);
        await billing.Collect(Transfer(invoice.Id, 100), default);
        await Assert.ThrowsAsync<BusinessException>(() => billing.Refund(invoice.Id, 10, 3, "Test", Guid.NewGuid(), default));
        await billing.Credit(invoice.Id, 30, "Correction", Guid.NewGuid(), default);
        await billing.Refund(invoice.Id, 30, 3, "External transfer refund", Guid.NewGuid(), default);
        Assert.Equal(0, (await billing.Balance(invoice.Id, default)).Balance);
        await Assert.ThrowsAsync<BusinessException>(() => billing.Refund(invoice.Id, 1, 3, "Test", Guid.NewGuid(), default));
    }
    [Fact]
    public async Task Cash_shift_reconciles_opening_collections_and_movements()
    {
        await using var db = fixture.Open();
        var services = Services(db);
        var billing = services.Billing;
        var clinic = services.Shifts;
        var shift = await clinic.OpenShift("USD", 50, default);
        var invoice = await billing.Create(await Input(db), default);
        await billing.Collect(new()
        {
            InvoiceId = invoice.Id,
            PaymentTypeId = 1,
            Amount = 100,
            RequestId = Guid.NewGuid(),
            Settings = new CashPaymentSettings { CashDrawerId = "test" }
        }, default);
        await clinic.MoveCash(shift.Id, -10, "Petty cash", default);
        var closed = await clinic.CloseShift(shift.Id, 139, default);
        Assert.Equal(140, closed.ExpectedAmount);
        Assert.Equal(-1, closed.CountedAmount - closed.ExpectedAmount);
        await Assert.ThrowsAsync<BusinessException>(() => clinic.MoveCash(shift.Id, 1, "Closed", default));
    }
    [Fact]
    public async Task Appointments_require_availability_and_reject_overlaps()
    {
        await using var db = fixture.Open();
        var input = await Input(db);
        var services = Services(db);
        var start = DateTime.UtcNow.Date.AddDays(10).AddHours(8);
        var appointment = new Appointment { DoctorId = input.DoctorId, PatientId = input.PatientId, StartsAt = start, EndsAt = start.AddMinutes(30) };
        await Assert.ThrowsAsync<BusinessException>(() => services.Appointments.SaveAsync(Guid.Empty, appointment, default));
        await services.Doctors.SaveAvailabilityAsync(new()
        {
            DoctorId = input.DoctorId,
            StartsAt = start,
            EndsAt = start.AddHours(8)
        }, default);
        await services.Appointments.SaveAsync(Guid.Empty, appointment, default);
        await Assert.ThrowsAsync<BusinessException>(() => services.Appointments.SaveAsync(Guid.Empty, appointment, default));
    }
    [Theory]
    [InlineData(1.5, "LBP")]
    [InlineData(1.001, "USD")]
    [InlineData(1, "EUR")]
    public void Invalid_currency_precision_is_rejected(decimal amount, string currency) => Assert.Throws<BusinessException>(() => PosHC.Application.Validation.BusinessRules.Money(amount, currency));
    [Fact]
    public async Task Receipts_and_reports_use_stored_invoice_currency()
    {
        await using var db = fixture.Open();
        var billing = Services(db).Billing;
        var invoice = await billing.Create(await Input(db), default);
        var payment = await billing.Collect(Transfer(invoice.Id, 100), default);
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        var bytes = new PosHC.Infrastructure.Pdf.ReceiptPdfGenerator().Generate(payment, invoice);
        Assert.StartsWith("%PDF", System.Text.Encoding.ASCII.GetString(bytes.Take(8).ToArray()));
        var report = JsonSerializer.SerializeToElement(await new ClinicReports(db).Summary(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), default));
        Assert.Equal(2, report.GetProperty("Summary").GetArrayLength());
        Assert.Contains(report.GetProperty("Summary").EnumerateArray(), row => row.GetProperty("Currency").GetString() == "USD" && row.GetProperty("Collected").GetDecimal() >= 100);
    }
    [Fact]
    public async Task Simultaneous_collections_cannot_exceed_invoice_total()
    {
        Guid id;
        await using (var db = fixture.Open())
        {
            id = (await Services(db).Billing.Create(await Input(db), default)).Id;
        }
        async Task Attempt()
        {
            await using var db = fixture.Open();
            try
            {
                await Services(db).Billing.Collect(Transfer(id, 80), default);
            }
            catch (BusinessException) { }
            catch (SqlException ex) when (ex.Number == 1205) { }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sql && sql.Number == 1205) { }
        }
        await Task.WhenAll(Attempt(), Attempt());
        await using var verify = fixture.Open();
        Assert.Equal(80, await verify.Payment.Where(x => x.InvoiceId == id).SumAsync(x => x.Amount));
    }
    [Fact]
    public async Task Domain_services_save_search_and_deactivate_master_data()
    {
        await using var db = fixture.Open();
        var services = Services(db);
        var repository = new POSHCRepository(db);
        var audit = new AuditService(services.Store, new Staff(fixture.StaffId));
        var patients = new PatientService(repository, services.Store, audit);
        var catalog = new CatalogItemService(repository, services.Store, audit);
        var uniqueName = Guid.NewGuid().ToString("N");

        var doctor = await services.Doctors.SaveAsync(Guid.Empty,
            new Doctor { FirstName = uniqueName, LastName = "Doctor", Fee = 25 }, default);
        var patient = await patients.SaveAsync(Guid.Empty,
            new Patient { FirstName = uniqueName, LastName = "Patient" }, default);
        var item = await catalog.SaveAsync(Guid.Empty,
            new CatalogItem { Name = uniqueName, UnitPrice = 40, Type = ItemType.Service }, default);

        Assert.Equal(doctor.Id, Assert.Single((await services.Doctors.GetPageAsync(uniqueName, 1, default)).Items).Id);
        Assert.Equal(patient.Id, Assert.Single((await patients.GetPageAsync(uniqueName, 1, default)).Items).Id);
        Assert.Equal(item.Id, Assert.Single((await catalog.GetPageAsync(uniqueName, 1, default)).Items).Id);

        doctor.IsActive = false;
        patient.IsActive = false;
        item.IsActive = false;
        await services.Doctors.SaveAsync(doctor.Id, doctor, default);
        await patients.SaveAsync(patient.Id, patient, default);
        await catalog.SaveAsync(item.Id, item, default);

        Assert.DoesNotContain(await services.Doctors.GetAllDoctorInfo(), entry => entry.Id == doctor.Id);
        Assert.DoesNotContain(await patients.GetAllPatientInfo(), entry => entry.Id == patient.Id);
        Assert.DoesNotContain(await catalog.GetAllCatalogItemsAsync(), entry => entry.Id == item.Id);
        var entityIds = new[] { doctor.Id.ToString(), patient.Id.ToString(), item.Id.ToString() };
        Assert.Equal(6, await db.Set<AuditEntry>().CountAsync(entry => entityIds.Contains(entry.EntityId)));
    }

    [Fact]
    public async Task Api_requires_authentication_csrf_and_correct_role()
    {
        await using var db = fixture.Open();
        var user = new StaffUser { Username = "api-reception", DisplayName = "Reception", Role = "Receptionist" };
        user.PasswordHash = new PasswordCodec().Hash(user, "Test-password-123!");
        db.Add(user);
        await db.SaveChangesAsync();
        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.UseEnvironment("Development").ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?> { { "ConnectionStrings:DefaultConnection", fixture.ConnectionString }, { "Setup:Token", new string('a', 64) } })));
        using var client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false
        });
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/clinic/patients")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/auth/login", new
        {
            Username = user.Username,
            Password = "Test-password-123!"
        })).StatusCode);
        async Task Csrf()
        {
            var response = await client.GetAsync("/api/auth/session");
            response.EnsureSuccessStatusCode();
            var cookie = response.Headers.GetValues("Set-Cookie").Single(x => x.StartsWith("XSRF-TOKEN="));
            client.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");
            client.DefaultRequestHeaders.Add("X-XSRF-TOKEN", Uri.UnescapeDataString(cookie.Split(';')[0]["XSRF-TOKEN=".Length..]));
        }
        await Csrf();
        (await client.PostAsJsonAsync("/api/auth/login", new
        {
            Username = user.Username,
            Password = "Test-password-123!"
        })).EnsureSuccessStatusCode();
        await Csrf();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/clinic/patients")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/clinic/staff")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/billing/payments", new
        {
        })).StatusCode);
        (await client.PostAsJsonAsync("/api/auth/logout", new
        {
        })).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/clinic/patients")).StatusCode);
    }
}
