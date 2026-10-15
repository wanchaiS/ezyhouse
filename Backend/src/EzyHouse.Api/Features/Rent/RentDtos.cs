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

/// <summary>An empty share list is checked by the service, so the message is the same however it is called.</summary>
public sealed record RentScheduleInput(
    [Required, Money] decimal? Amount,
    [Required] Frequency? Frequency,
    [Required] DateOnly? StartDate,
    DateOnly? EndDate,
    [Required] SplitType? SplitType,
    [Required] List<ShareInput>? Shares);
