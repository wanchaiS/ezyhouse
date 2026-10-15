using EzyHouse.Domain.Enums;

namespace EzyHouse.Api.Features.Expenses;

public interface IExpenseService
{
    Task<IReadOnlyList<ExpenseDto>> ListAsync(ExpenseTag? tag);

    Task<ExpenseDto> GetAsync(int id);

    Task<ExpenseDto> CreateAsync(NewExpenseInput request);

    Task<ExpenseDto> UpdateAsync(int id, ExpenseInput request);

    Task DeleteAsync(int id);
}
