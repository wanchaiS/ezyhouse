using System.ComponentModel.DataAnnotations;
using EzyHouse.Api.Core;
using EzyHouse.Domain.Enums;

namespace EzyHouse.Api.Features.Bills;

public sealed record BillTypeDto(
    int Id,
    string Name,
    Frequency Frequency,
    DateOnly StartDate,
    DateOnly? EndDate,
    IReadOnlyList<UserRefDto> Members,
    DateOnly? NextDueDate);

/// <summary>An empty member list is checked by the service, so the message is the same however it is called.</summary>
public sealed record BillTypeInput(
    [Required, StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be at most 100 characters.")] string? Name,
    [Required] Frequency? Frequency,
    [Required] DateOnly? StartDate,
    DateOnly? EndDate,
    [Required] List<int>? MemberIds);
