using EzyHouse.Domain.Interfaces;

namespace EzyHouse.Api.Core;

public sealed class SystemClock : IClock
{
    public DateOnly Today => DateOnly.FromDateTime(DateTime.Now);
}
