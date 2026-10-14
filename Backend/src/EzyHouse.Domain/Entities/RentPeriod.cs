using EzyHouse.Domain.Exceptions;
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

    public void SetSharePaid(int userId, bool paid, DateOnly today)
    {
        PaidShare share = Shares.SingleOrDefault(share => share.UserId == userId)
            ?? throw new NotFoundException("That person isn't in this rent period's split.");
        share.SetPaid(paid, today);
    }
}
