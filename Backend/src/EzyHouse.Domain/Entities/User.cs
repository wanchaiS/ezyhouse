namespace EzyHouse.Domain.Entities;

public class User
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Email { get; set; } = "";

    public string PasswordHash { get; set; } = "";

    /// <summary>A user belongs to at most one household.</summary>
    public int? HouseholdId { get; set; }

    public Household? Household { get; set; }
}
