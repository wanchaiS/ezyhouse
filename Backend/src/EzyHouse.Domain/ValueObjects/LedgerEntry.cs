namespace EzyHouse.Domain.ValueObjects;

/// <summary>
/// One directed amount: <paramref name="DebtorId"/> owes <paramref name="CreditorId"/>.
/// </summary>
public sealed record LedgerEntry(int DebtorId, int CreditorId, decimal Amount);
