using EzyHouse.Domain.Entities;

namespace EzyHouse.Domain.ValueObjects;

/// <summary>
/// A share that can be ticked as paid. Used by rent periods and bills.
/// </summary>
public class PaidShare
{
    public int UserId { get; set; }

    public decimal Amount { get; set; }

    public bool Paid { get; set; }

    public DateOnly? PaidDate { get; set; }

    public User? User { get; set; }

    public PaidShare()
    {
    }

    public PaidShare(int userId, decimal amount)
    {
        UserId = userId;
        Amount = amount;
    }

    public PaidShare(int userId, decimal amount, bool paid, DateOnly? paidDate)
        : this(userId, amount)
    {
        Paid = paid;
        PaidDate = paidDate;
    }
}
