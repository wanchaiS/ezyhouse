namespace EzyHouse.Api.Features.Expenses;

public static class ExpensesFeature
{
    public static IServiceCollection AddExpensesFeature(this IServiceCollection services)
    {
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IExpenseService, ExpenseService>();
        return services;
    }
}
