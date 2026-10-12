using System.Security.Claims;
using EzyHouse.Domain.Exceptions;

namespace EzyHouse.Api.Infrastructure;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        string? sub = principal.FindFirst("sub")?.Value;
        if (!int.TryParse(sub, out int userId))
            throw new ForbiddenException("You need to be signed in.");

        return userId;
    }
}
