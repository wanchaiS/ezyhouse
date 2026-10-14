using EzyHouse.Domain.Interfaces;

namespace EzyHouse.Tests.Support;

/// <summary>A "today" that never moves, so date rules give the same answer on every run.</summary>
public sealed class FixedClock(DateOnly today) : IClock
{
    public DateOnly Today { get; } = today;
}
