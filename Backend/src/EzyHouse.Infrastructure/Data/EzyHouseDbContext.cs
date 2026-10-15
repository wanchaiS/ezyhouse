using EzyHouse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EzyHouse.Infrastructure.Data;

public class EzyHouseDbContext(DbContextOptions<EzyHouseDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Household> Households => Set<Household>();

    /// <summary>Every kind of expense, in one table; use <c>OfType&lt;RentExpense&gt;()</c> and so on for one kind.</summary>
    public DbSet<Expense> Expenses => Set<Expense>();

    public DbSet<Settlement> Settlements => Set<Settlement>();

    public DbSet<RentSchedule> RentSchedules => Set<RentSchedule>();

    public DbSet<BillType> BillTypes => Set<BillType>();

    public DbSet<Bond> Bonds => Set<Bond>();

    public DbSet<BondContribution> BondContributions => Set<BondContribution>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EzyHouseDbContext).Assembly);
    }
}
