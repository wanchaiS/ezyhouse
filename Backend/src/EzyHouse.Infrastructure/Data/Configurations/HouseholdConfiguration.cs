using EzyHouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzyHouse.Infrastructure.Data.Configurations;

public sealed class HouseholdConfiguration : IEntityTypeConfiguration<Household>
{
    public void Configure(EntityTypeBuilder<Household> builder)
    {
        builder.ToTable("Households");
        builder.Property(household => household.Name).HasMaxLength(100).IsRequired();
        builder.Property(household => household.InviteCode).HasMaxLength(8).IsRequired();
        builder.HasIndex(household => household.InviteCode).IsUnique();

        builder.HasOne(household => household.PrimaryUser)
            .WithMany()
            .HasForeignKey(household => household.PrimaryUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
