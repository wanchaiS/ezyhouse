using EzyHouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EzyHouse.Infrastructure.Data;

public class EzyHouseDbContext(DbContextOptions<EzyHouseDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Household> Households => Set<Household>();

    public DbSet<Expense> Expenses => Set<Expense>();

    public DbSet<Settlement> Settlements => Set<Settlement>();

    public DbSet<RentSchedule> RentSchedules => Set<RentSchedule>();

    public DbSet<RentPeriod> RentPeriods => Set<RentPeriod>();

    public DbSet<BillType> BillTypes => Set<BillType>();

    public DbSet<Bill> Bills => Set<Bill>();

    public DbSet<Furniture> Furniture => Set<Furniture>();

    public DbSet<FurnitureTransaction> FurnitureTransactions => Set<FurnitureTransaction>();

    public DbSet<Bond> Bonds => Set<Bond>();

    public DbSet<BondContribution> BondContributions => Set<BondContribution>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EzyHouseDbContext).Assembly);
    }
}
