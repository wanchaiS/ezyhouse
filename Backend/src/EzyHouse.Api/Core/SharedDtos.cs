using System.ComponentModel.DataAnnotations;

namespace EzyHouse.Api.Core;

public sealed record UserRefDto(int Id, string Name);

public sealed record UserDto(int Id, string Name, string Email, int? HouseholdId);

public sealed record ShareDto(UserRefDto User, decimal Amount);

public sealed record PaidShareDto(UserRefDto User, decimal Amount, bool Paid, DateOnly? PaidDate);

public sealed record PaidRequest([Required] bool? Paid);
