using System.ComponentModel.DataAnnotations;
using EzyHouse.Api.Core;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Api.Features.Expenses;

public sealed record ExpenseDto(
    int Id,
    ExpenseTag Tag,
    string Description,
    decimal Amount,
    DateOnly Date,
    UserRefDto PaidBy,
    UserRefDto CreatedBy,
    SplitType SplitType,
    IReadOnlyList<ShareDto> Shares,
    bool CanChange,
    int? RentScheduleId,
    DateOnly? PeriodEnd,
    int? BillTypeId);

/// <summary>An edit. The tag isn't part of it: an expense's tag never changes.</summary>
public sealed record ExpenseInput(
    [Required, StringLength(500, MinimumLength = 1, ErrorMessage = "Description must be at most 500 characters.")] string? Description,
    [Required, Money(AllowZero = true)] decimal? Amount,
    [Required] DateOnly? Date,
    [Required] int? PaidByUserId,
    [Required] SplitType? SplitType,
    [Required, MinLength(1, ErrorMessage = "Choose at least one person for the split.")] List<ShareInput>? Shares);

/// <summary>
/// A new expense: the same fields as <see cref="ExpenseInput"/> plus its tag. Written out in full because
/// ASP.NET reads validation attributes from the record's own constructor, not an inherited one.
/// </summary>
public sealed record NewExpenseInput(
    [Required] ExpenseTag? Tag,
    [Required, StringLength(500, MinimumLength = 1, ErrorMessage = "Description must be at most 500 characters.")] string? Description,
    [Required, Money(AllowZero = true)] decimal? Amount,
    [Required] DateOnly? Date,
    [Required] int? PaidByUserId,
    [Required] SplitType? SplitType,
    [Required, MinLength(1, ErrorMessage = "Choose at least one person for the split.")] List<ShareInput>? Shares)
{
    public ExpenseInput WithoutTag() => new(Description, Amount, Date, PaidByUserId, SplitType, Shares);
}
