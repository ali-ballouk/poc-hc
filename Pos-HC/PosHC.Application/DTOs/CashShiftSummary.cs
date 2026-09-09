namespace PosHC.Application.DTOs;

public record CashShiftSummary(
    Guid Id, Guid UserId, string Currency, DateTime OpenedAt, DateTime? ClosedAt,
    decimal OpeningAmount, decimal? CountedAmount, decimal ExpectedAmount, decimal? Variance);
