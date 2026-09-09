using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;
namespace PosHC.Application.Services;
public class InvoiceService(BillingService billing, IClinicStore store) : IInvoiceService
{
    public async Task<InvoiceResultDto> SaveInvoiceAsync(CreateInvoiceDto dto, CancellationToken cancellationToken = default)
    {
        var invoice = await billing.Create(dto, cancellationToken);
        return new InvoiceResultDto { InvoiceId = invoice.Id, InvoiceDate = invoice.CreatedAt, Total = invoice.Total };
    }
    public async Task<List<InvoiceDto>> GetAllInvoicesDto(CancellationToken cancellationToken = default)
    {
        var invoices = await store.List<Invoice>(limit: 500, ct: cancellationToken);
        var result = new List<InvoiceDto>();
        foreach (var row in invoices)
        {
            var invoice = await billing.Invoice(row.Id, cancellationToken);
            result.Add(new InvoiceDto { InvoiceId = row.Id, InvoiceDate = row.CreatedAt, DoctorId = row.DoctorId, DoctorName = row.DoctorName, PatientId = row.PatientId, PatientName = row.PatientName, Discount = row.Discount, DoctorFee = row.DoctorFee, Total = invoice.Total });
        }
        return result.OrderByDescending(x => x.InvoiceDate).ToList();
    }
}
