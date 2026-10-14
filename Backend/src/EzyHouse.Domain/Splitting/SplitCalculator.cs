using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Exceptions;
using EzyHouse.Domain.Interfaces;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Domain.Splitting;

public sealed class SplitCalculator(IEnumerable<ISplitStrategy> strategies)
{
    private readonly Dictionary<SplitType, ISplitStrategy> byType = strategies.ToDictionary(strategy => strategy.Type);

    public IReadOnlyList<Share> Split(
        decimal total,
        SplitType type,
        IReadOnlyList<ShareInput> shares,
        IReadOnlySet<int> memberIds)
    {
        if (shares.Count == 0)
            throw new ValidationFailedException("At least one person must be in the split.");

        if (shares.Select(share => share.UserId).Distinct().Count() != shares.Count)
            throw new ValidationFailedException("A person can only appear once in a split.");

        if (shares.Any(share => !memberIds.Contains(share.UserId)))
            throw new ValidationFailedException("Everyone in a split must be a current household member.");

        if (!byType.TryGetValue(type, out ISplitStrategy? strategy))
            throw new ValidationFailedException($"Unknown split type '{type}'.");

        return strategy.Split(total, shares);
    }
}
