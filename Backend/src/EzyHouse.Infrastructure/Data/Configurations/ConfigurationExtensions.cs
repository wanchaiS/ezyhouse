using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EzyHouse.Domain.ValueObjects;

namespace EzyHouse.Infrastructure.Data.Configurations;

internal static class ConfigurationExtensions
{
    public static PropertyBuilder<decimal> HasMoneyPrecision(this PropertyBuilder<decimal> property) =>
        property.HasPrecision(10, 2);

    public static PropertyBuilder<decimal?> HasMoneyPrecision(this PropertyBuilder<decimal?> property) =>
        property.HasPrecision(10, 2);

    public static PropertyBuilder<TEnum> HasEnumString<TEnum>(this PropertyBuilder<TEnum> property)
        where TEnum : struct, Enum =>
        property.HasConversion<string>().HasMaxLength(32);

    public static void ConfigureShares<TOwner>(
        this OwnedNavigationBuilder<TOwner, Share> shares,
        string table,
        string ownerId)
        where TOwner : class
    {
        shares.ToTable(table);
        shares.WithOwner().HasForeignKey(ownerId);
        shares.HasKey(ownerId, nameof(Share.UserId));
        shares.Property(share => share.Amount).HasMoneyPrecision();
        shares.HasOne(share => share.User)
            .WithMany()
            .HasForeignKey(share => share.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public static void ConfigurePaidShares<TOwner>(
        this OwnedNavigationBuilder<TOwner, PaidShare> shares,
        string table,
        string ownerId)
        where TOwner : class
    {
        shares.ToTable(table);
        shares.WithOwner().HasForeignKey(ownerId);
        shares.HasKey(ownerId, nameof(PaidShare.UserId));
        shares.Property(share => share.Amount).HasMoneyPrecision();
        shares.HasOne(share => share.User)
            .WithMany()
            .HasForeignKey(share => share.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
