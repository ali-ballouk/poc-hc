using PosHC.Application.Exceptions;
using PosHC.Application.Interfaces;

namespace PosHC.Application.Validation;

public static class BusinessRules
{
    public static string Required(string? value, string label, int max = 150)
    {
        value = value?.Trim();
        if (string.IsNullOrWhiteSpace(value) || value.Length > max)
        {
            throw new BusinessException($"{label} is required and must be at most {max} characters.");
        }
        return value;
    }

    public static void Check(bool condition, string message)
    {
        if (!condition)
        {
            throw new BusinessException(message);
        }
    }

    public static void Money(decimal amount, string currency)
    {
        Check(currency is "USD" or "LBP", "Currency must be USD or LBP.");
        var decimalPlaces = currency == "USD" ? 2 : 0;
        Check(Math.Abs(amount) <= 1_000_000_000_000m && decimal.Round(amount, decimalPlaces) == amount,
            "USD supports two decimal places; LBP uses whole lira.");
    }
}
