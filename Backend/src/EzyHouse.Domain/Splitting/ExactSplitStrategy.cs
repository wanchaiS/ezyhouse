using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Exceptions;
using EzyHouse.Domain.Extensions;
using EzyHouse.Domain.Interfaces;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Domain.Splitting;

public sealed class ExactSplitStrategy : ISplitStrategy
{
    public SplitType Type => SplitType.Exact;

    /// <summary>Share amounts must add up to the total.</summary>
    public IReadOnlyList<Share> Split(decimal total, IReadOnlyList<ShareInput> shares)
    {
        if (shares.Any(share => share.Amount is null))
            throw new ValidationFailedException("Every share needs an amount when splitType is exact.");

        if (shares.Any(share => share.Amount!.Value < 0 || !share.Amount.Value.HasAtMostTwoDecimals()))
            throw new ValidationFailedException("Share amounts can't be negative or have more than 2 decimal places.");

        decimal sum = shares.Sum(share => share.Amount!.Value);
        if (sum != total)
            throw new ValidationFailedException($"Shares add up to {sum:0.00} but the total is {total:0.00}.");

        return shares.Select(share => new Share(share.UserId, share.Amount!.Value)).ToList();
    }
}
