using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Enums;

namespace EzyHouse.Tests.Domain;

public class ExpenseTests
{
    private const int Creator = 1;
    private const int Primary = 2;
    private const int Someone = 3;

    [Test]
    public void Groceries_can_be_changed_by_whoever_added_them_and_not_even_by_the_primary()
    {
        Expense groceries = Expense.Create(ExpenseTag.Groceries);
        groceries.CreatedByUserId = Creator;

        Assert.Multiple(() =>
        {
            Assert.That(groceries.CanBeChangedBy(Creator, Primary), Is.True);
            Assert.That(groceries.CanBeChangedBy(Primary, Primary), Is.False);
            Assert.That(groceries.CanBeChangedBy(Someone, Primary), Is.False);
        });
    }

    [TestCase(ExpenseTag.Rent)]
    [TestCase(ExpenseTag.Bills)]
    [TestCase(ExpenseTag.Furniture)]
    public void Rent_bills_and_furniture_can_be_changed_by_the_current_primary_only_not_by_whoever_added_them(ExpenseTag tag)
    {
        Expense expense = Expense.Create(tag);
        expense.CreatedByUserId = Creator;

        Assert.Multiple(() =>
        {
            Assert.That(expense.Tag, Is.EqualTo(tag));
            Assert.That(expense.CanBeChangedBy(Primary, Primary), Is.True);
            Assert.That(expense.CanBeChangedBy(Creator, Primary), Is.False);
            Assert.That(expense.CanBeChangedBy(Someone, Primary), Is.False);
        });
    }
}
