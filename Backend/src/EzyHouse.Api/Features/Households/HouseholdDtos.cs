using System.ComponentModel.DataAnnotations;
using EzyHouse.Api.Core;

namespace EzyHouse.Api.Features.Households;

public sealed record HouseholdDto(
    int Id,
    string Name,
    string InviteCode,
    UserRefDto Primary,
    IReadOnlyList<UserRefDto> Members);

public sealed record CreateHouseholdRequest(
    [Required, StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be at most 100 characters.")] string? Name);

public sealed record JoinHouseholdRequest(
    [Required, StringLength(8, MinimumLength = 8, ErrorMessage = "An invite code has 8 characters.")] string? InviteCode);

public sealed record TransferPrimaryRequest([Required] int? UserId);

/// <summary><see cref="NextPrimaryUserId"/> is needed only when the primary leaves while others remain.</summary>
public sealed record LeaveInput(int? NextPrimaryUserId);
