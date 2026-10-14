using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Interfaces;

namespace EzyHouse.Api.Features.Expenses;

public interface IExpenseRepository : IRepository
{
    Task<IReadOnlyList<Expense>> ListAsync(int householdId);

    Task<Expense?> FindAsync(int householdId, int id);

    void Add(Expense expense);

    void Remove(Expense expense);
}
