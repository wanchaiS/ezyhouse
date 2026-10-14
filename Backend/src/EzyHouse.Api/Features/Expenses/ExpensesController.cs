using Microsoft.AspNetCore.Mvc;

namespace EzyHouse.Api.Features.Expenses;

[ApiController]
[Route("api/expenses")]
public sealed class ExpensesController(IExpenseService expenses) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<ExpenseDto>> List() => expenses.ListAsync();

    [HttpGet("{id:int}")]
    public Task<ExpenseDto> Get(int id) => expenses.GetAsync(id);

    [HttpPost]
    public Task<ExpenseDto> Create(ExpenseInput request) => expenses.CreateAsync(request);

    [HttpPut("{id:int}")]
    public Task<ExpenseDto> Update(int id, ExpenseInput request) => expenses.UpdateAsync(id, request);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await expenses.DeleteAsync(id);
        return NoContent();
    }
}
