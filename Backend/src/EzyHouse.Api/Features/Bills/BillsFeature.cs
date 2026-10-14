namespace EzyHouse.Api.Features.Bills;

public static class BillsFeature
{
    public static IServiceCollection AddBillsFeature(this IServiceCollection services)
    {
        services.AddScoped<IBillRepository, BillRepository>();
        services.AddScoped<IBillService, BillService>();
        return services;
    }
}
