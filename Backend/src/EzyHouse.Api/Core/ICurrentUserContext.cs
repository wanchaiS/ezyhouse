using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Exceptions;

namespace EzyHouse.Api.Core;

/// <summary>
/// Reads the caller from the JWT <c>sub</c> claim and loads their user and household.
/// </summary>
public interface ICurrentUserContext
{
    /// <summary>Throws <see cref="UnauthorizedException"/> when nobody is signed in.</summary>
    int UserId { get; }

    /// <summary>Throws <see cref="UnauthorizedException"/> when the token's user no longer exists.</summary>
    Task<User> GetUserAsync();

    /// <summary>
    /// Throws <see cref="ForbiddenException"/> when the caller is not in a household.
    /// GET /api/household is the exception: it uses <see cref="GetUserAsync"/> and returns 404 instead.
    /// </summary>
    Task<Household> RequireHouseholdAsync();

    /// <summary>Throws <see cref="ForbiddenException"/> when the caller is not the primary.</summary>
    Task<Household> RequirePrimaryAsync();
}
