using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

public class SellasistOrderResponse
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("email")] public string? Email { get; set; }
    [JsonPropertyName("date")] public string? Date { get; set; }
    [JsonPropertyName("status")] public SellasistStatusInfo? Status { get; set; }
    [JsonPropertyName("comment")] public string? Comment { get; set; }
    [JsonPropertyName("shipment_address")] public SellasistAddress? ShipmentAddress { get; set; }
    [JsonPropertyName("bill_address")] public SellasistAddress? BillAddress { get; set; }
    [JsonPropertyName("carts")] public List<SellasistCartItem>? Carts { get; set; }
    [JsonPropertyName("shipment")] public SellasistShipmentInfo? Shipment { get; set; }
    [JsonPropertyName("payment")] public SellasistPaymentInfo? Payment { get; set; }
    [JsonPropertyName("additional_fields")] public List<SellasistAdditionalField>? AdditionalFields { get; set; }
    [JsonPropertyName("external_data")] public SellasistExternalData? ExternalData { get; set; }

    /// <summary>Wartość całkowita zamówienia (brutto) — przy pobraniu to kwota do pobrania od klienta.</summary>
    [JsonPropertyName("total")] public decimal? Total { get; set; }

    /// <summary>Numer dokumentu zakupu nadany w Sellasist (np. "FA/2020/123321").</summary>
    [JsonPropertyName("document_number")] public string? DocumentNumber { get; set; }

    /// <summary>Listy przewozowe przypięte do zamówienia (tylko GET /orders/{id}) — źródło
    /// <c>ordershipment_uuid</c> do pobrania etykiety przez GET /ordersshipments/{uuid}.</summary>
    [JsonPropertyName("shipments")] public List<SellasistOrderShipmentInfo>? Shipments { get; set; }
}

/// <summary>List przewozowy w odpowiedzi GET /orders/{id} (pole shipments[]).</summary>
public class SellasistOrderShipmentInfo
{
    [JsonPropertyName("ordershipment_id")] public int? OrdershipmentId { get; set; }

    /// <summary>Identyfikator listu przewozowego (GUID) — parametr GET /ordersshipments/{uuid} do pobrania etykiety PDF.</summary>
    [JsonPropertyName("ordershipment_uuid")] public string? OrdershipmentUuid { get; set; }

    /// <summary>Usługa kurierska (np. "inpostCourierC2C", "INPOST - Paczka kurierska").</summary>
    [JsonPropertyName("service")] public string? Service { get; set; }

    [JsonPropertyName("tracking_number")] public string? TrackingNumber { get; set; }

    /// <summary>Numery śledzenia paczek listu (jeden list może mieć wiele paczek).</summary>
    [JsonPropertyName("tracking_numbers")] public List<SellasistTrackingNumberInfo>? TrackingNumbers { get; set; }
}

/// <summary>Pojedynczy numer śledzenia w shipments[].tracking_numbers — pola w camelCase (inaczej niż reszta API).</summary>
public class SellasistTrackingNumberInfo
{
    [JsonPropertyName("trackingNumber")] public string? TrackingNumber { get; set; }
    [JsonPropertyName("alternativeTrackingNumber")] public string? AlternativeTrackingNumber { get; set; }
    [JsonPropertyName("deliveryStatusInternal")] public string? DeliveryStatusInternal { get; set; }
    [JsonPropertyName("deliveryStatusExternal")] public string? DeliveryStatusExternal { get; set; }
    [JsonPropertyName("deliveryStatusExternalDescription")] public string? DeliveryStatusExternalDescription { get; set; }
    [JsonPropertyName("deliveryStatusUpdatedAt")] public string? DeliveryStatusUpdatedAt { get; set; }
    [JsonPropertyName("trackingUrl")] public string? TrackingUrl { get; set; }
}

public class SellasistPaymentInfo
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("paid")] public string? Paid { get; set; }
    [JsonPropertyName("paid_date")] public string? PaidDate { get; set; }

    /// <summary>Flaga pobrania: 1 = COD. Spec dokumentuje bool, realne odpowiedzi bywają 0/1 —
    /// konwerter przyjmuje oba (true→1, false→0).</summary>
    [JsonPropertyName("cod")]
    [JsonConverter(typeof(BoolOrNumberToIntJsonConverter))]
    public int Cod { get; set; }
    [JsonPropertyName("status")] public string? Status { get; set; }
    [JsonPropertyName("currency")] public string? Currency { get; set; }
    [JsonPropertyName("tax")] public string? Tax { get; set; }
}

public class SellasistAddress
{
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("surname")] public string? Surname { get; set; }
    [JsonPropertyName("company_name")] public string? CompanyName { get; set; }
    [JsonPropertyName("company_nip")] public string? CompanyNip { get; set; }
    [JsonPropertyName("street")] public string? Street { get; set; }
    [JsonPropertyName("home_number")] public string? HomeNumber { get; set; }
    [JsonPropertyName("flat_number")] public string? FlatNumber { get; set; }
    [JsonPropertyName("city")] public string? City { get; set; }
    [JsonPropertyName("postcode")] public string? Postcode { get; set; }
    [JsonPropertyName("phone")] public string? Phone { get; set; }
    [JsonPropertyName("state")] public string? State { get; set; }
    [JsonPropertyName("country")] public SellasistCountry? Country { get; set; }
}

