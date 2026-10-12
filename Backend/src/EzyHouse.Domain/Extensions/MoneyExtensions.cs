namespace EzyHouse.Domain.Extensions;

public static class MoneyExtensions
{
    /// <summary>
    /// Money has at most 2 decimal places. More decimals is a validation failure.
    /// </summary>
    public static bool HasAtMostTwoDecimals(this decimal value) => decimal.Round(value, 2) == value;
}
