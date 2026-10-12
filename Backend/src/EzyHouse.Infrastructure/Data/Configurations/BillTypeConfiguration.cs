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

        builder.HasMany(billType => billType.Members)
            .WithMany()
            .UsingEntity(join => join.ToTable("BillTypeMembers"));

        builder.HasMany(billType => billType.Bills)
            .WithOne(bill => bill.BillType)
            .HasForeignKey(bill => bill.BillTypeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
