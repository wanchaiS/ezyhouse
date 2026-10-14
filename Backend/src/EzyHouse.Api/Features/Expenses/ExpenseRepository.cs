using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Extensions;
using EzyHouse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EzyHouse.Api.Features.Expenses;

public sealed class ExpenseRepository(EzyHouseDbContext db) : IExpenseRepository
{
    public async Task<IReadOnlyList<Expense>> ListAsync(int householdId) =>
        await WithPeople(householdId)
            .OrderByDescending(expense => expense.Date)
            .ThenByDescending(expense => expense.Id)
            .ToListAsync();

    public Task<Expense?> FindAsync(int householdId, int id) =>
        WithPeople(householdId).FirstOrDefaultAsync(expense => expense.Id == id);

    public void Add(Expense expense) => db.Expenses.Add(expense);

    public void Remove(Expense expense) => db.Expenses.Remove(expense);

    public Task SaveChangesAsync() => db.SaveChangesAsync();

    private IQueryable<Expense> WithPeople(int householdId) =>
        db.Expenses
            .ForHousehold(householdId)
            .Include(expense => expense.PaidBy)
            .Include(expense => expense.CreatedBy)
            .Include(expense => expense.Shares).ThenInclude(share => share.User);
}
