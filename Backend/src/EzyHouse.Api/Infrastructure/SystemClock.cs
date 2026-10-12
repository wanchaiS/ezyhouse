using EzyHouse.Domain.Interfaces;

namespace EzyHouse.Api.Infrastructure;

public sealed class SystemClock : IClock
{
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Now);
}
