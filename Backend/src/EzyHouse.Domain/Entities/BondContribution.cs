namespace EzyHouse.Domain.Entities;

public class BondContribution
{
    public int Id { get; set; }

    public int BondId { get; set; }

    public Bond? Bond { get; set; }

    public int UserId { get; set; }

    public User? User { get; set; }

    public decimal Amount { get; set; }

    public DateOnly Date { get; set; }
}
