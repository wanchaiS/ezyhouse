using System.ComponentModel.DataAnnotations;

namespace EzyHouse.Api.Dtos;

public sealed record RegisterRequest(
    [property: Required, StringLength(100, MinimumLength = 1)] string? Name,
    [property: Required, EmailAddress, StringLength(256)] string? Email,
    [property: Required, MinLength(8), StringLength(100)] string? Password);

public sealed record LoginRequest(
    [property: Required, EmailAddress] string? Email,
    [property: Required] string? Password);

public sealed record AuthResultDto(string Token, UserDto User);
