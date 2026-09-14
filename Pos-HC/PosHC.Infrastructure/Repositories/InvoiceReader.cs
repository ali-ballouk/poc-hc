using Microsoft.EntityFrameworkCore;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Application.Services;
using PosHC.Infrastructure.Persistence;

namespace PosHC.Infrastructure.Repositories
{
    public class InvoiceReader : IInvoiceReader
    {
        private readonly ApplicationDbContext _context;

        public InvoiceReader(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<InvoiceListRow>> GetPageAsync(string search, Guid? patientId,
            int page, int pageSize, string? sortBy, string? sortDirection, CancellationToken cancellationToken)
        {
            var invoices = _context.Invoice.AsNoTracking().Where(invoice =>
                (!patientId.HasValue || invoice.PatientId == patientId) &&
                (invoice.PatientName.Contains(search) || invoice.DoctorName.Contains(search) ||
                    invoice.Number.ToString().Contains(search)));
            var total = await invoices.CountAsync(cancellationToken);
            pageSize = Math.Clamp(pageSize, 1, 100);
            page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(total / (double)pageSize)));

            var rows = from invoice in invoices
                       let subtotal = invoice.DoctorFee + (invoice.Items.Sum(item => (decimal?)item.Quantity * item.UnitPrice) ?? 0)
                       let invoiceTotal = subtotal - (invoice.Discount ?? 0) +
                           Math.Round((subtotal - (invoice.Discount ?? 0)) * invoice.TaxRate / 100, invoice.Currency == "LBP" ? 0 : 2)
                       let paid = _context.Payment.Where(payment => payment.InvoiceId == invoice.Id).Sum(payment => (decimal?)payment.Amount) ?? 0
                       let credits = _context.CreditNote.Where(credit => credit.InvoiceId == invoice.Id).Sum(credit => (decimal?)credit.Amount) ?? 0
                       let balance = invoice.Status == "Void" ? 0 : invoiceTotal - credits - paid
                       select new InvoiceListRow
                       {
                           Id = invoice.Id,
                           Number = invoice.Number,
                           CreatedAt = invoice.CreatedAt,
                           PatientName = invoice.PatientName,
                           DoctorName = invoice.DoctorName,
                           Currency = invoice.Currency,
                           Status = invoice.Status,
                           Total = invoiceTotal,
                           Paid = paid,
                           Credits = credits,
                           Balance = balance,
                           PaymentStatus = invoice.Status == "Draft" || invoice.Status == "Void" ? invoice.Status :
                               balance < 0 ? "RefundDue" : balance == 0 ? "Paid" : paid > 0 ? "PartiallyPaid" : "Unpaid"
                       };

            var items = await GridOrdering.Apply(rows, sortBy, sortDirection)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            return new PagedResult<InvoiceListRow>(items, total, pageSize, page);
        }
    }
}
