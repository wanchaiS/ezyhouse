namespace EzyHouse.Api.Features.Bonds;

public static class BondsFeature
{
    public static IServiceCollection AddBondsFeature(this IServiceCollection services)
    {
        services.AddScoped<IBondRepository, BondRepository>();
        services.AddScoped<IBondService, BondService>();
        return services;
    }
}
