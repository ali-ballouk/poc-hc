using PosHC.Application.DTOs;
using PosHC.Domain.Entities;

namespace PosHC.Application.Interfaces;

public interface ICashShiftService
{
    Task<CashShiftDetailsDto> OpenShift(string currency, decimal opening, CancellationToken cancellationToken);
    Task<CashShiftDetailsDto> CloseShift(Guid id, decimal counted, CancellationToken cancellationToken);
    Task<CashMovementDetailsDto> MoveCash(Guid id, decimal amount, string reason, CancellationToken cancellationToken);
    Task<decimal> Expected(CashShift shift, CancellationToken cancellationToken);
    Task<PagedResult<CashShiftSummary>> GetPageAsync(int page, CancellationToken cancellationToken, int pageSize = 50, string? sortBy = null, string? sortDirection = null);
    Task<List<CashMovementDetailsDto>> GetMovementsAsync(Guid id, CancellationToken cancellationToken);
}
