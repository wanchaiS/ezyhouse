namespace EzyHouse.Api.Core;

/// <summary>
/// Creates every due rent and bill expense when the API starts, then once an hour. Starting with a run
/// catches up on dates that passed while the API was off; the hourly runs pick up each new day.
/// </summary>
public sealed class ScheduleBackgroundService(
    IServiceScopeFactory scopes,
    ILogger<ScheduleBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        do
        {
            await RunOnceAsync();
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunOnceAsync()
    {
        // A fresh scope per run: the runner uses the request-scoped DbContext, which must not live for hours.
        try
        {
            await using AsyncServiceScope scope = scopes.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<IScheduleRunner>().RunDueForEveryHouseholdAsync();
        }
        catch (Exception exception)
        {
            // A failed run (say, the database is briefly unreachable) must not stop the next one.
            logger.LogError(exception, "Creating due rent and bill expenses failed; trying again next hour.");
        }
    }
}
