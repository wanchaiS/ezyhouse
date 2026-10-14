using EzyHouse.Api.Core;
using EzyHouse.Domain.Entities;

namespace EzyHouse.Api.Features.Expenses;

public static class ExpenseMappings
{
    public static ExpenseDto ToDto(this Expense expense) => new(
        expense.Id,
        expense.Description,
        expense.Amount,
        expense.Date,
        CoreMappings.Ref(expense.PaidBy, expense.PaidByUserId),
        CoreMappings.Ref(expense.CreatedBy, expense.CreatedByUserId),
        expense.SplitType,
        expense.Shares.Select(share => share.ToDto()).ToList());
}
