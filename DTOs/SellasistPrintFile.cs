using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Żądanie wydruku dokumentu przez Sellasist Cloud Print (POST /printfile).
/// UWAGA: pola MUSZĄ iść w camelCase. Globalna polityka <c>JsonNamingPolicy.SnakeCaseLower</c>
/// w <see cref="Sellasist.Services.SellasistService"/> zamieniłaby <c>PrinterPointId</c> →
/// <c>printer_point_id</c>, czego Cloud Print nie akceptuje. Jawne <c>[JsonPropertyName]</c>
/// nadpisują politykę nazewnictwa — dlatego każde pole ma adnotację.</summary>
public class SellasistPrintFileRequest
{
    /// <summary>Numer ID stanowiska WMS, na które ma trafić wydruk (z <see cref="SellasistPrinterPoint.Id"/>).</summary>
    [JsonPropertyName("printerPointId")] public int PrinterPointId { get; set; }

    /// <summary>Dokument do wydruku zakodowany w base64 (dla <c>pdf</c> — bajty PDF, dla <c>raw</c> — np. ZPL). Wymagane.</summary>
    [JsonPropertyName("document")] public string Document { get; set; } = string.Empty;

    /// <summary>Typ dokumentu: PRINT_ORDER, PRINT_LABEL, PRINT_SALEDOC, PRINT_BARCODE, PRINT_OTHER. Wymagane.</summary>
    [JsonPropertyName("documentType")] public string DocumentType { get; set; } = "PRINT_BARCODE";

    /// <summary>Format dokumentu: <c>pdf</c> lub <c>raw</c> (dotyczy np. ZPL). Domyślnie <c>pdf</c>.</summary>
    [JsonPropertyName("documentFormat")] public string DocumentFormat { get; set; } = "pdf";
}

/// <summary>Stanowisko/drukarka Cloud Print z listy GET /printerpoints — źródło wartości
/// <c>printerPointId</c> przy konfiguracji druku. Pola odwzorowują realną odpowiedź API;
/// uzupełnić po podejrzeniu żywej odpowiedzi (minimum: id + nazwa).</summary>
public class SellasistPrinterPoint
{
    /// <summary>ID stanowiska — wartość do pola <see cref="SellasistPrintFileRequest.PrinterPointId"/>.</summary>
    [JsonPropertyName("id")] public int Id { get; set; }

    /// <summary>Nazwa stanowiska/drukarki widoczna w panelu Sellasist.</summary>
    [JsonPropertyName("name")] public string? Name { get; set; }
}
