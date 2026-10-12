using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Domain.Entities;

public class RentPeriod
{
    public int Id { get; set; }

    public int ScheduleId { get; set; }

    public RentSchedule? Schedule { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public decimal Amount { get; set; }

    public int PayeeUserId { get; set; }

    public User? Payee { get; set; }

    public List<PaidShare> Shares { get; set; } = [];
}
