using PosHC.Application.DTOs;

namespace PosHC.Application.Interfaces
{
    public interface IInvoiceService
    {
        Task<InvoiceResultDto> SaveInvoiceAsync(CreateInvoiceDto input, CancellationToken cancellationToken = default);
        Task<List<InvoiceDto>> GetAllInvoicesDto(CancellationToken cancellationToken = default);
        Task<PagedResult<InvoiceListRow>> GetPageAsync(string search, Guid? patientId, int page,
            int pageSize, string? sortBy, string? sortDirection, CancellationToken cancellationToken);
        Task<InvoiceHistoryDto> GetDetailsAsync(Guid id, CancellationToken cancellationToken);
        Task<InvoiceDetailsDto> ChangeStatusAsync(Guid id, InvoiceStatusInput input, CancellationToken cancellationToken);
        Task<CreditNoteDetailsDto> CreditAsync(Guid id, AdjustmentInput input, CancellationToken cancellationToken);
        Task<byte[]?> PrintAsync(Guid id, string language, CancellationToken cancellationToken);
    }
}
