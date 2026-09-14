using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Application.Services;
using PosHC.Domain.Entities;
using PosHC.Infrastructure.Persistence;
using PosHC.Infrastructure.Repositories;

namespace PosHC.Tests
{
    public class ArchitectureWorkflowTests : IClassFixture<ClinicDatabase>
    {
        private readonly ClinicDatabase _database;

        public ArchitectureWorkflowTests(ClinicDatabase database)
        {
            _database = database;
        }

        [Fact]
        public async Task Resource_routes_preserve_the_clinic_billing_and_reporting_workflow()
        {
            await using var db = _database.Open();
            var user = new StaffUser { Username = "architecture-admin", DisplayName = "Architecture admin", Role = "Administrator" };
            user.PasswordHash = new PasswordCodec().Hash(user, "Test-password-123!");
            db.Add(user);
            await db.SaveChangesAsync();
            await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
                builder.UseEnvironment("Development").ConfigureAppConfiguration((_, config) =>
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = _database.ConnectionString,
                        ["Setup:Token"] = new string('a', 64)
                    })));
            using var client = factory.CreateClient(new()
            {
                AllowAutoRedirect = false
            });
            await SetCsrfAsync(client);
            (await client.PostAsJsonAsync("/api/auth/login", new LoginInput(user.Username, "Test-password-123!"))).EnsureSuccessStatusCode();
            await SetCsrfAsync(client);

            var doctor = await PostAsync<DoctorDetailsDto>(client, "/api/doctor", new DoctorDetailsDto { FirstName = "Route", LastName = "Doctor", Fee = 20 });
            var patient = await PostAsync<PatientDetailsDto>(client, "/api/patient", new PatientDetailsDto { FirstName = "Route", LastName = "Patient" });
            var item = await PostAsync<CatalogItemDetailsDto>(client, "/api/catalogitem", new CatalogItemDetailsDto { Name = "Consultation", UnitPrice = 30, Type = ItemType.Service });
            var start = DateTime.UtcNow.Date.AddDays(2).AddHours(9);
            await PostAsync<DoctorAvailabilityDetailsDto>(client, "/api/doctor/availability", new DoctorAvailabilityDetailsDto { DoctorId = doctor.Id, StartsAt = start, EndsAt = start.AddHours(2) });
            var appointment = await PostAsync<AppointmentDetailsDto>(client, "/api/appointments", new AppointmentDetailsDto { DoctorId = doctor.Id, PatientId = patient.Id, StartsAt = start, EndsAt = start.AddMinutes(30) });
            appointment.Status = "Completed";
            (await client.PutAsJsonAsync($"/api/appointments/{appointment.Id}", appointment)).EnsureSuccessStatusCode();
            var clinic = (await client.GetFromJsonAsync<ClinicSettingsDetailsDto>("/api/clinic/settings"))!;
            clinic.Name = "Route clinic";
            (await client.PutAsJsonAsync("/api/clinic/settings", clinic)).EnsureSuccessStatusCode();
            Assert.Equal("Route clinic", (await client.GetFromJsonAsync<ClinicSettingsDetailsDto>("/api/clinic/settings"))!.Name);
            clinic.Address = new string('x', 501);
            Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsJsonAsync("/api/clinic/settings", clinic)).StatusCode);
            clinic.Address = "";
            clinic.Phone = new string('x', 51);
            Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsJsonAsync("/api/clinic/settings", clinic)).StatusCode);
            doctor.Phone = "123456";
            (await client.PutAsJsonAsync($"/api/doctor/{doctor.Id}", doctor)).EnsureSuccessStatusCode();
            patient.Address = "Updated address";
            (await client.PutAsJsonAsync($"/api/patient/{patient.Id}", patient)).EnsureSuccessStatusCode();
            (await client.PutAsJsonAsync($"/api/catalogitem/{item.Id}", item)).EnsureSuccessStatusCode();
            await PostAsync<CashShiftDetailsDto>(client, "/api/shifts", new OpenShiftInput("USD", 0));

            var invoiceIds = new List<Guid>();
            foreach (var quantity in new[] { 1, 2, 3 })
            {
                var invoice = await PostAsync<InvoiceResultDto>(client, "/api/invoice", new CreateInvoiceDto
                {
                    PatientId = patient.Id,
                    DoctorId = doctor.Id,
                    RequestId = Guid.NewGuid(),
                    VisitDescription = "Private visit description",
                    Diagnosis = "Private diagnosis",
                    Items = [new InvoiceItemDto { CatalogItemId = item.Id, Quantity = quantity }]
                });
                invoiceIds.Add(invoice.InvoiceId);
            }
            var id = invoiceIds[0];
            var payment = await PostAsync<PaymentDetailsDto>(client, "/api/payment", new PaymentRequestDto
            {
                InvoiceId = id,
                PaymentTypeId = 1,
                Amount = 10,
                RequestId = Guid.NewGuid(),
                Settings = new CashPaymentSettings()
            });
            await PostAsync<CreditNoteDetailsDto>(client, $"/api/invoice/{id}/credits", new AdjustmentInput(45, "Correction", Guid.NewGuid()));
            await PostAsync<PaymentDetailsDto>(client, $"/api/payment/invoice/{id}/refunds", new AdjustmentInput(5, "Cash refund", Guid.NewGuid()));

            var detailsResponse = await client.GetAsync($"/api/invoice/{id}");
            detailsResponse.EnsureSuccessStatusCode();
            var detailsText = await detailsResponse.Content.ReadAsStringAsync();
            Assert.DoesNotContain("Private", detailsText);
            Assert.DoesNotContain("Diagnosis", detailsText);
            var details = (await detailsResponse.Content.ReadFromJsonAsync<InvoiceHistoryDto>())!;
            Assert.Equal(0, details.Summary.Balance);
            Assert.Equal(2, details.Payments.Count);
            var visits = await client.GetFromJsonAsync<PagedResult<PatientVisitDto>>($"/api/patient/{patient.Id}/visits");
            Assert.Contains(visits!.Items, visit => visit.Diagnosis == "Private diagnosis");

            foreach (var sort in new[] { "Number", "Total", "Paid", "Credits", "Balance", "PaymentStatus" })
            {
                var response = await client.GetAsync($"/api/invoice?patientId={patient.Id}&page=1&pageSize=2&sortBy={sort}&sortDirection=desc");
                Assert.True(response.IsSuccessStatusCode, sort + ": " + await response.Content.ReadAsStringAsync());
                var page = (await response.Content.ReadFromJsonAsync<PagedResult<InvoiceListRow>>())!;
                Assert.Equal(3, page.Total);
                Assert.Equal(2, page.Items.Count);
                if (sort == "Total")
                {
                    Assert.Equal(new decimal[] { 110, 80 }, page.Items.Select(row => row.Total));
                }
            }

            foreach (var route in new[] { "/api/doctor", "/api/doctor/lookup", "/api/doctor/availability", "/api/patient", "/api/patient/lookup", "/api/catalogitem", "/api/catalogitem/lookup", "/api/paymenttype/lookup", "/api/invoice/lookup", "/api/shifts", "/api/appointments", "/api/audit", "/api/staff", "/api/clinic/settings" })
            {
                var response = await client.GetAsync(route);
                Assert.True(response.IsSuccessStatusCode, route + ": " + await response.Content.ReadAsStringAsync());
                Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
            }
            foreach (var route in new[] { $"/api/invoice/{id}/print?language=ar", $"/api/payment/{payment.Id}/receipt?language=en" })
            {
                var response = await client.GetAsync(route);
                response.EnsureSuccessStatusCode();
                Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
                Assert.StartsWith("%PDF", System.Text.Encoding.ASCII.GetString((await response.Content.ReadAsByteArrayAsync()).Take(8).ToArray()));
            }

            var report = await client.GetFromJsonAsync<ClinicReportDto>($"/api/reports?from={DateTime.UtcNow.AddDays(-1):yyyy-MM-dd}&to={DateTime.UtcNow.AddDays(1):yyyy-MM-dd}");
            Assert.True(report!.Summary.Single(row => row.Currency == "USD").Collected >= 10);
            var export = await client.GetAsync("/api/reports/export");
            export.EnsureSuccessStatusCode();
            Assert.DoesNotContain("Private diagnosis", await export.Content.ReadAsStringAsync());
            Assert.DoesNotContain("PasswordHash", await (await client.GetAsync("/api/staff")).Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/billing/invoices")).StatusCode);

            user.SecurityStamp = Guid.NewGuid().ToString();
            await db.SaveChangesAsync();
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/invoice")).StatusCode);
        }

        [Fact]
        public async Task Receipt_service_uses_current_clinic_settings_and_keeps_the_invoice_snapshot()
        {
            await using var db = _database.Open();
            var patient = new Patient { Id = Guid.NewGuid(), FirstName = "Receipt", LastName = "Patient" };
            var doctor = new Doctor { Id = Guid.NewGuid(), FirstName = "Receipt", LastName = "Doctor" };
            var store = new ClinicStore(db);
            var invoice = new Invoice { Id = Guid.NewGuid(), Number = await store.NextInvoiceNumber(), PatientId = patient.Id, DoctorId = doctor.Id, PatientName = "Receipt patient", ClinicName = "POS HC" };
            var payment = new Payment { Id = Guid.NewGuid(), InvoiceId = invoice.Id, Amount = 10, Currency = "USD", PaymentTypeId = 1, Settings = "{}" };
            db.AddRange(patient, doctor, invoice, payment);
            var settings = await db.ClinicSettings.SingleAsync();
            settings.Name = "Current clinic / العيادة الحالية";
            await db.SaveChangesAsync();
            var current = new CurrentStaff(_database.StaffId);
            var audit = new AuditService(store, current);
            var settingsService = new ClinicSettingsService(store, audit);
            var billing = new BillingService(store, settingsService, new CashShiftService(store, audit, current), audit, current);
            var generator = new CaptureReceipt();
            var service = new PaymentService(billing, store, settingsService, generator);
            await service.PrintReceiptAsync(payment.Id, "ar", default);
            Assert.Equal(settings.Name, generator.Receipt!.ClinicName);
            Assert.Equal("ar", generator.Language);
            Assert.Equal("POS HC", invoice.ClinicName);
            Assert.Null(await service.PrintReceiptAsync(Guid.NewGuid(), "en", default));
        }

        private static async Task<T> PostAsync<T>(HttpClient client, string route, object input)
        {
            var response = await client.PostAsJsonAsync(route, input);
            Assert.True(response.IsSuccessStatusCode, route + ": " + await response.Content.ReadAsStringAsync());
            return (await response.Content.ReadFromJsonAsync<T>())!;
        }

        [Theory]
        [InlineData("USD", 0.05, 1.05, 0.05)]
        [InlineData("LBP", 5, 105, 5)]
        public async Task Sql_invoice_totals_match_domain_rounding(string currency, decimal fee, decimal price, decimal discount)
        {
            await using var db = _database.Open();
            var patient = new Patient { Id = Guid.NewGuid(), FirstName = "Rounding", LastName = currency };
            var doctor = new Doctor { Id = Guid.NewGuid(), FirstName = "Rounding", LastName = "Doctor" };
            var item = new CatalogItem { Id = Guid.NewGuid(), Name = "Rounding", Type = ItemType.Service };
            var store = new ClinicStore(db);
            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                Number = await store.NextInvoiceNumber(),
                PatientId = patient.Id,
                DoctorId = doctor.Id,
                PatientName = patient.FirstName,
                DoctorFee = fee,
                Discount = discount,
                Currency = currency,
                TaxRate = 10
            };
            invoice.Items.Add(new InvoiceItem { Id = Guid.NewGuid(), InvoiceId = invoice.Id, CatalogItemId = item.Id, Quantity = 1, UnitPrice = price });
            db.AddRange(patient, doctor, item, invoice);
            await db.SaveChangesAsync();
            var page = await new InvoiceReader(db).GetPageAsync("", patient.Id, 1, 20, "Total", "desc", default);
            var row = Assert.Single(page.Items);
            Assert.Equal(invoice.Total, row.Total);
            Assert.Equal(invoice.Total, row.Balance);
        }

        private static async Task SetCsrfAsync(HttpClient client)
        {
            var response = await client.GetAsync("/api/auth/session");
            response.EnsureSuccessStatusCode();
            var cookie = response.Headers.GetValues("Set-Cookie").Single(value => value.StartsWith("XSRF-TOKEN="));
            client.DefaultRequestHeaders.Remove("X-XSRF-TOKEN");
            client.DefaultRequestHeaders.Add("X-XSRF-TOKEN", Uri.UnescapeDataString(cookie.Split(';')[0]["XSRF-TOKEN=".Length..]));
        }

        private sealed class CurrentStaff(Guid id) : ICurrentStaff
        {
            public Guid Id => id;
            public string Name => "Architecture test";
            public string Role => "Administrator";
        }

        private sealed class CaptureReceipt : IReceiptPdfGenerator
        {
            public ReceiptGenerateDto? Receipt
            {
                get; private set;
            }
            public string? Language
            {
                get; private set;
            }

            public byte[] Generate(ReceiptGenerateDto receipt, string language = "en")
            {
                Receipt = receipt;
                Language = language;
                return [];
            }
        }
    }
}
