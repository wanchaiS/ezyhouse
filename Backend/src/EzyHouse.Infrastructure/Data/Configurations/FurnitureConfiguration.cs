using EzyHouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzyHouse.Infrastructure.Data.Configurations;

public sealed class FurnitureConfiguration : IEntityTypeConfiguration<Furniture>
{
    public void Configure(EntityTypeBuilder<Furniture> builder)
    {
        builder.ToTable("Furniture");
        builder.Property(item => item.Name).HasMaxLength(100).IsRequired();
        builder.Property(item => item.Status).HasEnumString();
        builder.HasIndex(item => item.HouseholdId);
        builder.Ignore(item => item.Purchase);
        builder.Ignore(item => item.Sale);
        builder.Ignore(item => item.Disposal);

        builder.HasOne(item => item.Household)
            .WithMany()
            .HasForeignKey(item => item.HouseholdId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(item => item.CreatedBy)
            .WithMany()
            .HasForeignKey(item => item.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(item => item.Transactions)
            .WithOne(transaction => transaction.Furniture)
            .HasForeignKey(transaction => transaction.FurnitureId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
