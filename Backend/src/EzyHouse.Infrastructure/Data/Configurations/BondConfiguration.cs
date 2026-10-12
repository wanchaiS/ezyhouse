using EzyHouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzyHouse.Infrastructure.Data.Configurations;

public sealed class BondConfiguration : IEntityTypeConfiguration<Bond>
{
    public void Configure(EntityTypeBuilder<Bond> builder)
    {
        builder.ToTable("Bonds");
        builder.Property(bond => bond.Total).HasMoneyPrecision();
        builder.HasIndex(bond => bond.HouseholdId);
        builder.Ignore(bond => bond.Outstanding);

        builder.HasOne(bond => bond.Household)
            .WithMany()
            .HasForeignKey(bond => bond.HouseholdId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(bond => bond.Contributions)
            .WithOne(contribution => contribution.Bond)
            .HasForeignKey(contribution => contribution.BondId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
