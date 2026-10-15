using EzyHouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzyHouse.Infrastructure.Data.Configurations;

/// <summary>
/// A schedule creates at most one rent expense per date. Deleting the schedule keeps its expenses,
/// which stay in balances; the database just clears the link.
/// </summary>
public sealed class RentExpenseConfiguration : IEntityTypeConfiguration<RentExpense>
{
    public void Configure(EntityTypeBuilder<RentExpense> builder)
    {
        builder.HasIndex(expense => new { expense.RentScheduleId, expense.Date }).IsUnique();

        builder.HasOne<RentSchedule>()
            .WithMany()
            .HasForeignKey(expense => expense.RentScheduleId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
