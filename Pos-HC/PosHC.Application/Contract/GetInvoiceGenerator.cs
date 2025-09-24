using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;

namespace PosHC.Application.Invoices.Queries
{
    public class InvoiceForPrintService
    {
        private readonly IPOSHCRepository _repo;

        public InvoiceForPrintService(IPOSHCRepository repo)
        {
            _repo = repo;
        }

        public async Task<InvoiceGenerateDto?> GetInvoice(Guid id, CancellationToken ct)
        {
            var inv = await _repo.GetInvoiceByIdAsync(id, ct);
            if (inv is null) return null;

            return new InvoiceGenerateDto(
                inv.Id,
                inv.Doctor?.FirstName + " " + inv.Doctor?.LastName,
                inv.Patient?.FirstName + " " + inv.Patient?.LastName,
                inv.DoctorFee,
                inv.Discount,
                inv.CreatedAt,
                inv.Subtotal,
                inv.Total,
                inv.Items.Select(i =>
                    new InvoiceItemGenerateDto(
                        i.Id,
                        i.Name,
                        i.Quantity,
                        i.UnitPrice,
                        i.Quantity * i.UnitPrice
                    )).ToList()
            );
        }
    }
}
