namespace EzyHouse.Api.Features.Auth;

/// <summary>The HttpOnly cookie that carries the JWT for the browser app.</summary>
public static class AuthCookie
{
    public const string Name = "ezyhouse_token";

    /// <summary>
    /// Options for setting and for deleting the cookie. A delete only works if name, path,
    /// SameSite and Secure match the cookie that was set.
    /// </summary>
    public static CookieOptions Options(HttpRequest request, TimeSpan? maxAge = null) => new()
    {
        HttpOnly = true,
        SameSite = SameSiteMode.Strict,
        // Safari refuses Secure cookies on http://localhost, so only mark it Secure over HTTPS
        Secure = request.IsHttps,
        Path = "/",
        MaxAge = maxAge,
    };
}
