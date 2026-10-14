namespace EzyHouse.Api.Features.Rent;

public static class RentFeature
{
    public static IServiceCollection AddRentFeature(this IServiceCollection services)
    {
        services.AddScoped<IRentRepository, RentRepository>();
        services.AddScoped<IRentService, RentService>();
        return services;
    }
}
