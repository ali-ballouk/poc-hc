using PosHC.Application.Exceptions;
using PosHC.Application.Mapping;
using PosHC.Application.DTOs;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;
using static PosHC.Application.Validation.BusinessRules;

namespace PosHC.Application.Services
{
    public class CashShiftService : ICashShiftService
    {
        private readonly IClinicStore _store;
        private readonly IAuditService _auditService;
        private readonly ICurrentStaff _staff;

        public CashShiftService(IClinicStore store, IAuditService auditService, ICurrentStaff staff)
        {
            _store = store;
            _auditService = auditService;
            _staff = staff;
        }

        public Task<CashShiftDetailsDto> OpenShift(string currency, decimal opening, CancellationToken cancellationToken)
        {
            return _store.Transaction(async () =>
            {
                Money(opening, currency);
                Check(opening >= 0, "Opening amount cannot be negative.");
                var existingShift = await _store.Find<CashShift>(shift =>
                    shift.UserId == _staff.Id && shift.Currency == currency && shift.ClosedAt == null, cancellationToken);
                Check(existingShift == null, "You already have an open shift in this currency.");

                var shift = new CashShift { UserId = _staff.Id, Currency = currency, OpeningAmount = opening };
                _store.Add(shift);
                _auditService.Record("Open", "CashShift", shift.Id);
                return ClinicDtoMapper.ToDto(shift);
            }, cancellationToken);
        }

        public Task<CashShiftDetailsDto> CloseShift(Guid id, decimal counted, CancellationToken cancellationToken)
        {
            return _store.Transaction(async () =>
            {
                var shift = await GetOwnOpenShiftAsync(id, cancellationToken);
                Money(counted, shift.Currency);
                Check(counted >= 0, "Counted amount cannot be negative.");

                shift.ExpectedAmount = await Expected(shift, cancellationToken);
                shift.CountedAmount = counted;
                shift.ClosedAt = DateTime.UtcNow;
                _auditService.Record("Close", "CashShift", id, $"Expected={shift.ExpectedAmount}; counted={counted}; {shift.Currency}");
                return ClinicDtoMapper.ToDto(shift);
            }, cancellationToken);
        }

        public Task<CashMovementDetailsDto> MoveCash(Guid id, decimal amount, string reason, CancellationToken cancellationToken)
        {
            return _store.Transaction(async () =>
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
                _store.Add(movement);
                _auditService.Record("Movement", "CashShift", id, $"{amount}: {movement.Reason}");
                return ClinicDtoMapper.ToDto(movement);
            }, cancellationToken);
        }

        public async Task<decimal> Expected(CashShift shift, CancellationToken cancellationToken)
        {
            var payments = await _store.List<Payment>(payment => payment.CashShiftId == shift.Id, int.MaxValue, ct: cancellationToken);
            var movements = await _store.List<CashMovement>(movement => movement.CashShiftId == shift.Id, int.MaxValue, ct: cancellationToken);
            return shift.OpeningAmount + payments.Sum(payment => payment.Amount) + movements.Sum(movement => movement.Amount);
        }

        public async Task<PagedResult<CashShiftSummary>> GetPageAsync(int page, CancellationToken cancellationToken, int pageSize = 50, string? sortBy = null, string? sortDirection = null)
        {
            var isAdministrator = _staff.Role == "Administrator";
            var userId = _staff.Id;
            var calculatedSort = sortBy is "ExpectedAmount" or "Variance";
            var shifts = await PagedQuery.ReadAsync<CashShift>(_store,
                shift => isAdministrator || shift.UserId == userId, page, cancellationToken, pageSize,
                calculatedSort ? null : sortBy, sortDirection);
            var source = calculatedSort ? await _store.List<CashShift>(shift => isAdministrator || shift.UserId == userId, int.MaxValue, ct: cancellationToken) : shifts.Items;
            var summaries = new List<CashShiftSummary>();

            foreach (var shift in source)
            {
                var expectedAmount = shift.ExpectedAmount ?? await Expected(shift, cancellationToken);
                summaries.Add(new CashShiftSummary(shift.Id, shift.UserId, shift.Currency,
                    shift.OpenedAt, shift.ClosedAt, shift.OpeningAmount, shift.CountedAmount,
                    expectedAmount, shift.CountedAmount - shift.ExpectedAmount));
            }

            if (calculatedSort)
            {
                summaries = GridOrdering.Apply(summaries.AsQueryable(), sortBy, sortDirection)
                    .Skip((shifts.Page - 1) * shifts.PageSize).Take(shifts.PageSize).ToList();
            }

            return new PagedResult<CashShiftSummary>(summaries, shifts.Total, shifts.PageSize, shifts.Page);
        }

        public async Task<List<CashMovementDetailsDto>> GetMovementsAsync(Guid id, CancellationToken cancellationToken)
        {
            var isAdministrator = _staff.Role == "Administrator";
            var userId = _staff.Id;
            var shift = await _store.Find<CashShift>(shift => shift.Id == id &&
                (isAdministrator || shift.UserId == userId), cancellationToken);
            if (shift == null)
            {
                throw new BusinessException("Cash shift not found.", 404);
            }

            var movements = await _store.List<CashMovement>(movement => movement.CashShiftId == id, int.MaxValue, ct: cancellationToken);
            return movements.Select(ClinicDtoMapper.ToDto).ToList();
        }

        private async Task<CashShift> GetOwnOpenShiftAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _store.Find<CashShift>(shift => shift.Id == id && shift.UserId == _staff.Id && shift.ClosedAt == null,
                cancellationToken) ?? throw new BusinessException("Your open shift was not found.");
        }
    }
}
