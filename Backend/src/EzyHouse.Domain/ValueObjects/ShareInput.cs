namespace EzyHouse.Domain.ValueObjects;

/// <summary>
/// Request shape for a split. <see cref="Amount"/> is required only when the split is exact.
/// This value is never stored; the calculated <see cref="Share"/> is.
/// </summary>
public sealed record ShareInput(int UserId, decimal? Amount);
