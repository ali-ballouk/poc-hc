using PosHC.Application.DTOs;

namespace PosHC.Application.Interfaces
{
    public interface IInvoiceForPrintService
    {
        Task<InvoiceGenerateDto?> GetInvoice(Guid id, CancellationToken cancellationToken);
    }
}
