using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Konwerter pól, które Sellasist zwraca raz jako liczbę, raz jako string
/// (np. id listu przewozowego: liczba przy zapytaniu po id, UUID przy zapytaniu po uuid).
/// Zawsze mapuje na string.</summary>
public class NumberOrStringJsonConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => reader.TryGetInt64(out var l) ? l.ToString() : reader.GetDecimal().ToString(System.Globalization.CultureInfo.InvariantCulture),
            JsonTokenType.Null => null,
            _ => throw new JsonException($"Nieobsługiwany token {reader.TokenType} dla pola number-or-string.")
        };

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value is null) writer.WriteNullValue();
        else writer.WriteStringValue(value);
    }
}
