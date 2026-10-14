using System.ComponentModel.DataAnnotations;
using EzyHouse.Api.Core;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Api.Features.Rent;

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
    [Required, Money] decimal? Amount,
    [Required] Frequency? Frequency,
    [Required] DateOnly? StartDate,
    DateOnly? EndDate,
    [Required] SplitType? SplitType,
    [Required, MinLength(1)] List<ShareInput>? Shares);

public sealed record RentPeriodDto(
    int Id,
    int ScheduleId,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Amount,
    UserRefDto Payee,
    IReadOnlyList<PaidShareDto> Shares);
