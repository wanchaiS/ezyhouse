using EzyHouse.Api.Infrastructure;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Exceptions;
using EzyHouse.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EzyHouse.Api.Services;

public sealed class CurrentUserContext(
    IHttpContextAccessor http,
    IRepository<User> users,
    IRepository<Household> households) : ICurrentUserContext
{
    private User? caller;

    public int UserId =>
        http.HttpContext?.User.GetUserId()
        ?? throw new ForbiddenException("You need to be signed in.");

    public async Task<User> GetUserAsync()
    {
        if (caller is not null)
            return caller;

        caller = await users.GetByIdAsync(UserId)
            ?? throw new NotFoundException("User was not found.");
        return caller;
    }

    public async Task<Household> RequireHouseholdAsync()
    {
        User user = await GetUserAsync();
        if (user.HouseholdId is not int householdId)
            throw new ForbiddenException("You need to be in a household to do that.");

        Household? household = await households.Query()
            .Include(item => item.Members)
            .Include(item => item.PrimaryUser)
            .FirstOrDefaultAsync(item => item.Id == householdId);

        return household ?? throw new ForbiddenException("You need to be in a household to do that.");
    }

    public async Task<Household> RequirePrimaryAsync()
    {
        Household household = await RequireHouseholdAsync();
        if (household.PrimaryUserId != UserId)
            throw new ForbiddenException("Only the primary housemate can do that.");

        return household;
    }
}
