using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Interfaces;

namespace EzyHouse.Api.Features.Expenses;

public interface IExpenseRepository : IRepository
{
    /// <summary>Every tag when <paramref name="tag"/> is null; newest first.</summary>
    Task<IReadOnlyList<Expense>> ListAsync(int householdId, ExpenseTag? tag);

    Task<Expense?> FindAsync(int householdId, int id);

    void Add(Expense expense);

    void Remove(Expense expense);
}
