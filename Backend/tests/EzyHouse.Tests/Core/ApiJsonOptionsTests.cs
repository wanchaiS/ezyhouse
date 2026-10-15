using System.Text.Json;
using EzyHouse.Api.Core;
using EzyHouse.Api.Features.Balances;
using EzyHouse.Api.Features.Bills;
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
    public void A_pending_bill_has_no_amount_and_a_submitted_one_has_two_decimal_places()
    {
        var pending = new BillDto(1, new BillTypeRefDto(1, "Internet"), new DateOnly(2026, 10, 1), BillStatus.Pending, null, null, null, []);
        var submitted = pending with { Status = BillStatus.Submitted, Amount = 33.3m };

        Assert.That(JsonSerializer.Serialize(pending, Options), Does.Contain("\"amount\":null"));
        Assert.That(JsonSerializer.Serialize(submitted, Options), Does.Contain("\"amount\":33.30"));
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
