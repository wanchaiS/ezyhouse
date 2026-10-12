using EzyHouse.Domain.Enums;

namespace EzyHouse.Domain.Entities;

public class Furniture
{
    public int Id { get; set; }

    public int HouseholdId { get; set; }

    public Household? Household { get; set; }

    public string Name { get; set; } = "";

    public FurnitureStatus Status { get; set; }

    public int CreatedByUserId { get; set; }

    public User? CreatedBy { get; set; }

    public List<FurnitureTransaction> Transactions { get; set; } = [];

    public FurniturePurchase Purchase => Transactions.OfType<FurniturePurchase>().Single();

    public FurnitureSale? Sale => Transactions.OfType<FurnitureSale>().SingleOrDefault();

    public FurnitureDisposal? Disposal => Transactions.OfType<FurnitureDisposal>().SingleOrDefault();
}
