using EzyHouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzyHouse.Infrastructure.Data.Configurations;

public sealed class SettlementConfiguration : IEntityTypeConfiguration<Settlement>
{
    public void Configure(EntityTypeBuilder<Settlement> builder)
    {
        builder.ToTable("Settlements");
        builder.Property(settlement => settlement.Amount).HasMoneyPrecision();
        builder.HasIndex(settlement => settlement.HouseholdId);

        builder.HasOne(settlement => settlement.Household)
            .WithMany()
            .HasForeignKey(settlement => settlement.HouseholdId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(settlement => settlement.From)
            .WithMany()
            .HasForeignKey(settlement => settlement.FromUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(settlement => settlement.To)
            .WithMany()
            .HasForeignKey(settlement => settlement.ToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(settlement => settlement.CreatedBy)
            .WithMany()
            .HasForeignKey(settlement => settlement.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
