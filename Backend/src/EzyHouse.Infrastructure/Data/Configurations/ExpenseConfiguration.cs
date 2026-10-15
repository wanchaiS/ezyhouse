using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzyHouse.Infrastructure.Data.Configurations;

/// <summary>
/// Every kind of expense lives in one Expenses table (table per hierarchy). The tag column says
/// which subclass a row is, so reading an expense back gives the right <c>CanBeChangedBy</c> rule.
/// </summary>
public sealed class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses");
        builder.HasDiscriminator(expense => expense.Tag)
            .HasValue<RentExpense>(ExpenseTag.Rent)
            .HasValue<BillExpense>(ExpenseTag.Bills)
            .HasValue<GroceryExpense>(ExpenseTag.Groceries)
            .HasValue<FurnitureExpense>(ExpenseTag.Furniture);
        builder.Property(expense => expense.Tag).HasEnumString();

        builder.Property(expense => expense.Description).HasMaxLength(500).IsRequired();
        builder.Property(expense => expense.Amount).HasMoneyPrecision();
        builder.Property(expense => expense.SplitType).HasEnumString();
        builder.HasIndex(expense => new { expense.HouseholdId, expense.Tag });

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
