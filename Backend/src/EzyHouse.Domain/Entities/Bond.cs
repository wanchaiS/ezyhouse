using EzyHouse.Domain.Exceptions;
using EzyHouse.Domain.Interfaces;

namespace EzyHouse.Domain.Entities;

public class Bond : IHouseholdOwned
{
    public int Id { get; set; }

    public int HouseholdId { get; set; }

    public Household? Household { get; set; }

    public decimal Total { get; private set; }

    public DateOnly PaidDate { get; private set; }

    public List<BondContribution> Contributions { get; private set; } = [];

    public decimal Outstanding => Total - Contributions.Sum(contribution => contribution.Amount);

    public void Update(decimal total, DateOnly paidDate, IReadOnlyList<BondContribution> contributions)
    {
        if (contributions.Sum(contribution => contribution.Amount) > total)
            throw new ValidationFailedException("Contributions can't add up to more than the bond total.");

        Total = total;
        PaidDate = paidDate;
        Contributions = contributions.ToList();
    }
}
