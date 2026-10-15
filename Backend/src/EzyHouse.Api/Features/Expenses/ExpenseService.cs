using EzyHouse.Api.Core;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;
using EzyHouse.Domain.Exceptions;
using EzyHouse.Domain.Splitting;

namespace EzyHouse.Api.Features.Expenses;

public sealed class ExpenseService(
    IExpenseRepository expenses,
    ICurrentUserContext currentUser,
    SplitCalculator splitCalculator) : IExpenseService
{
    public async Task<IReadOnlyList<ExpenseDto>> ListAsync(ExpenseTag? tag)
    {
        // The query string binds any number as an enum, so an unknown tag is checked here.
        if (tag is ExpenseTag given && !Enum.IsDefined(given))
            throw new ValidationFailedException("tag must be rent, bills, groceries or furniture.");

        Household household = await currentUser.RequireHouseholdAsync();
        IReadOnlyList<Expense> list = await expenses.ListAsync(household.Id, tag);
        return list.Select(expense => ToDto(expense, household)).ToList();
    }

    public async Task<ExpenseDto> GetAsync(int id)
    {
        Household household = await currentUser.RequireHouseholdAsync();
        return ToDto(await LoadAsync(household.Id, id), household);
    }

    public async Task<ExpenseDto> CreateAsync(NewExpenseInput request)
    {
        Household household = await currentUser.RequireHouseholdAsync();
        Expense expense = Expense.Create(request.Tag!.Value);
        expense.HouseholdId = household.Id;
        expense.CreatedByUserId = currentUser.UserId;

        // You may add an expense exactly when you could change it once added: groceries are anyone's
        // (the creator can always change their own), every other tag is the primary's.
        if (!expense.CanBeChangedBy(currentUser.UserId, household.PrimaryUserId))
            throw new ForbiddenException("Only the primary housemate can add rent, bills or furniture.");

        Apply(expense, request.WithoutTag(), household);
        expenses.Add(expense);
        await expenses.SaveChangesAsync();
        return ToDto(await LoadAsync(household.Id, expense.Id), household);
    }

    public async Task<ExpenseDto> UpdateAsync(int id, ExpenseInput request)
    {
        Household household = await currentUser.RequireHouseholdAsync();
        Expense expense = await LoadChangeableAsync(household, id);
        Apply(expense, request, household);
        await expenses.SaveChangesAsync();
        return ToDto(await LoadAsync(household.Id, id), household);
    }

    public async Task DeleteAsync(int id)
    {
        Household household = await currentUser.RequireHouseholdAsync();
        Expense expense = await LoadChangeableAsync(household, id);

        // Permanent even for an expense a schedule created: the schedule's next due date has already
        // moved past this date, so it is never created again.
        expenses.Remove(expense);
        await expenses.SaveChangesAsync();
    }

    private ExpenseDto ToDto(Expense expense, Household household) =>
        expense.ToDto(currentUser.UserId, household.PrimaryUserId);

    private async Task<Expense> LoadAsync(int householdId, int id) =>
        await expenses.FindAsync(householdId, id) ?? throw new NotFoundException("Expense not found.");

    // Loaded before the permission check, so another household's expense is a 404, not a 403.
    private async Task<Expense> LoadChangeableAsync(Household household, int id)
    {
        Expense expense = await LoadAsync(household.Id, id);
        if (!expense.CanBeChangedBy(currentUser.UserId, household.PrimaryUserId))
            throw new ForbiddenException(expense.Tag == ExpenseTag.Groceries
                ? "Only the person who added this expense can change it."
                : "Only the primary housemate can change this expense.");
        return expense;
    }

    // Model validation guarantees the required fields are present. The tag and any schedule link stay as they are.
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
