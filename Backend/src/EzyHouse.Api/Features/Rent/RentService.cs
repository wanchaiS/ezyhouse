using EzyHouse.Api.Core;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Exceptions;
using EzyHouse.Domain.Interfaces;
using EzyHouse.Domain.Splitting;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Api.Features.Rent;

public sealed class RentService(
    IRentRepository rent,
    ICurrentUserContext currentUser,
    IScheduleRunner runner,
    SplitCalculator splitCalculator,
    IClock clock) : IRentService
{
    public async Task<IReadOnlyList<RentScheduleDto>> ListSchedulesAsync()
    {
        Household household = await currentUser.RequireHouseholdAsync();
        List<RentSchedule> schedules = await rent.ListSchedulesAsync(household.Id);
        return schedules.Select(schedule => schedule.ToDto()).ToList();
    }

    public async Task<RentScheduleDto> CreateScheduleAsync(RentScheduleInput request)
    {
        Household household = await currentUser.RequirePrimaryAsync();
        var schedule = new RentSchedule { HouseholdId = household.Id };
        Apply(schedule, request, household);

        // Starts at the first date on or after today; earlier dates are never created.
        schedule.Begin(clock.Today);
        rent.AddSchedule(schedule);
        await rent.SaveChangesAsync();

        // If that first date is today, its rent is created straight away rather than on the next hourly run.
        await runner.RunDueAsync(schedule, household.PrimaryUserId);
        return schedule.ToDto();
    }

    public async Task<RentScheduleDto> UpdateScheduleAsync(int id, RentScheduleInput request)
    {
        Household household = await currentUser.RequirePrimaryAsync();
        RentSchedule schedule = await rent.FindScheduleAsync(household.Id, id)
            ?? throw new NotFoundException("Rent schedule not found.");

        // Anything already due is created under the old settings first, so the edit only shapes what comes after.
        await runner.RunDueAsync(schedule, household.PrimaryUserId);

        // Rent already created keeps its own copies of amount and shares; only later rent changes.
        Apply(schedule, request, household);
        schedule.Reschedule(clock.Today);
        await rent.SaveChangesAsync();

        await runner.RunDueAsync(schedule, household.PrimaryUserId);
        return schedule.ToDto();
    }

    public async Task DeleteScheduleAsync(int id)
    {
        Household household = await currentUser.RequirePrimaryAsync();
        RentSchedule schedule = await rent.FindScheduleAsync(household.Id, id)
            ?? throw new NotFoundException("Rent schedule not found.");
        rent.RemoveSchedule(schedule);
        await rent.SaveChangesAsync();
    }

    private void Apply(RentSchedule schedule, RentScheduleInput request, Household household)
    {
        if (request.EndDate is { } end && end < request.StartDate!.Value)
            throw new ValidationFailedException("The end date can't be before the start date.");

        // Rent split among nobody couldn't be created, so the schedule would never produce anything.
        if (request.Shares!.Count == 0)
            throw new ValidationFailedException("Pick at least one person to share the rent.");

        HashSet<int> memberIds = household.Members.Select(member => member.Id).ToHashSet();
        IReadOnlyList<Share> shares = splitCalculator.Split(
            request.Amount!.Value, request.SplitType!.Value, request.Shares!, memberIds);

        // The members are already loaded, so the response can name everyone without another query.
        foreach (Share share in shares)
            share.User = household.Members.Single(member => member.Id == share.UserId);

        schedule.Amount = request.Amount.Value;
        schedule.Frequency = request.Frequency!.Value;
        schedule.StartDate = request.StartDate!.Value;
        schedule.EndDate = request.EndDate;
        schedule.SplitType = request.SplitType.Value;
        schedule.Shares = shares.ToList();
    }
}
