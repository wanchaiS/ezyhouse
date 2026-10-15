using System.Text.Json;
using EzyHouse.Api.Core;
using EzyHouse.Api.Features.Balances;
using EzyHouse.Api.Features.Bonds;
using EzyHouse.Api.Features.Expenses;
using EzyHouse.Domain.Enums;

namespace EzyHouse.Tests.Core;

public class ApiJsonOptionsTests
{
    private static readonly JsonSerializerOptions Options = CreateOptions();
    private static readonly UserRefDto Alice = new(1, "Alice");

    [Test]
    public void Money_is_written_with_two_decimal_places_whatever_its_source()
    {
        // 500m is what a request body gives; 400.5m is what arithmetic can give.
        var bond = new BondDto(1, 500m, new DateOnly(2026, 9, 3), [new ContributionDto(Alice, 100m, new DateOnly(2026, 9, 3))], 400.5m);
        var balance = new MemberBalanceDto(Alice, -553.2m);

        string bondJson = JsonSerializer.Serialize(bond, Options);
        string balanceJson = JsonSerializer.Serialize(balance, Options);

        Assert.That(bondJson, Does.Contain("\"total\":500.00"));
        Assert.That(bondJson, Does.Contain("\"outstanding\":400.50"));
        Assert.That(bondJson, Does.Contain("\"amount\":100.00"));
        Assert.That(balanceJson, Does.Contain("\"net\":-553.20"));
    }

    [Test]
    public void The_tag_is_read_and_written_as_a_lowercase_string()
    {
        var input = JsonSerializer.Deserialize<NewExpenseInput>(
            """{"tag":"groceries","description":"Milk","amount":0,"date":"2026-10-15","paidByUserId":1,"splitType":"equal","shares":[]}""",
            Options);
        var bill = new ExpenseDto(1, ExpenseTag.Bills, "Internet", 0m, new DateOnly(2026, 10, 1), Alice, Alice,
            SplitType.Equal, [], true, null, null, 4);

        string json = JsonSerializer.Serialize(bill, Options);

        Assert.That(input!.Tag, Is.EqualTo(ExpenseTag.Groceries));
        Assert.That(json, Does.Contain("\"tag\":\"bills\""));
        Assert.That(json, Does.Contain("\"amount\":0.00"));
        Assert.That(json, Does.Contain("\"rentScheduleId\":null"));
        Assert.That(json, Does.Contain("\"billTypeId\":4"));
    }

    [Test]
    public void Reading_keeps_every_decimal_place_so_validation_can_still_reject_three()
    {
        var input = JsonSerializer.Deserialize<ExpenseInput>(
            """{"description":"Milk","amount":10.505,"date":"2026-10-15","paidByUserId":1,"splitType":"equal","shares":[]}""",
            Options);

        Assert.That(input!.Amount, Is.EqualTo(10.505m));
    }

    [Test]
    public void An_enum_sent_as_a_number_is_rejected()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<ExpenseInput>(
            """{"description":"Milk","amount":10,"date":"2026-10-15","paidByUserId":1,"splitType":7,"shares":[]}""",
            Options));
    }

    private static JsonSerializerOptions CreateOptions()
    {
        // The same starting point ASP.NET uses for controllers (camelCase), plus the API's own rules.
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        ApiJsonOptions.Apply(options);
        return options;
    }
}
