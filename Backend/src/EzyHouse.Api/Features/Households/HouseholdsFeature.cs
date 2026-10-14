namespace EzyHouse.Api.Features.Households;

public static class HouseholdsFeature
{
    public static IServiceCollection AddHouseholdsFeature(this IServiceCollection services)
    {
        services.AddScoped<IHouseholdRepository, HouseholdRepository>();
        services.AddScoped<IHouseholdService, HouseholdService>();
        return services;
    }
}
