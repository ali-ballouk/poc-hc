using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;

namespace PosHC.Application.Services
{
    public class InvoiceForPrintService : IInvoiceForPrintService
    {
        private readonly IPOSHCRepository _repository;
        private readonly IClinicSettingsService _settings;

        public InvoiceForPrintService(IPOSHCRepository repository, IClinicSettingsService settings)
        {
            _repository = repository;
            _settings = settings;
        }

        public async Task<InvoiceGenerateDto?> GetInvoice(Guid id, CancellationToken ct)
        {
            var inv = await _repository.GetInvoiceByIdAsync(id, ct);
            if (inv is null)
            {
                return null;
            }

            var clinic = await _settings.GetAsync(ct);
            return new InvoiceGenerateDto(
                inv.Id,
                inv.DoctorName,
                inv.PatientName,
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
                    )).ToList(), inv.Number, inv.Currency, inv.Status, inv.Tax, clinic.Name, inv.ClinicAddress, inv.ClinicPhone
            );
        }
    }
}
