using System.ComponentModel.DataAnnotations;

namespace EzyHouse.Api.Dtos;

public sealed record HouseholdDto(
    int Id,
    string Name,
    string InviteCode,
    UserRefDto Primary,
    IReadOnlyList<UserRefDto> Members);

public sealed record CreateHouseholdRequest(
    [property: Required, StringLength(100, MinimumLength = 1)] string? Name);

public sealed record JoinHouseholdRequest(
    [property: Required, StringLength(8, MinimumLength = 8)] string? InviteCode);

public sealed record TransferPrimaryRequest([property: Required] int? UserId);
