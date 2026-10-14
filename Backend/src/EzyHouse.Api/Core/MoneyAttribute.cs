using System.ComponentModel.DataAnnotations;
using EzyHouse.Domain.Extensions;

namespace EzyHouse.Api.Infrastructure;

public sealed class MoneyAttribute : ValidationAttribute
{
    public bool AllowZero { get; set; }

    public override bool IsValid(object? value) => value switch
    {
        null => true,
        decimal amount => (AllowZero ? amount >= 0 : amount > 0) && amount.HasAtMostTwoDecimals(),
        _ => false,
    };

    public override string FormatErrorMessage(string name) =>
        AllowZero
            ? $"{name} must be 0 or greater, with at most 2 decimal places."
            : $"{name} must be greater than 0, with at most 2 decimal places.";
}
