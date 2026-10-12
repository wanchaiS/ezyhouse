using EzyHouse.Domain.Enums;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Domain.Interfaces;

public interface ISplitStrategy
{
    SplitType Type { get; }

    IReadOnlyList<Share> Split(decimal total, IReadOnlyList<ShareInput> shares);
}
