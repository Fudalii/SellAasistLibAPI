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
}

public class SellasistPaymentInfo
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("paid")] public string? Paid { get; set; }
    [JsonPropertyName("paid_date")] public string? PaidDate { get; set; }
    [JsonPropertyName("cod")] public int Cod { get; set; }
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

    /// <summary>Stawka VAT pozycji jako string (np. "23.000", "8.000", "0.000"). Parsować przez
    /// <c>decimal.Parse(InvariantCulture)</c> w konsumencie — Sellasist zwraca string z 3 miejscami po przecinku.</summary>
    [JsonPropertyName("tax_rate")] public string? TaxRate { get; set; }
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
}
