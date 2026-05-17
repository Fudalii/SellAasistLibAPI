using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Odpowiedź POST /operationdocuments. Sellasist zwraca:
/// <list type="bullet">
/// <item>Sukces: <c>{"status":"created","id":"261"}</c> — id jako STRING (deserializator parsuje przez NumberHandling.AllowReadingFromString)</item>
/// <item>Błąd: <c>{"status":"error","message":"Error creating document: Missing required fields..."}</c></item>
/// </list>
/// Sellasist NIE zwraca pola <c>number</c> w response — żeby pobrać numer trzeba zrobić follow-up
/// GET /operationdocuments/{id}.</summary>
public class SellasistCreateOperationDocumentResponse
{
    /// <summary>ID utworzonego dokumentu (Sellasist zwraca jako string, parsujemy do int).</summary>
    [JsonPropertyName("id")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int Id { get; set; }

    /// <summary>Status — "created" przy sukcesie, "error" przy błędzie.</summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>Wiadomość błędu (tylko gdy <see cref="Status"/> == "error").</summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
