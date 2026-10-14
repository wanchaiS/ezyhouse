namespace EzyHouse.Api.Features.FurnitureItems;

public static class FurnitureFeature
{
    public static IServiceCollection AddFurnitureFeature(this IServiceCollection services)
    {
        services.AddScoped<IFurnitureRepository, FurnitureRepository>();
        services.AddScoped<IFurnitureService, FurnitureService>();
        return services;
    }
}
