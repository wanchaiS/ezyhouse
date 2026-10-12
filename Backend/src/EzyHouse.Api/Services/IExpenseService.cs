using EzyHouse.Api.Dtos;

namespace EzyHouse.Api.Services;

public interface IExpenseService
{
    Task<IReadOnlyList<ExpenseDto>> ListAsync();

    Task<ExpenseDto> GetAsync(int id);

    Task<ExpenseDto> CreateAsync(ExpenseInput request);

    Task<ExpenseDto> UpdateAsync(int id, ExpenseInput request);

    Task DeleteAsync(int id);
}
