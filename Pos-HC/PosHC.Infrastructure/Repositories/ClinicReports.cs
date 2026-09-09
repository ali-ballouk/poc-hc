using Microsoft.EntityFrameworkCore;
using PosHC.Application.Interfaces;
using PosHC.Infrastructure.Persistence;

namespace PosHC.Infrastructure.Repositories;
public class ClinicReports(ApplicationDbContext db) : IClinicReports
{
    public async Task<object> Summary(DateTime from, DateTime to, CancellationToken ct)
    {
        if (to <= from || to - from > TimeSpan.FromDays(366))
        {
            throw new BusinessException("Choose a report period of at most 366 days.");
        }

        var invoices = await db.Invoice.AsNoTracking().Include(x => x.Items).Where(x => x.CreatedAt >= from && x.CreatedAt < to && x.Status == "Issued").ToListAsync(ct);
        var payments = await db.Payment.AsNoTracking().Where(x => x.PaymentDate >= from && x.PaymentDate < to).ToListAsync(ct);
        var outstanding = await db.Invoice.AsNoTracking().Include(x => x.Items).Where(x => x.Status == "Issued").ToListAsync(ct);
        var balances = await db.Payment.AsNoTracking().GroupBy(x => x.InvoiceId).Select(g => new { Id = g.Key, Paid = g.Sum(x => x.Amount) }).ToDictionaryAsync(x => x.Id, x => x.Paid, ct);
        var credits = await db.CreditNote.AsNoTracking().GroupBy(x => x.InvoiceId).Select(g => new { Id = g.Key, Amount = g.Sum(x => x.Amount) }).ToDictionaryAsync(x => x.Id, x => x.Amount, ct);
        var periodCredits = await db.CreditNote.AsNoTracking().Where(x => x.CreatedAt >= from && x.CreatedAt < to).Join(db.Invoice, x => x.InvoiceId, i => i.Id, (c, i) => new { i.Currency, c.Amount }).ToListAsync(ct);
        var summary = new[] { "USD", "LBP" }.Select(currency => new { Currency = currency, InvoiceCount = invoices.Count(x => x.Currency == currency), Invoiced = invoices.Where(x => x.Currency == currency).Sum(x => x.Total), Discounts = invoices.Where(x => x.Currency == currency).Sum(x => x.Discount ?? 0), Tax = invoices.Where(x => x.Currency == currency).Sum(x => x.Tax), Collected = payments.Where(x => x.Currency == currency && x.Amount > 0).Sum(x => x.Amount), Refunded = -payments.Where(x => x.Currency == currency && x.Amount < 0).Sum(x => x.Amount), Credits = periodCredits.Where(x => x.Currency == currency).Sum(x => x.Amount), Outstanding = outstanding.Where(x => x.Currency == currency).Sum(x => Math.Max(0, x.Total - balances.GetValueOrDefault(x.Id) - credits.GetValueOrDefault(x.Id))) }).ToList();
        return new
        {
            From = from,
            To = to,
            Summary = summary,
            ByDoctor = invoices.GroupBy(x => new { x.DoctorId, x.DoctorName, x.Currency }).Select(g => new { g.Key.DoctorName, g.Key.Currency, Invoices = g.Count(), Billed = g.Sum(x => x.Total), ConsultationFees = g.Sum(x => x.DoctorFee) }),
            ByService = invoices.SelectMany(i => i.Items.Select(item => new { i.Currency, item.Name, item.Quantity, item.LineTotal })).GroupBy(x => new { x.Currency, x.Name }).Select(g => new { g.Key.Name, g.Key.Currency, Quantity = g.Sum(x => x.Quantity), GrossSales = g.Sum(x => x.LineTotal) }),
            ByPaymentMethod = payments.GroupBy(x => new { x.Currency, x.PaymentTypeId }).Select(g => new { g.Key.Currency, Method = g.Key.PaymentTypeId switch { 1 => "Cash", 2 => "Card terminal", 3 => "Bank transfer", _ => "Deferred" }, NetCollected = g.Sum(x => x.Amount) })
        };
    }
    public async Task<object> Export(CancellationToken ct) => new
    {
        ExportedAt = DateTime.UtcNow,
        Version = 1,
        Patients = await db.Patient.AsNoTracking().ToListAsync(ct),
        Doctors = await db.Doctor.AsNoTracking().ToListAsync(ct),
        Catalog = await db.CatalogItem.AsNoTracking().ToListAsync(ct),
        Invoices = await db.Invoice.AsNoTracking().ToListAsync(ct),
        InvoiceItems = await db.Set<PosHC.Domain.Entities.InvoiceItem>().AsNoTracking().ToListAsync(ct),
        Payments = await db.Payment.AsNoTracking().ToListAsync(ct),
        CreditNotes = await db.CreditNote.AsNoTracking().ToListAsync(ct),
        Appointments = await db.Appointment.AsNoTracking().ToListAsync(ct),
        Availability = await db.DoctorAvailability.AsNoTracking().ToListAsync(ct),
        CashShifts = await db.CashShift.AsNoTracking().ToListAsync(ct),
        CashMovements = await db.CashMovement.AsNoTracking().ToListAsync(ct),
        Settings = await db.ClinicSettings.AsNoTracking().ToListAsync(ct),
        Audit = await db.AuditEntry.AsNoTracking().ToListAsync(ct)
    };
}
