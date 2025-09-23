using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;

namespace PosHC.Application.Interfaces
{
    public interface IInvoiceService
    {
        Task<InvoiceResultDto> SaveInvoiceAsync(CreateInvoiceDto dto, CancellationToken cancellationToken = default);

        Task<List<InvoiceDto>> GetAllInvoicesDto(CancellationToken cancellationToken = default);

    }
}