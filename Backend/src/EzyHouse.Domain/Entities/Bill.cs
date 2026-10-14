using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Exceptions;
using EzyHouse.Domain.Interfaces;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Domain.Entities;

public class Bill : IHouseholdOwned
{
    public int Id { get; set; }

    /// <summary>
    /// Kept on the bill itself so a submitted bill can still be filtered to its household
    /// after the bill type is deleted and <see cref="BillTypeId"/> is set to null.
    /// </summary>
    public int HouseholdId { get; set; }

    public Household? Household { get; set; }

    public int? BillTypeId { get; set; }

    public BillType? BillType { get; set; }

    public string BillTypeName { get; set; } = "";

    public DateOnly DueDate { get; set; }

    public BillStatus Status { get; private set; } = BillStatus.Pending;

    public decimal? Amount { get; private set; }

    public int? PayeeUserId { get; private set; }

    public User? Payee { get; set; }

    public DateOnly? SubmittedDate { get; private set; }

    public List<PaidShare> Shares { get; set; } = [];

    /// <summary>The payee's own share is ticked straight away, since they paid the bill.</summary>
    public void Submit(decimal amount, IReadOnlyList<Share> shares, int payeeUserId, DateOnly today)
    {
        if (Status != BillStatus.Pending)
            throw new BusinessRuleException("This bill is already submitted; its amount can't be changed.");

        Amount = amount;
        PayeeUserId = payeeUserId;
        SubmittedDate = today;
        Status = BillStatus.Submitted;
        Shares = shares.Select(share => new PaidShare(share.UserId, share.Amount)).ToList();
        Shares.SingleOrDefault(share => share.UserId == payeeUserId)?.SetPaid(true, today);
    }

    public void SetSharePaid(int userId, bool paid, DateOnly today)
    {
        if (Status == BillStatus.Pending)
            throw new BusinessRuleException("A pending bill has no shares to tick; submit it first.");

        PaidShare share = Shares.SingleOrDefault(share => share.UserId == userId)
            ?? throw new NotFoundException("That person isn't in this bill's split.");
        share.SetPaid(paid, today);
    }
}
