using System.ComponentModel.DataAnnotations;
using EzyHouse.Domain.Extensions;

namespace EzyHouse.Api.Core;

public sealed class MoneyAttribute : ValidationAttribute
{
    // Amounts are stored in DECIMAL(10,2) columns.
    public const decimal Max = 99_999_999.99m;

    public bool AllowZero { get; set; }

    public override bool IsValid(object? value) => value switch
    {
        null => true,
        decimal amount => (AllowZero ? amount >= 0 : amount > 0) && amount <= Max && amount.HasAtMostTwoDecimals(),
        _ => false,
    };

    public override string FormatErrorMessage(string name) =>
        AllowZero
            ? $"{name} must be between 0 and {Max:N2}, with at most 2 decimal places."
            : $"{name} must be greater than 0 and at most {Max:N2}, with at most 2 decimal places.";
}
