namespace Sellasist.DTOs;

/// <summary>Kategoria z endpointu /categories (lista) — zwraca nadrzedne grupy.</summary>
public class SellasistCategoryResponse
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Parent { get; set; }
}

/// <summary>Szczegoly kategorii z endpointu /categories/{id}.</summary>
public class SellasistCategoryDetailResponse
{
    public string Id { get; set; } = string.Empty;
    public string? Parent { get; set; }
    public string? Status { get; set; }
    public List<SellasistCategoryLanguage>? Languages { get; set; }
}

/// <summary>Tlumaczenie kategorii.</summary>
public class SellasistCategoryLanguage
{
    public string? LanguageId { get; set; }
    public string Title { get; set; } = string.Empty;
}
