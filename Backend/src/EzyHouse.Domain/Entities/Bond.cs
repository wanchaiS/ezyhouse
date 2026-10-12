namespace EzyHouse.Domain.Entities;

public class Bond
{
    public int Id { get; set; }

    public int HouseholdId { get; set; }

    public Household? Household { get; set; }

    public decimal Total { get; set; }

    public DateOnly PaidDate { get; set; }

    public List<BondContribution> Contributions { get; set; } = [];

    public decimal Outstanding => Total - Contributions.Sum(contribution => contribution.Amount);
}
