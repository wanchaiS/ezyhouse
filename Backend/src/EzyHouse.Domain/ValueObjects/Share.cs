using EzyHouse.Domain.Entities;

namespace EzyHouse.Domain.ValueObjects;

/// <summary>
/// A calculated share of an expense or a rent schedule.
/// The amount is always the result of the split, including for equal splits.
/// </summary>
public class Share
{
    public int UserId { get; set; }

    public decimal Amount { get; set; }

    public User? User { get; set; }

    public Share()
    {
    }

    public Share(int userId, decimal amount)
    {
        UserId = userId;
        Amount = amount;
    }
}
