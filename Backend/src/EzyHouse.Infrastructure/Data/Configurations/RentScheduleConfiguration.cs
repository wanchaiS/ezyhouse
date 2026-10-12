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

        builder.HasOne(schedule => schedule.Household)
            .WithMany()
            .HasForeignKey(schedule => schedule.HouseholdId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(schedule => schedule.Periods)
            .WithOne(period => period.Schedule)
            .HasForeignKey(period => period.ScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsMany(schedule => schedule.Shares, shares =>
            shares.ConfigureShares("RentScheduleShares", "RentScheduleId"));
    }
}
