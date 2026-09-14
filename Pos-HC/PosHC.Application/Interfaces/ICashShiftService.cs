using PosHC.Application.DTOs;
using PosHC.Domain.Entities;

namespace PosHC.Application.Interfaces;

public interface ICashShiftService
{
    Task<CashShift> OpenShift(string currency, decimal opening, CancellationToken cancellationToken);
    Task<CashShift> CloseShift(Guid id, decimal counted, CancellationToken cancellationToken);
    Task<CashMovement> MoveCash(Guid id, decimal amount, string reason, CancellationToken cancellationToken);
    Task<decimal> Expected(CashShift shift, CancellationToken cancellationToken);
    Task<PagedResult<CashShiftSummary>> GetPageAsync(int page, CancellationToken cancellationToken, int pageSize = 50, string? sortBy = null, string? sortDirection = null);
    Task<List<CashMovement>> GetMovementsAsync(Guid id, CancellationToken cancellationToken);
}
