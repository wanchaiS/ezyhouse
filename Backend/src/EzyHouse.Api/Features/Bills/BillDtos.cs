using System.ComponentModel.DataAnnotations;
using EzyHouse.Api.Core;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Api.Features.Bills;

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
    [Required, StringLength(100, MinimumLength = 1)] string? Name,
    [Required] Frequency? Frequency,
    [Required] DateOnly? StartDate,
    DateOnly? EndDate,
    [Required] List<int>? MemberIds);

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
    [Required, Money] decimal? Amount,
    [Required] SplitType? SplitType,
    List<ShareInput>? Shares);
