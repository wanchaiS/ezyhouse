using System.ComponentModel.DataAnnotations;
using EzyHouse.Api.Core;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Api.Features.Expenses;

public sealed record ExpenseDto(
    int Id,
    string Description,
    decimal Amount,
    DateOnly Date,
    UserRefDto PaidBy,
    UserRefDto CreatedBy,
    SplitType SplitType,
    IReadOnlyList<ShareDto> Shares);

public sealed record ExpenseInput(
    [Required, StringLength(500, MinimumLength = 1, ErrorMessage = "Description must be at most 500 characters.")] string? Description,
    [Required, Money] decimal? Amount,
    [Required] DateOnly? Date,
    [Required] int? PaidByUserId,
    [Required] SplitType? SplitType,
    [Required, MinLength(1, ErrorMessage = "Choose at least one person for the split.")] List<ShareInput>? Shares);
