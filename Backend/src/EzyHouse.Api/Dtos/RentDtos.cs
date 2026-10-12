using System.ComponentModel.DataAnnotations;
using EzyHouse.Api.Infrastructure;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Api.Dtos;

public sealed record RentScheduleDto(
    int Id,
    decimal Amount,
    Frequency Frequency,
    DateOnly StartDate,
    DateOnly? EndDate,
    SplitType SplitType,
    IReadOnlyList<ShareDto> Shares,
    DateOnly? NextDueDate);

public sealed record RentScheduleInput(
    [property: Required, Money] decimal? Amount,
    [property: Required] Frequency? Frequency,
    [property: Required] DateOnly? StartDate,
    DateOnly? EndDate,
    [property: Required] SplitType? SplitType,
    [property: Required, MinLength(1)] List<ShareInput>? Shares);

public sealed record RentPeriodDto(
    int Id,
    int ScheduleId,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Amount,
    UserRefDto Payee,
    IReadOnlyList<PaidShareDto> Shares);

public sealed record PaidRequest([property: Required] bool? Paid);
