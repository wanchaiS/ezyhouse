using System.ComponentModel.DataAnnotations;
using EzyHouse.Api.Infrastructure;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Api.Dtos;

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
    [property: Required, StringLength(500, MinimumLength = 1)] string? Description,
    [property: Required, Money] decimal? Amount,
    [property: Required] DateOnly? Date,
    [property: Required] int? PaidByUserId,
    [property: Required] SplitType? SplitType,
    [property: Required, MinLength(1)] List<ShareInput>? Shares);
