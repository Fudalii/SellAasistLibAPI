using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Sellasist.Config;
using Sellasist.DTOs;
using Sellasist.Interfaces;

namespace Sellasist.Services;

public class SellasistService(IHttpClientFactory httpClientFactory, SellasistConfig config, ILogger<SellasistService> logger)
    : ISellasistService
{
    private SellasistConfig _config = config;

    /// <inheritdoc />
    public void Configure(SellasistConfig newConfig) => _config = newConfig;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    // === CORE METHOD ===

    private async Task<T?> SendRequestAsync<T>(string endpoint, HttpMethod method, object? body = null)
    {
        if (string.IsNullOrWhiteSpace(_config.ApiToken))
        {
            logger.LogError("Sellasist API token is empty");
            return default;
        }

        var url = $"{_config.BaseUrl}/{endpoint}";

        using var request = new HttpRequestMessage(method, url);
        request.Headers.Add("apiKey", _config.ApiToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (body != null)
        {
            var json = JsonSerializer.Serialize(body, JsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        using var client = httpClientFactory.CreateClient("SellasistApi");
        using var response = await client.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Sellasist {Method} /{Endpoint} failed: {Status} {Body}",
                method.Method, endpoint, (int)response.StatusCode, responseContent);
            return default;
        }

        if (typeof(T) == typeof(bool))
            return (T)(object)true;

        try
        {
            return JsonSerializer.Deserialize<T>(responseContent, JsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to deserialize Sellasist response from /{Endpoint}", endpoint);
            return default;
        }
    }

    // === ORDER CREATION ===

    public async Task<SellasistCreateOrderResponse?> CreateOrderAsync(SellasistCreateOrderRequest request)
        => await SendRequestAsync<SellasistCreateOrderResponse>("orders", HttpMethod.Post, request);

    // === ORDERS ===

    public async Task<SellasistOrderResponse?> GetOrderAsync(int orderId)
        => await SendRequestAsync<SellasistOrderResponse>($"orders/{orderId}", HttpMethod.Get);

    public async Task<List<SellasistOrderResponse>> GetOrdersByStatusAsync(int statusId, int limit = 50)
    {
        var all = new List<SellasistOrderResponse>();
        int offset = 0;
        bool hasMore = true;

        while (hasMore)
        {
            var batch = await SendRequestAsync<List<SellasistOrderResponse>>(
                $"orders?offset={offset}&limit={limit}&status_id={statusId}", HttpMethod.Get);

            if (batch is { Count: > 0 })
            {
                all.AddRange(batch);
                offset += limit;
                if (batch.Count < limit) hasMore = false;
            }
            else hasMore = false;
        }
        return all;
    }

    public async Task<List<SellasistOrderResponse>> GetOrdersWithCartsAsync(int statusId, int limit = 50)
    {
        var all = new List<SellasistOrderResponse>();
        int offset = 0;
        bool hasMore = true;

        while (hasMore)
        {
            var batch = await SendRequestAsync<List<SellasistOrderResponse>>(
                $"orders_with_carts?offset={offset}&limit={limit}&status_id={statusId}", HttpMethod.Get);

            if (batch is { Count: > 0 })
            {
                all.AddRange(batch);
                offset += limit;
                if (batch.Count < limit) hasMore = false;
            }
            else hasMore = false;
        }
        return all;
    }

    // === ORDER UPDATES ===

    public async Task<bool> UpdateOrderStatusAsync(int orderId, int statusId)
        => await SendRequestAsync<bool>($"orders/{orderId}", HttpMethod.Put, new { status = statusId.ToString() });

    public async Task<bool> UpdateAdditionalFieldAsync(int orderId, int fieldId, string value)
        => await SendRequestAsync<bool>($"orders/{orderId}", HttpMethod.Put,
            new { additional_fields = new[] { new { field_id = fieldId, field_value = value } } });

    public async Task<bool> ClearShippingCostAsync(int orderId)
        => await SendRequestAsync<bool>($"orders/{orderId}", HttpMethod.Put, new { shipment_price = "0.00" });

    public async Task<bool> UpdatePaymentStatusAsync(int orderId, string status)
        => await SendRequestAsync<bool>($"orders/{orderId}", HttpMethod.Put, new { payment_status = status });

    public async Task<bool> SendShipmentCostAsync(int orderId, string cost)
        => await SendRequestAsync<bool>($"orders/{orderId}", HttpMethod.Put, new { shipment_price = cost });

    public async Task<bool> UpdateOrderTotalAsync(int orderId, string total)
        => await SendRequestAsync<bool>($"orders/{orderId}", HttpMethod.Put, new { total });

    // === AWB / SHIPMENTS ===

    public async Task<bool> SubmitAwbAsync(SellasistAddAwbRequest request)
    {
        var result = await SendRequestAsync<bool>("ordersshipments", HttpMethod.Post, request);
        if (result)
            logger.LogInformation("Sellasist AWB submitted: order {OrderId}, tracking {Tracking}",
                request.OrderId, request.TrackingNumber);
        return result;
    }

    public async Task<List<SellasistShipmentDto>> GetOrderShipmentsAsync(int orderId)
        => await SendRequestAsync<List<SellasistShipmentDto>>($"ordersshipments?order_id={orderId}", HttpMethod.Get)
           ?? [];

    // === PRODUCTS ===

    public async Task<bool> UpdateProductQuantityAsync(int productId, string quantity)
        => await SendRequestAsync<bool>($"products/{productId}", HttpMethod.Put, new { quantity });

    public async Task<SellasistProductBulkUpdateResponse?> UpdateProductsBulkAsync(List<SellasistProductBulkUpdateItem> items)
        => await SendRequestAsync<SellasistProductBulkUpdateResponse>("products_bulk", HttpMethod.Put, items);

    public async Task<List<SellasistProductBulkItem>> GetProductsBulkAsync(int limit = 500)
    {
        var all = new List<SellasistProductBulkItem>();
        int offset = 0;
        bool hasMore = true;

        while (hasMore)
        {
            var batch = await SendRequestAsync<List<SellasistProductBulkItem>>(
                $"products_bulk?offset={offset}&limit={limit}", HttpMethod.Get);

            if (batch is { Count: > 0 })
            {
                all.AddRange(batch);
                offset += limit;
                if (batch.Count < limit) hasMore = false;
            }
            else hasMore = false;
        }
        return all;
    }

    public async Task<SellasistProductResponse?> GetProductAsync(int productId)
        => await SendRequestAsync<SellasistProductResponse>($"products/{productId}", HttpMethod.Get);

    // === CATEGORIES ===

    public async Task<List<SellasistCategoryResponse>> GetCategoriesAsync(int limit = 500)
    {
        var all = new List<SellasistCategoryResponse>();
        int offset = 0;
        bool hasMore = true;

        while (hasMore)
        {
            var batch = await SendRequestAsync<List<SellasistCategoryResponse>>(
                $"categories?offset={offset}&limit={limit}", HttpMethod.Get);

            if (batch is { Count: > 0 })
            {
                all.AddRange(batch);
                offset += limit;
                if (batch.Count < limit) hasMore = false;
            }
            else hasMore = false;
        }
        return all;
    }

    public async Task<SellasistCategoryDetailResponse?> GetCategoryAsync(int categoryId)
        => await SendRequestAsync<SellasistCategoryDetailResponse>($"categories/{categoryId}", HttpMethod.Get);

    public async Task<List<SellasistManufacturerResponse>> GetManufacturersAsync(int limit = 500)
    {
        var all = new List<SellasistManufacturerResponse>();
        int offset = 0;
        bool hasMore = true;

        while (hasMore)
        {
            var batch = await SendRequestAsync<List<SellasistManufacturerResponse>>(
                $"manufacturers?offset={offset}&limit={limit}", HttpMethod.Get);

            if (batch is { Count: > 0 })
            {
                all.AddRange(batch);
                offset += limit;
                if (batch.Count < limit) hasMore = false;
            }
            else hasMore = false;
        }
        return all;
    }

    public async Task<List<SellasistStatusResponse>> GetOrderStatusesAsync()
    {
        var result = await SendRequestAsync<List<SellasistStatusResponse>>("statuses", HttpMethod.Get);
        return result ?? new List<SellasistStatusResponse>();
    }

    public async Task<List<SellasistShipmentMethodResponse>> GetShipmentMethodsAsync()
    {
        var result = await SendRequestAsync<List<SellasistShipmentMethodResponse>>("shipments", HttpMethod.Get);
        return result ?? new List<SellasistShipmentMethodResponse>();
    }

    public async Task<List<SellasistPaymentMethodResponse>> GetPaymentMethodsAsync()
    {
        var result = await SendRequestAsync<List<SellasistPaymentMethodResponse>>("payments", HttpMethod.Get);
        return result ?? new List<SellasistPaymentMethodResponse>();
    }

    // === EXTRA FIELDS ===

    public async Task<List<SellasistExtraFieldResponse>> GetExtraFieldsAsync()
    {
        var result = await SendRequestAsync<List<SellasistExtraFieldResponse>>("extra-fields", HttpMethod.Get);
        return result ?? [];
    }
}
