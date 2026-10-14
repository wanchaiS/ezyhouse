using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Exceptions;
using EzyHouse.Domain.Interfaces;

namespace EzyHouse.Domain.Entities;

public class Furniture : IHouseholdOwned
{
    public int Id { get; set; }

    public int HouseholdId { get; set; }

    public Household? Household { get; set; }

    public string Name { get; set; } = "";

    public FurnitureStatus Status { get; private set; } = FurnitureStatus.Owned;

    public int CreatedByUserId { get; set; }

    public User? CreatedBy { get; set; }

    public List<FurnitureTransaction> Transactions { get; set; } = [];

    public FurniturePurchase Purchase => Transactions.OfType<FurniturePurchase>().Single();

    public FurnitureSale? Sale => Transactions.OfType<FurnitureSale>().SingleOrDefault();

    public FurnitureDisposal? Disposal => Transactions.OfType<FurnitureDisposal>().SingleOrDefault();

    public void RecordSale(FurnitureSale sale)
    {
        EnsureOwned("sell");
        Transactions.Add(sale);
        Status = FurnitureStatus.Sold;
    }

    public void RecordDisposal(FurnitureDisposal disposal)
    {
        EnsureOwned("dispose of");
        Transactions.Add(disposal);
        Status = FurnitureStatus.Disposed;
    }

    public void Edit(string name, FurniturePurchase purchase)
    {
        EnsureOwned("edit");
        Name = name;
        Transactions.Remove(Purchase);
        Transactions.Add(purchase);
    }

    private void EnsureOwned(string action)
    {
        if (Status != FurnitureStatus.Owned)
            throw new BusinessRuleException($"You can't {action} an item that is already {Status.ToString().ToLowerInvariant()}.");
    }
}
