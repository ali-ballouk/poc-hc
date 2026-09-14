using PosHC.Application.DTOs;

namespace PosHC.Application.Interfaces
{
    public interface IInvoiceReader
    {
        Task<PagedResult<InvoiceListRow>> GetPageAsync(string search, Guid? patientId, int page,
            int pageSize, string? sortBy, string? sortDirection, CancellationToken cancellationToken);
    }
}
