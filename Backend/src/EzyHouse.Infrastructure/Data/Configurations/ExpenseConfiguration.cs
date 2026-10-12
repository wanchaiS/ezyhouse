using EzyHouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzyHouse.Infrastructure.Data.Configurations;

public sealed class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses");
        builder.Property(expense => expense.Description).HasMaxLength(500).IsRequired();
        builder.Property(expense => expense.Amount).HasMoneyPrecision();
        builder.Property(expense => expense.SplitType).HasEnumString();
        builder.HasIndex(expense => expense.HouseholdId);

        builder.HasOne(expense => expense.Household)
            .WithMany()
            .HasForeignKey(expense => expense.HouseholdId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(expense => expense.PaidBy)
            .WithMany()
            .HasForeignKey(expense => expense.PaidByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(expense => expense.CreatedBy)
            .WithMany()
            .HasForeignKey(expense => expense.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsMany(expense => expense.Shares, shares => shares.ConfigureShares("ExpenseShares", "ExpenseId"));
    }
}
