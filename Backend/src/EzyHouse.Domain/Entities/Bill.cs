using EzyHouse.Domain.Enums;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Domain.Entities;

public class Bill
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

    public BillStatus Status { get; set; }

    public decimal? Amount { get; set; }

    public int? PayeeUserId { get; set; }

    public User? Payee { get; set; }

    public DateOnly? SubmittedDate { get; set; }

    public List<PaidShare> Shares { get; set; } = [];
}
