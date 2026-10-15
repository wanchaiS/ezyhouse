using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EzyHouse.Api.Core;

/// <summary>
/// Writes every decimal (all of them are money in this API) with exactly two decimal places, so an
/// amount looks the same whether it came from the request (<c>500</c>) or from MySQL (<c>500.00</c>).
/// Reading is left untouched: <c>10.505</c> stays <c>10.505</c>, so the [Money] check can still reject it.
/// </summary>
public sealed class TwoDecimalPlacesConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.GetDecimal();

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options) =>
        writer.WriteRawValue(value.ToString("0.00", CultureInfo.InvariantCulture));
}
