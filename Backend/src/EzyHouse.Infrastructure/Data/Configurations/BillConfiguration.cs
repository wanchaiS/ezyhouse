using EzyHouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzyHouse.Infrastructure.Data.Configurations;

public sealed class BillConfiguration : IEntityTypeConfiguration<Bill>
{
    public void Configure(EntityTypeBuilder<Bill> builder)
    {
        builder.ToTable("Bills");
        builder.Property(bill => bill.BillTypeName).HasMaxLength(100).IsRequired();
        builder.Property(bill => bill.Status).HasEnumString();
        builder.Property(bill => bill.Amount).HasMoneyPrecision();
        builder.HasIndex(bill => bill.HouseholdId);
        builder.HasIndex(bill => new { bill.BillTypeId, bill.DueDate }).IsUnique();

        builder.HasOne(bill => bill.Household)
            .WithMany()
            .HasForeignKey(bill => bill.HouseholdId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(bill => bill.Payee)
            .WithMany()
            .HasForeignKey(bill => bill.PayeeUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsMany(bill => bill.Shares, shares => shares.ConfigurePaidShares("BillShares", "BillId"));
    }
}
