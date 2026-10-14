using System.ComponentModel.DataAnnotations;
using EzyHouse.Api.Infrastructure;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Api.Dtos;

public sealed record BillTypeRefDto(int? Id, string Name);

public sealed record BillTypeDto(
    int Id,
    string Name,
    Frequency Frequency,
    DateOnly StartDate,
    DateOnly? EndDate,
    IReadOnlyList<UserRefDto> Members,
    DateOnly? NextDueDate);

public sealed record BillTypeInput(
    [property: Required, StringLength(100, MinimumLength = 1)] string? Name,
    [property: Required] Frequency? Frequency,
    [property: Required] DateOnly? StartDate,
    DateOnly? EndDate,
    [property: Required] List<int>? MemberIds);

public sealed record BillDto(
    int Id,
    BillTypeRefDto BillType,
    DateOnly DueDate,
    BillStatus Status,
    decimal? Amount,
    UserRefDto? Payee,
    DateOnly? SubmittedDate,
    IReadOnlyList<PaidShareDto> Shares);

public sealed record SubmitBillInput(
    [property: Required, Money] decimal? Amount,
    [property: Required] SplitType? SplitType,
    List<ShareInput>? Shares);
