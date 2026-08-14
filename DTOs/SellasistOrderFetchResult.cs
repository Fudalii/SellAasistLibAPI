namespace Sellasist.DTOs;

/// <summary>Wynik pobrania zamówienia z diagnostyką niepowodzenia — w odróżnieniu od
/// <c>GetOrderAsync</c> (null bez powodu) niesie status HTTP i opis błędu (parsowanie/sieć),
/// żeby konsument mógł pokazać użytkownikowi DLACZEGO zamówienia nie ma.</summary>
public class SellasistOrderFetchResult
{
    /// <summary>Zamówienie — null gdy pobranie lub parsowanie się nie powiodło.</summary>
    public SellasistOrderResponse? Order { get; set; }

    /// <summary>Status HTTP odpowiedzi Sellasist (null, gdy żądanie nie doszło do odpowiedzi — błąd sieci/timeout).</summary>
    public int? HttpStatus { get; set; }

    /// <summary>Krótki opis przyczyny niepowodzenia (po polsku, nadaje się do UI/logów) — null przy sukcesie.</summary>
    public string? Error { get; set; }
}
