using System.Text.Json;
using System.Text.Json.Serialization;

namespace EzyHouse.Api.Core;

/// <summary>The JSON rules from API.md, in one place so Program.cs and the tests use the same settings.</summary>
public static class ApiJsonOptions
{
    public static void Apply(JsonSerializerOptions options)
    {
        // Enums are lowercase strings ("equal", "weekly"); numbers such as 7 are rejected with a 400.
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
        // Money always has exactly two decimal places in responses.
        options.Converters.Add(new TwoDecimalPlacesConverter());
    }
}
