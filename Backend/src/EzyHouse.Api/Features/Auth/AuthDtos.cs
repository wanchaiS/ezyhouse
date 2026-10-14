using System.ComponentModel.DataAnnotations;
using EzyHouse.Api.Core;

namespace EzyHouse.Api.Features.Auth;

public sealed record RegisterRequest(
    [Required, StringLength(100, MinimumLength = 1)] string? Name,
    [Required, EmailAddress, StringLength(256)] string? Email,
    [Required, MinLength(8), StringLength(100)] string? Password);

public sealed record LoginRequest(
    [Required, EmailAddress] string? Email,
    [Required] string? Password);

public sealed record AuthResultDto(string Token, UserDto User);
