using EzyHouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzyHouse.Infrastructure.Data.Configurations;

public sealed class BillTypeConfiguration : IEntityTypeConfiguration<BillType>
{
    public void Configure(EntityTypeBuilder<BillType> builder)
    {
        builder.ToTable("BillTypes");
        builder.Property(billType => billType.Name).HasMaxLength(100).IsRequired();
        builder.Property(billType => billType.Frequency).HasEnumString();
        builder.HasIndex(billType => billType.HouseholdId);

        builder.HasOne(billType => billType.Household)
            .WithMany()
            .HasForeignKey(billType => billType.HouseholdId)
            .OnDelete(DeleteBehavior.Restrict);

        // Users are never deleted, so a member link must block a user delete rather than vanish with it.
        builder.HasMany(billType => billType.Members)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "BillTypeMembers",
                member => member.HasOne<User>().WithMany().HasForeignKey("MembersId").OnDelete(DeleteBehavior.Restrict),
                billType => billType.HasOne<BillType>().WithMany().HasForeignKey("BillTypeId").OnDelete(DeleteBehavior.Cascade));

        builder.HasMany(billType => billType.Bills)
            .WithOne(bill => bill.BillType)
            .HasForeignKey(bill => bill.BillTypeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
