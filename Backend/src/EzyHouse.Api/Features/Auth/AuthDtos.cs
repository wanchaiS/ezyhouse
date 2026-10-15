using System.ComponentModel.DataAnnotations;
using EzyHouse.Api.Core;

namespace EzyHouse.Api.Features.Auth;

public sealed record RegisterRequest(
    [Required, StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be at most 100 characters.")] string? Name,
    [Required, EmailAddress(ErrorMessage = "Enter a valid email address."), StringLength(256, ErrorMessage = "Email must be at most 256 characters.")] string? Email,
    [Required, MinLength(8, ErrorMessage = "Password must be at least 8 characters."), StringLength(100, ErrorMessage = "Password must be at most 100 characters.")] string? Password);

public sealed record LoginRequest(
    [Required, EmailAddress(ErrorMessage = "Enter a valid email address.")] string? Email,
    [Required] string? Password);

public sealed record AuthResultDto(string Token, UserDto User);
