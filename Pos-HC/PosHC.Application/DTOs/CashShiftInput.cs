namespace PosHC.Application.DTOs
{
    public record OpenShiftInput(string Currency, decimal OpeningAmount);
    public record CashMovementInput(decimal Amount, string Reason = "");
}
