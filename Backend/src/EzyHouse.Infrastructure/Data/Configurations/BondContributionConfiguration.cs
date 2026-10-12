using EzyHouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzyHouse.Infrastructure.Data.Configurations;

public sealed class BondContributionConfiguration : IEntityTypeConfiguration<BondContribution>
{
    public void Configure(EntityTypeBuilder<BondContribution> builder)
    {
        builder.ToTable("BondContributions");
        builder.Property(contribution => contribution.Amount).HasMoneyPrecision();

        builder.HasOne(contribution => contribution.User)
            .WithMany()
            .HasForeignKey(contribution => contribution.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
