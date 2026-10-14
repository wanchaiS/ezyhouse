using EzyHouse.Domain.Entities;

namespace EzyHouse.Domain.ValueObjects;

/// <summary>
/// A share that can be ticked as paid. Used by rent periods and bills.
/// </summary>
public class PaidShare
{
    public int UserId { get; set; }

    public decimal Amount { get; set; }

    public bool Paid { get; private set; }

    public DateOnly? PaidDate { get; private set; }

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

    public void SetPaid(bool paid, DateOnly today)
    {
        Paid = paid;
        PaidDate = paid ? today : null;
    }
}
