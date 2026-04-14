using System.Text.Json.Serialization;

namespace Sellasist.DTOs;

/// <summary>Dane zamówienia do utworzenia przez POST /orders.</summary>
public class SellasistCreateOrderRequest
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "pln";

    [JsonPropertyName("payment_status")]
    public string PaymentStatus { get; set; } = "unpaid";

    [JsonPropertyName("paid")]
    public decimal Paid { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("shipment_price")]
    public decimal ShipmentPrice { get; set; }

    [JsonPropertyName("payment_id")]
    public int PaymentId { get; set; }

    [JsonPropertyName("payment_name")]
    public string? PaymentName { get; set; }

    [JsonPropertyName("shipment_id")]
    public int ShipmentId { get; set; }

    [JsonPropertyName("shipment_name")]
    public string? ShipmentName { get; set; }

    [JsonPropertyName("invoice")]
    public int Invoice { get; set; }

    [JsonPropertyName("comment")]
    public string Comment { get; set; } = string.Empty;

    [JsonPropertyName("bill_address")]
    public SellasistCreateOrderAddress BillAddress { get; set; } = new();

    [JsonPropertyName("shipment_address")]
    public SellasistCreateOrderAddress ShipmentAddress { get; set; } = new();

    [JsonPropertyName("carts")]
    public List<SellasistCreateOrderCartItem> Carts { get; set; } = new();

    [JsonPropertyName("pickup_point")]
    public SellasistCreateOrderPickupPoint? PickupPoint { get; set; }
}

/// <summary>Adres w zamówieniu Sellasist (faktura lub wysyłka).</summary>
public class SellasistCreateOrderAddress
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("surname")]
    public string Surname { get; set; } = string.Empty;

    [JsonPropertyName("street")]
    public string Street { get; set; } = string.Empty;

    [JsonPropertyName("home_number")]
    public string HomeNumber { get; set; } = string.Empty;

    [JsonPropertyName("flat_number")]
    public string FlatNumber { get; set; } = string.Empty;

    [JsonPropertyName("postcode")]
    public string Postcode { get; set; } = string.Empty;

    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("company_name")]
    public string? CompanyName { get; set; }

    [JsonPropertyName("company_nip")]
    public string? CompanyNip { get; set; }
}

/// <summary>Pozycja koszyka w zamówieniu Sellasist.</summary>
public class SellasistCreateOrderCartItem
{
    [JsonPropertyName("product_id")]
    public int ProductId { get; set; }

    [JsonPropertyName("variant_id")]
    public int? VariantId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("tax")]
    public int Tax { get; set; }

    [JsonPropertyName("catalog_number")]
    public string? CatalogNumber { get; set; }

    [JsonPropertyName("ean")]
    public string? Ean { get; set; }
}

/// <summary>Punkt odbioru (np. InPost paczkomat).</summary>
public class SellasistCreateOrderPickupPoint
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string? Address { get; set; }
}
