namespace EzyHouse.Api.Features.Balances;

public static class BalancesFeature
{
    public static IServiceCollection AddBalancesFeature(this IServiceCollection services)
    {
        services.AddScoped<ISettlementRepository, SettlementRepository>();
        services.AddScoped<IBalanceService, BalanceService>();
        return services;
    }
}
