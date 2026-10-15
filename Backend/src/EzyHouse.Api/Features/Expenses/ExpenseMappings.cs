using EzyHouse.Api.Core;
using EzyHouse.Domain.Entities;

namespace EzyHouse.Api.Features.Expenses;

public static class ExpenseMappings
{
    /// <summary><paramref name="callerId"/> and <paramref name="primaryUserId"/> decide <c>canChange</c> for the person asking.</summary>
    public static ExpenseDto ToDto(this Expense expense, int callerId, int primaryUserId) => new(
        expense.Id,
        expense.Tag,
        expense.Description,
        expense.Amount,
        expense.Date,
        CoreMappings.Ref(expense.PaidBy, expense.PaidByUserId),
        CoreMappings.Ref(expense.CreatedBy, expense.CreatedByUserId),
        expense.SplitType,
        expense.Shares.Select(share => share.ToDto()).ToList(),
        expense.CanBeChangedBy(callerId, primaryUserId),
        (expense as RentExpense)?.RentScheduleId,
        (expense as RentExpense)?.PeriodEnd,
        (expense as BillExpense)?.BillTypeId);
}
