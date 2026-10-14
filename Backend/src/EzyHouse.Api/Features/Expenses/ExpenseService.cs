using EzyHouse.Api.Core;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Exceptions;
using EzyHouse.Domain.Splitting;

namespace EzyHouse.Api.Features.Expenses;

public sealed class ExpenseService(
    IExpenseRepository expenses,
    ICurrentUserContext currentUser,
    SplitCalculator splitCalculator) : IExpenseService
{
    public async Task<IReadOnlyList<ExpenseDto>> ListAsync()
    {
        Household household = await currentUser.RequireHouseholdAsync();
        IReadOnlyList<Expense> list = await expenses.ListAsync(household.Id);
        return list.Select(expense => expense.ToDto()).ToList();
    }

    public async Task<ExpenseDto> GetAsync(int id)
    {
        Household household = await currentUser.RequireHouseholdAsync();
        return (await LoadAsync(household.Id, id)).ToDto();
    }

    public async Task<ExpenseDto> CreateAsync(ExpenseInput request)
    {
        Household household = await currentUser.RequireHouseholdAsync();
        var expense = new Expense { HouseholdId = household.Id, CreatedByUserId = currentUser.UserId };
        Apply(expense, request, household);
        expenses.Add(expense);
        await expenses.SaveChangesAsync();
        return (await LoadAsync(household.Id, expense.Id)).ToDto();
    }

    public async Task<ExpenseDto> UpdateAsync(int id, ExpenseInput request)
    {
        Household household = await currentUser.RequireHouseholdAsync();
        Expense expense = await LoadOwnAsync(household.Id, id);
        Apply(expense, request, household);
        await expenses.SaveChangesAsync();
        return (await LoadAsync(household.Id, id)).ToDto();
    }

    public async Task DeleteAsync(int id)
    {
        Household household = await currentUser.RequireHouseholdAsync();
        Expense expense = await LoadOwnAsync(household.Id, id);
        expenses.Remove(expense);
        await expenses.SaveChangesAsync();
    }

    private async Task<Expense> LoadAsync(int householdId, int id) =>
        await expenses.FindAsync(householdId, id) ?? throw new NotFoundException("Expense not found.");

    private async Task<Expense> LoadOwnAsync(int householdId, int id)
    {
        Expense expense = await LoadAsync(householdId, id);
        if (expense.CreatedByUserId != currentUser.UserId)
            throw new ForbiddenException("Only the person who added this expense can change it.");
        return expense;
    }

    // Model validation guarantees the required fields are present.
    private void Apply(Expense expense, ExpenseInput request, Household household)
    {
        var memberIds = household.Members.Select(member => member.Id).ToHashSet();
        int paidBy = request.PaidByUserId!.Value;
        if (!memberIds.Contains(paidBy))
            throw new ValidationFailedException("The payer must be a current household member.");

        decimal amount = request.Amount!.Value;
        expense.Shares = splitCalculator.Split(amount, request.SplitType!.Value, request.Shares!, memberIds).ToList();
        expense.Description = request.Description!.Trim();
        expense.Amount = amount;
        expense.Date = request.Date!.Value;
        expense.PaidByUserId = paidBy;
        expense.SplitType = request.SplitType.Value;
    }
}
