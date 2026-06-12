using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Konwerter pól flagowych, które Sellasist zwraca raz jako bool, raz jako 0/1 (spec OpenAPI
/// dokumentuje payment.cod jako bool, realne odpowiedzi bywają liczbą). true→1, false→0, liczba/string
/// liczbowy → wartość, null → 0. Chroni przed wywaleniem deserializacji CAŁEGO zamówienia jedną flagą.</summary>
public class BoolOrNumberToIntJsonConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.True => 1,
            JsonTokenType.False => 0,
            JsonTokenType.Number => reader.TryGetInt32(out var i) ? i : (int)reader.GetDecimal(),
            JsonTokenType.String => int.TryParse(reader.GetString(), out var parsed) ? parsed : 0,
            JsonTokenType.Null => 0,
            _ => throw new JsonException($"Nieobsługiwany token {reader.TokenType} dla pola bool-or-number.")
        };

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value);
}
