using System.ComponentModel.DataAnnotations;
using EzyHouse.Api.Infrastructure;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Api.Dtos;

public sealed record PurchaseDto(
    DateOnly Date,
    decimal Amount,
    UserRefDto PaidBy,
    SplitType SplitType,
    IReadOnlyList<ShareDto> Shares);

public sealed record SaleDto(
    DateOnly Date,
    decimal Amount,
    UserRefDto ReceivedBy,
    SplitType SplitType,
    IReadOnlyList<ShareDto> Shares);

public sealed record DisposalDto(
    DateOnly Date,
    decimal Amount,
    UserRefDto PaidBy,
    SplitType SplitType,
    IReadOnlyList<ShareDto> Shares);

public sealed record FurnitureDto(
    int Id,
    string Name,
    FurnitureStatus Status,
    UserRefDto CreatedBy,
    PurchaseDto Purchase,
    SaleDto? Sale,
    DisposalDto? Disposal);

public sealed record PurchaseInput(
    [property: Required] DateOnly? Date,
    [property: Required, Money] decimal? Amount,
    [property: Required] int? PaidByUserId,
    [property: Required] SplitType? SplitType,
    [property: Required, MinLength(1)] List<ShareInput>? Shares);

public sealed record FurnitureInput(
    [property: Required, StringLength(100, MinimumLength = 1)] string? Name,
    [property: Required] PurchaseInput? Purchase);

public sealed record SaleInput(
    [property: Required] DateOnly? Date,
    [property: Required, Money] decimal? Amount,
    [property: Required] int? ReceivedByUserId,
    [property: Required] SplitType? SplitType,
    [property: Required, MinLength(1)] List<ShareInput>? Shares);

public sealed record DisposalInput(
    [property: Required] DateOnly? Date,
    [property: Required, Money(AllowZero = true)] decimal? Amount,
    [property: Required] int? PaidByUserId,
    [property: Required] SplitType? SplitType,
    [property: Required, MinLength(1)] List<ShareInput>? Shares);
