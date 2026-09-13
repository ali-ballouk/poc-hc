using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;

namespace PosHCExternal.web.Controllers;

[ApiController, Route("api/clinic/shifts"), Authorize(Roles = "Administrator,Cashier")]
public class CashShiftController(ICashShiftService shifts) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<CashShiftSummary>> GetPage(int page = 1, CancellationToken ct = default, int pageSize = 50)
        => shifts.GetPageAsync(page, ct, pageSize);

    [HttpPost]
    public Task<CashShift> Open(OpenShiftInput input, CancellationToken ct)
        => shifts.OpenShift(input.Currency, input.OpeningAmount, ct);

    [HttpPost("{id:guid}/close")]
    public Task<CashShift> Close(Guid id, AmountInput input, CancellationToken ct)
        => shifts.CloseShift(id, input.Amount, ct);

    [HttpPost("{id:guid}/movements")]
    public Task<CashMovement> MoveCash(Guid id, AmountInput input, CancellationToken ct)
        => shifts.MoveCash(id, input.Amount, input.Reason, ct);

    [HttpGet("{id:guid}/movements")]
    public Task<List<CashMovement>> GetMovements(Guid id, CancellationToken ct)
        => shifts.GetMovementsAsync(id, ct);

    public record OpenShiftInput(string Currency, decimal OpeningAmount);
    public record AmountInput(decimal Amount, string Reason = "");
}
