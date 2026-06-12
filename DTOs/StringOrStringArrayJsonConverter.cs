using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Konwerter pól, które Sellasist zwraca raz jako pojedynczy string, raz jako tablicę stringów
/// (np. file w /ordersshipments: spec dokumentuje string, realna odpowiedź po uuid zwraca tablicę —
/// jeden PDF per paczka). Zawsze mapuje na listę; null → null.</summary>
public class StringOrStringArrayJsonConverter : JsonConverter<List<string>?>
{
    public override List<string>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;

            case JsonTokenType.String:
                var single = reader.GetString();
                return string.IsNullOrEmpty(single) ? [] : [single];

            case JsonTokenType.StartArray:
                var list = new List<string>();
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    if (reader.TokenType == JsonTokenType.String)
                    {
                        var value = reader.GetString();
                        if (!string.IsNullOrEmpty(value))
                            list.Add(value);
                    }
                    else if (reader.TokenType != JsonTokenType.Null)
                    {
                        throw new JsonException($"Nieobsługiwany token {reader.TokenType} w tablicy string-or-array.");
                    }
                }
                return list;

            default:
                throw new JsonException($"Nieobsługiwany token {reader.TokenType} dla pola string-or-array.");
        }
    }

    public override void Write(Utf8JsonWriter writer, List<string>? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }
        writer.WriteStartArray();
        foreach (var item in value)
            writer.WriteStringValue(item);
        writer.WriteEndArray();
    }
}
