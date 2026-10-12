namespace EzyHouse.Domain.Interfaces;

public interface IClock
{
    DateOnly Today { get; }
}
