using EzyHouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzyHouse.Infrastructure.Data.Configurations;

public sealed class RentScheduleConfiguration : IEntityTypeConfiguration<RentSchedule>
{
    public void Configure(EntityTypeBuilder<RentSchedule> builder)
    {
        builder.ToTable("RentSchedules");
        builder.Property(schedule => schedule.Amount).HasMoneyPrecision();
        builder.Property(schedule => schedule.Frequency).HasEnumString();
        builder.Property(schedule => schedule.SplitType).HasEnumString();
        builder.HasIndex(schedule => schedule.HouseholdId);
        builder.HasIndex(schedule => schedule.NextDueDate);
        builder.Ignore(schedule => schedule.HasAnyoneToShare);

        builder.HasOne(schedule => schedule.Household)
            .WithMany()
            .HasForeignKey(schedule => schedule.HouseholdId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsMany(schedule => schedule.Shares, shares =>
            shares.ConfigureShares("RentScheduleShares", "RentScheduleId"));
    }
}
