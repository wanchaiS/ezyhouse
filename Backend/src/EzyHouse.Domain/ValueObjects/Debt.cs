namespace EzyHouse.Domain.ValueObjects;

/// <summary>
/// A pairwise debt with no simplification. <paramref name="FromUserId"/> owes <paramref name="ToUserId"/>.
/// </summary>
public sealed record Debt(int FromUserId, int ToUserId, decimal Amount);
