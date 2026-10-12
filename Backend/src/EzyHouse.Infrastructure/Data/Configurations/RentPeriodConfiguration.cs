using EzyHouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EzyHouse.Infrastructure.Data.Configurations;

public sealed class RentPeriodConfiguration : IEntityTypeConfiguration<RentPeriod>
{
    public void Configure(EntityTypeBuilder<RentPeriod> builder)
    {
        builder.ToTable("RentPeriods");
        builder.Property(period => period.Amount).HasMoneyPrecision();
        builder.HasIndex(period => new { period.ScheduleId, period.StartDate }).IsUnique();

        builder.HasOne(period => period.Payee)
            .WithMany()
            .HasForeignKey(period => period.PayeeUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsMany(period => period.Shares, shares =>
            shares.ConfigurePaidShares("RentPeriodShares", "RentPeriodId"));
    }
}
