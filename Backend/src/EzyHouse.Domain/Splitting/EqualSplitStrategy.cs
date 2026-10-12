using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Interfaces;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Domain.Splitting;

public sealed class EqualSplitStrategy : ISplitStrategy
{
    public SplitType Type => SplitType.Equal;

    /// <summary>The server divides the amount. Leftover cents go to the first share.</summary>
    public IReadOnlyList<Share> Split(decimal total, IReadOnlyList<ShareInput> shares)
    {
        decimal each = Math.Floor(total * 100m / shares.Count) / 100m;
        decimal leftover = total - each * shares.Count;
        return shares
            .Select((share, index) => new Share(share.UserId, index == 0 ? each + leftover : each))
            .ToList();
    }
}
