namespace EzyHouse.Domain.Entities;

public class Household
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string InviteCode { get; set; } = "";

    public int PrimaryUserId { get; set; }

    public User? PrimaryUser { get; set; }

    public List<User> Members { get; set; } = [];
}
