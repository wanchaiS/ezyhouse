using System.Security.Claims;
using EzyHouse.Api.Core;
using EzyHouse.Domain.Entities;
using EzyHouse.Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace EzyHouse.Tests.Support;

public static class TestUsers
{
    /// <summary>
    /// The real <see cref="CurrentUserContext"/>, as if <paramref name="user"/> had sent a valid token,
    /// so service tests exercise the same 401/403 rules the API uses.
    /// </summary>
    public static ICurrentUserContext SignIn(EzyHouseDbContext db, User user)
    {
        var identity = new ClaimsIdentity([new Claim("sub", user.Id.ToString())], authenticationType: "Test");
        var http = new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } };
        return new CurrentUserContext(http, db);
    }
}
