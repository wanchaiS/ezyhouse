using EzyHouse.Api.Core;
using EzyHouse.Infrastructure.Data;
using Microsoft.Extensions.Logging.Abstractions;

namespace EzyHouse.Tests.Support;

public static class TestScheduleRunner
{
    /// <summary>The real <see cref="ScheduleRunner"/> on the test database, with "today" fixed.</summary>
    public static ScheduleRunner Create(EzyHouseDbContext db, DateOnly today) =>
        new(db, new FixedClock(today), NullLogger<ScheduleRunner>.Instance);
}
