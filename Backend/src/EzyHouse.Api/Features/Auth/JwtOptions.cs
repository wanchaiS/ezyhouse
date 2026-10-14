namespace EzyHouse.Api.Features.Auth;

/// <summary>The <c>Jwt</c> section of appsettings.json.</summary>
public sealed class JwtOptions
{
    public string Issuer { get; set; } = "";

    public string Audience { get; set; } = "";

    public string Key { get; set; } = "";

    public int ExpiryMinutes { get; set; } = 1440;
}
