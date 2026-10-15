using EzyHouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzyHouse.Infrastructure.Data.Configurations;

/// <summary>The same rules as rent, for the bills a bill type creates.</summary>
public sealed class BillExpenseConfiguration : IEntityTypeConfiguration<BillExpense>
{
    public void Configure(EntityTypeBuilder<BillExpense> builder)
    {
        builder.HasIndex(expense => new { expense.BillTypeId, expense.Date }).IsUnique();

        builder.HasOne<BillType>()
            .WithMany()
            .HasForeignKey(expense => expense.BillTypeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
