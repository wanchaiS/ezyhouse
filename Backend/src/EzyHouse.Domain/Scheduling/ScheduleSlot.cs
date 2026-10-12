namespace EzyHouse.Domain.Scheduling;

public sealed record ScheduleSlot(int Step, DateOnly Start, DateOnly End);
