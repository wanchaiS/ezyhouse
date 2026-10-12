using EzyHouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzyHouse.Infrastructure.Data.Configurations;

public sealed class FurnitureTransactionConfiguration : IEntityTypeConfiguration<FurnitureTransaction>
{
    public void Configure(EntityTypeBuilder<FurnitureTransaction> builder)
    {
        builder.ToTable("FurnitureTransactions");
        builder.HasDiscriminator<string>("Kind")
            .HasValue<FurniturePurchase>("Purchase")
            .HasValue<FurnitureSale>("Sale")
            .HasValue<FurnitureDisposal>("Disposal");
        builder.Property("Kind").HasMaxLength(32);
        builder.HasIndex("FurnitureId", "Kind").IsUnique();

        builder.Property(transaction => transaction.Amount).HasMoneyPrecision();
        builder.Property(transaction => transaction.SplitType).HasEnumString();

        builder.HasOne(transaction => transaction.Counterparty)
            .WithMany()
            .HasForeignKey(transaction => transaction.CounterpartyUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsMany(transaction => transaction.Shares, shares =>
            shares.ConfigureShares("FurnitureTransactionShares", "FurnitureTransactionId"));
    }
}
