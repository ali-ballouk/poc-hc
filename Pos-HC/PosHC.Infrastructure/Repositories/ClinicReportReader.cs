using Microsoft.EntityFrameworkCore;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;
using PosHC.Infrastructure.Persistence;

namespace PosHC.Infrastructure.Repositories
{
    public class ClinicReportReader : IClinicReportReader
    {
        private readonly ApplicationDbContext _context;

        public ClinicReportReader(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ClinicReportData> ReadAsync(DateTime from, DateTime to, CancellationToken cancellationToken)
        {
            var invoices = await _context.Invoice.AsNoTracking().Include(invoice => invoice.Items)
                .Where(invoice => invoice.CreatedAt >= from && invoice.CreatedAt < to && invoice.Status == "Issued").ToListAsync(cancellationToken);
            var payments = await _context.Payment.AsNoTracking()
                .Where(payment => payment.PaymentDate >= from && payment.PaymentDate < to).ToListAsync(cancellationToken);
            var outstanding = await _context.Invoice.AsNoTracking().Include(invoice => invoice.Items)
                .Where(invoice => invoice.Status == "Issued").ToListAsync(cancellationToken);
            var paid = await _context.Payment.AsNoTracking().GroupBy(payment => payment.InvoiceId)
                .Select(group => new { Id = group.Key, Amount = group.Sum(payment => payment.Amount) })
                .ToDictionaryAsync(row => row.Id, row => row.Amount, cancellationToken);
            var credits = await _context.CreditNote.AsNoTracking().GroupBy(credit => credit.InvoiceId)
                .Select(group => new { Id = group.Key, Amount = group.Sum(credit => credit.Amount) })
                .ToDictionaryAsync(row => row.Id, row => row.Amount, cancellationToken);
            var periodCredits = await _context.CreditNote.AsNoTracking()
                .Where(credit => credit.CreatedAt >= from && credit.CreatedAt < to)
                .Join(_context.Invoice, credit => credit.InvoiceId, invoice => invoice.Id,
                    (credit, invoice) => new CreditReportRow(invoice.Currency, credit.Amount)).ToListAsync(cancellationToken);
            return new ClinicReportData(invoices, payments, outstanding, paid, credits, periodCredits);
        }

        public async Task<ClinicExportData> ReadExportAsync(CancellationToken cancellationToken)
        {
            return new ClinicExportData
            {
                Patients = await _context.Set<Patient>().AsNoTracking().ToListAsync(cancellationToken),
                Doctors = await _context.Set<Doctor>().AsNoTracking().ToListAsync(cancellationToken),
                Catalog = await _context.Set<CatalogItem>().AsNoTracking().ToListAsync(cancellationToken),
                Invoices = await _context.Set<Invoice>().AsNoTracking().Include(invoice => invoice.Items).ToListAsync(cancellationToken),
                InvoiceItems = await _context.Set<InvoiceItem>().AsNoTracking().ToListAsync(cancellationToken),
                Payments = await _context.Set<Payment>().AsNoTracking().ToListAsync(cancellationToken),
                CreditNotes = await _context.Set<CreditNote>().AsNoTracking().ToListAsync(cancellationToken),
                Appointments = await _context.Set<Appointment>().AsNoTracking().ToListAsync(cancellationToken),
                Availability = await _context.Set<DoctorAvailability>().AsNoTracking().ToListAsync(cancellationToken),
                CashShifts = await _context.Set<CashShift>().AsNoTracking().ToListAsync(cancellationToken),
                CashMovements = await _context.Set<CashMovement>().AsNoTracking().ToListAsync(cancellationToken),
                Settings = await _context.Set<ClinicSettings>().AsNoTracking().ToListAsync(cancellationToken),
                Audit = await _context.Set<AuditEntry>().AsNoTracking().ToListAsync(cancellationToken)
            };
        }
    }
}
