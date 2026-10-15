using EzyHouse.Domain.Entities;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Api.Core;

public static class CoreMappings
{
    public static UserRefDto ToRef(this User user) => new(user.Id, user.Name);

    public static UserDto ToDto(this User user) => new(user.Id, user.Name, user.Email, user.HouseholdId);

    public static ShareDto ToDto(this Share share) => new(Ref(share.User, share.UserId), share.Amount);

    internal static UserRefDto Ref(User? user, int id) =>
        user?.ToRef() ?? throw new InvalidOperationException($"User {id} was not loaded.");
}
