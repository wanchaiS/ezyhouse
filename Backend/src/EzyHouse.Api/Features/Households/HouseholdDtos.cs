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
    [Required, StringLength(100, MinimumLength = 1)] string? Name);

public sealed record JoinHouseholdRequest(
    [Required, StringLength(8, MinimumLength = 8)] string? InviteCode);

public sealed record TransferPrimaryRequest([Required] int? UserId);
