using EzyHouse.Api.Core;
using EzyHouse.Domain.Entities;

namespace EzyHouse.Api.Features.Rent;

public static class RentMappings
{
    public static RentScheduleDto ToDto(this RentSchedule schedule, DateOnly today) => new(
        schedule.Id,
        schedule.Amount,
        schedule.Frequency,
        schedule.StartDate,
        schedule.EndDate,
        schedule.SplitType,
        schedule.Shares.Select(share => share.ToDto()).ToList(),
        schedule.GetNextDueDate(today));

    public static RentPeriodDto ToDto(this RentPeriod period) => new(
        period.Id,
        period.ScheduleId,
        period.StartDate,
        period.EndDate,
        period.Amount,
        CoreMappings.Ref(period.Payee, period.PayeeUserId),
        period.Shares.Select(share => share.ToDto()).ToList());
}