public class SellasistCountry
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("code")] public string? Code { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
}

public class SellasistCartItem
{
    [JsonPropertyName("id")] public int Id { get; set; }

    /// <summary>Identyfikator linii zamówienia używany w PUT /orders_lines/{lineId}. W odpowiedzi GET /orders/{id}
    /// pole występuje obok `id` (zwykle ta sama wartość, ale traktować jako semantycznie odrębne).</summary>
    [JsonPropertyName("line_id")] public int? LineId { get; set; }

    /// <summary>ID produktu w Sellasist — wymagane przy PUT /orders_lines/{lineId} (rebuild request).</summary>
    [JsonPropertyName("product_id")] public int? ProductId { get; set; }

    /// <summary>ID wariantu produktu (jeśli pozycja używa wariantu) — null gdy produkt bez wariantów.</summary>
    [JsonPropertyName("variant_id")] public int? VariantId { get; set; }

    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("quantity")] public decimal Quantity { get; set; }
    [JsonPropertyName("weight")] public decimal Weight { get; set; }

    /// <summary>Cena jednostkowa brutto.</summary>
    [JsonPropertyName("price")] public decimal Price { get; set; }

    [JsonPropertyName("ean")] public string? Ean { get; set; }
    [JsonPropertyName("symbol")] public string? Symbol { get; set; }

    /// <summary>Sygnatura pozycji (np. sygnatura aukcji Allegro) — pole `signature` z API Sellasist.</summary>
    [JsonPropertyName("signature")] public string? Signature { get; set; }

    /// <summary>Stawka VAT pozycji jako string (np. "23.000", "8.000", "0.000"). Parsować przez
    /// <c>decimal.Parse(InvariantCulture)</c> w konsumencie — Sellasist zwraca string z 3 miejscami po przecinku.</summary>
    [JsonPropertyName("tax_rate")] public string? TaxRate { get; set; }

    /// <summary>Cena zakupu produktu (netto, z kartoteki produktu).</summary>
    [JsonPropertyName("price_buy")] public decimal? PriceBuy { get; set; }

    /// <summary>Wybrane opcje produktu zakodowane w base64 — preferuj <see cref="SelectedOptionsData"/>.</summary>
    [JsonPropertyName("selected_options")] public string? SelectedOptions { get; set; }

    /// <summary>Wybrane opcje produktu w formie strukturalnej (nazwa + wartość).</summary>
    [JsonPropertyName("selected_options_data")] public List<SellasistSelectedOption>? SelectedOptionsData { get; set; }
}

public class SellasistShipmentInfo
{
    [JsonPropertyName("total")] public string? Total { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("id")] public int? Id { get; set; }
}

public class SellasistAdditionalField
{
    [JsonPropertyName("field_id")] public int FieldId { get; set; }
    [JsonPropertyName("field_value")] public string? FieldValue { get; set; }
}

public class SellasistStatusInfo
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
}

public class SellasistExternalData
{
    [JsonPropertyName("external_id")] public string? ExternalId { get; set; }
    [JsonPropertyName("external_type")] public string? ExternalType { get; set; }

    /// <summary>Login kupującego w portalu zewnętrznym (np. nick Allegro).</summary>
    [JsonPropertyName("external_login")] public string? ExternalLogin { get; set; }

    /// <summary>Identyfikator kupującego w portalu zewnętrznym.</summary>
    [JsonPropertyName("external_user_id")] public int? ExternalUserId { get; set; }

    /// <summary>Nazwa sposobu wysyłki w portalu zewnętrznym (np. "Allegro Paczkomaty InPost").</summary>
    [JsonPropertyName("external_shipment_name")] public string? ExternalShipmentName { get; set; }

    /// <summary>Nazwa sposobu płatności w portalu zewnętrznym (np. "COD").</summary>
    [JsonPropertyName("external_payment_name")] public string? ExternalPaymentName { get; set; }
}

/// <summary>Wybrana opcja/wariant pozycji zamówienia (carts[].selected_options_data).</summary>
public class SellasistSelectedOption
{
    /// <summary>Rodzaj opcji/wariantu (np. "Kolor").</summary>
    [JsonPropertyName("name")] public string? Name { get; set; }

    /// <summary>Wartość opcji/wariantu (np. "Czerwony").</summary>
    [JsonPropertyName("prop")] public string? Prop { get; set; }

    [JsonPropertyName("price")] public decimal? Price { get; set; }
    [JsonPropertyName("option_id")] public int? OptionId { get; set; }
    [JsonPropertyName("variant_id")] public int? VariantId { get; set; }
}
