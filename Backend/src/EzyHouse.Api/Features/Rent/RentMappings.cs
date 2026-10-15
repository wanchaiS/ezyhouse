using EzyHouse.Api.Core;
using EzyHouse.Domain.Entities;

namespace EzyHouse.Api.Features.Rent;

public static class RentMappings
{
    public static RentScheduleDto ToDto(this RentSchedule schedule) => new(
        schedule.Id,
        schedule.Amount,
        schedule.Frequency,
        schedule.StartDate,
        schedule.EndDate,
        schedule.SplitType,
        schedule.Shares.Select(share => share.ToDto()).ToList(),
        schedule.NextDueDate);
}
