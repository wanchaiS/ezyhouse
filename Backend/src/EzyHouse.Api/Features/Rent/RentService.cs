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
    IRecurringChargeGenerator generator,
    SplitCalculator splitCalculator,
    IClock clock) : IRentService
{
    public async Task<IReadOnlyList<RentScheduleDto>> ListSchedulesAsync()
    {
        Household household = await currentUser.RequireHouseholdAsync();
        List<RentSchedule> schedules = await rent.ListSchedulesAsync(household.Id);
        return schedules.Select(schedule => schedule.ToDto(clock.Today)).ToList();
    }

    public async Task<RentScheduleDto> CreateScheduleAsync(RentScheduleInput request)
    {
        Household household = await currentUser.RequirePrimaryAsync();
        var schedule = new RentSchedule { HouseholdId = household.Id };
        Apply(schedule, request, household);
        rent.AddSchedule(schedule);
        await rent.SaveChangesAsync();
        return schedule.ToDto(clock.Today);
    }

    public async Task<RentScheduleDto> UpdateScheduleAsync(int id, RentScheduleInput request)
    {
        Household household = await currentUser.RequirePrimaryAsync();
        RentSchedule schedule = await rent.FindScheduleAsync(household.Id, id)
            ?? throw new NotFoundException("Rent schedule not found.");

        // Existing periods keep their own copies of amount and shares; only later periods change.
        Apply(schedule, request, household);
        await rent.SaveChangesAsync();
        return schedule.ToDto(clock.Today);
    }

    public async Task DeleteScheduleAsync(int id)
    {
        Household household = await currentUser.RequirePrimaryAsync();
        RentSchedule schedule = await rent.FindScheduleAsync(household.Id, id)
            ?? throw new NotFoundException("Rent schedule not found.");
        rent.RemoveSchedule(schedule);
        await rent.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<RentPeriodDto>> ListPeriodsAsync()
    {
        Household household = await currentUser.RequireHouseholdAsync();
        await generator.GenerateRentPeriodsAsync(household, clock.Today);
        List<RentPeriod> periods = await rent.ListPeriodsAsync(household.Id);
        return periods.Select(period => period.ToDto()).ToList();
    }

    public async Task<RentPeriodDto> SetSharePaidAsync(int periodId, int userId, bool paid)
    {
        Household household = await currentUser.RequireHouseholdAsync();
        if (currentUser.UserId != userId && currentUser.UserId != household.PrimaryUserId)
            throw new ForbiddenException("Only that housemate or the primary can change this share.");

        RentPeriod period = await rent.FindPeriodAsync(household.Id, periodId)
            ?? throw new NotFoundException("Rent period not found.");
        period.SetSharePaid(userId, paid, clock.Today);
        await rent.SaveChangesAsync();
        return period.ToDto();
    }

    private void Apply(RentSchedule schedule, RentScheduleInput request, Household household)
    {
        if (request.EndDate is { } end && end < request.StartDate!.Value)
            throw new ValidationFailedException("The end date can't be before the start date.");

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
