using System.ComponentModel.DataAnnotations;
using EzyHouse.Api.Core;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Api.Features.FurnitureItems;

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
    [Required] DateOnly? Date,
    [Required, Money] decimal? Amount,
    [Required] int? PaidByUserId,
    [Required] SplitType? SplitType,
    [Required, MinLength(1, ErrorMessage = "Choose at least one person for the split.")] List<ShareInput>? Shares);

public sealed record FurnitureInput(
    [Required, StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be at most 100 characters.")] string? Name,
    [Required] PurchaseInput? Purchase);

public sealed record SaleInput(
    [Required] DateOnly? Date,
    [Required, Money] decimal? Amount,
    [Required] int? ReceivedByUserId,
    [Required] SplitType? SplitType,
    [Required, MinLength(1, ErrorMessage = "Choose at least one person for the split.")] List<ShareInput>? Shares);

public sealed record DisposalInput(
    [Required] DateOnly? Date,
    [Required, Money(AllowZero = true)] decimal? Amount,
    [Required] int? PaidByUserId,
    [Required] SplitType? SplitType,
    [Required, MinLength(1, ErrorMessage = "Choose at least one person for the split.")] List<ShareInput>? Shares);
