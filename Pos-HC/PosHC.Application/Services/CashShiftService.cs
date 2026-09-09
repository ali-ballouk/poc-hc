using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;
using static PosHC.Application.Validation.BusinessRules;

namespace PosHC.Application.Services;

public class CashShiftService(IClinicStore store, IAuditService auditService, ICurrentStaff staff) : ICashShiftService
{
    public Task<CashShift> OpenShift(string currency, decimal opening, CancellationToken cancellationToken)
    {
        return store.Transaction(async () =>
        {
            Money(opening, currency);
            Check(opening >= 0, "Opening amount cannot be negative.");
            var existingShift = await store.Find<CashShift>(shift =>
                shift.UserId == staff.Id && shift.Currency == currency && shift.ClosedAt == null, cancellationToken);
            Check(existingShift == null, "You already have an open shift in this currency.");

            var shift = new CashShift { UserId = staff.Id, Currency = currency, OpeningAmount = opening };
            store.Add(shift);
            auditService.Record("Open", "CashShift", shift.Id);
            return shift;
        }, cancellationToken);
    }

    public Task<CashShift> CloseShift(Guid id, decimal counted, CancellationToken cancellationToken)
    {
        return store.Transaction(async () =>
        {
            var shift = await GetOwnOpenShiftAsync(id, cancellationToken);
            Money(counted, shift.Currency);
            Check(counted >= 0, "Counted amount cannot be negative.");

            shift.ExpectedAmount = await Expected(shift, cancellationToken);
            shift.CountedAmount = counted;
            shift.ClosedAt = DateTime.UtcNow;
            auditService.Record("Close", "CashShift", id, $"Expected={shift.ExpectedAmount}; counted={counted}; {shift.Currency}");
            return shift;
        }, cancellationToken);
    }

    public Task<CashMovement> MoveCash(Guid id, decimal amount, string reason, CancellationToken cancellationToken)
    {
        return store.Transaction(async () =>
        {
            var shift = await GetOwnOpenShiftAsync(id, cancellationToken);
            Money(amount, shift.Currency);
            Check(amount != 0, "Amount cannot be zero.");
            Check(await Expected(shift, cancellationToken) + amount >= 0,
                "Cash withdrawal exceeds the expected drawer balance.");

            var movement = new CashMovement
            {
                CashShiftId = id,
                Amount = amount,
                Reason = Required(reason, "Reason", 500)
            };
            store.Add(movement);
            auditService.Record("Movement", "CashShift", id, $"{amount}: {movement.Reason}");
            return movement;
        }, cancellationToken);
    }

    public async Task<decimal> Expected(CashShift shift, CancellationToken cancellationToken)
    {
        var payments = await store.List<Payment>(payment => payment.CashShiftId == shift.Id, int.MaxValue, ct: cancellationToken);
        var movements = await store.List<CashMovement>(movement => movement.CashShiftId == shift.Id, int.MaxValue, ct: cancellationToken);
        return shift.OpeningAmount + payments.Sum(payment => payment.Amount) + movements.Sum(movement => movement.Amount);
    }

    public async Task<PagedResult<CashShiftSummary>> GetPageAsync(int page, CancellationToken cancellationToken)
    {
        var isAdministrator = staff.Role == "Administrator";
        var userId = staff.Id;
        var shifts = await PagedQuery.ReadAsync<CashShift>(store,
            shift => isAdministrator || shift.UserId == userId, page, cancellationToken);
        var summaries = new List<CashShiftSummary>();

        foreach (var shift in shifts.Items)
        {
            var expectedAmount = shift.ExpectedAmount ?? await Expected(shift, cancellationToken);
            summaries.Add(new CashShiftSummary(shift.Id, shift.UserId, shift.Currency,
                shift.OpenedAt, shift.ClosedAt, shift.OpeningAmount, shift.CountedAmount,
                expectedAmount, shift.CountedAmount - shift.ExpectedAmount));
        }

        return new PagedResult<CashShiftSummary>(summaries, shifts.Total, shifts.PageSize);
    }

    public async Task<List<CashMovement>> GetMovementsAsync(Guid id, CancellationToken cancellationToken)
    {
        var isAdministrator = staff.Role == "Administrator";
        var userId = staff.Id;
        var shift = await store.Find<CashShift>(shift => shift.Id == id &&
            (isAdministrator || shift.UserId == userId), cancellationToken);
        if (shift == null)
        {
            throw new BusinessException("Cash shift not found.", 404);
        }

        return await store.List<CashMovement>(movement => movement.CashShiftId == id, int.MaxValue, ct: cancellationToken);
    }

    private async Task<CashShift> GetOwnOpenShiftAsync(Guid id, CancellationToken cancellationToken)
    {
        return await store.Find<CashShift>(shift => shift.Id == id && shift.UserId == staff.Id && shift.ClosedAt == null,
            cancellationToken) ?? throw new BusinessException("Your open shift was not found.");
    }
}
