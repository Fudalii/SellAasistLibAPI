using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Encodings.Web;
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
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        // Cudzysłowy w wartościach (np. nazwa firmy P.H.U. "JUREX") escapujemy jako \" (styl json_encode/PHP),
        // a nie domyślnym " — parser API Sellasist odrzucał payload z ". Bonus: polskie znaki jako UTF-8
        // (bezpieczne: Content-Type application/json; charset=utf-8; „unsafe" dotyczy tylko osadzania JSON w HTML).
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    // Throttle — minimum odstęp między requestami (config.MinDelayBetweenRequestsMs).
    // _nextAllowedRequestUtc to „następny dozwolony moment" — każdy call go aktualizuje rezerwując
    // sobie okno (now+delay). Dzięki temu equal-tempo nawet przy concurrent calls.
    private readonly object _throttleLock = new();
    private DateTime _nextAllowedRequestUtc = DateTime.MinValue;

    private async Task ApplyThrottleAsync()
    {
        var delayMs = _config.MinDelayBetweenRequestsMs;
        if (delayMs <= 0) return;

        int waitMs;
        lock (_throttleLock)
        {
            var now = DateTime.UtcNow;
            var earliest = now < _nextAllowedRequestUtc ? _nextAllowedRequestUtc : now;
            waitMs = (int)(earliest - now).TotalMilliseconds;
            _nextAllowedRequestUtc = earliest.AddMilliseconds(delayMs);
        }
        if (waitMs > 0)
            await Task.Delay(waitMs);
    }

    // === CORE METHOD ===

    /// <summary>Ile razy próbujemy wysłać jedno zapytanie (razem z pierwszą próbą), gdy padnie łącze
    /// albo serwer odpowie błędem przejściowym.</summary>
    private const int MaxAttempts = 3;

    /// <summary>Timeout dla endpointów listowych z paginacją (products_bulk, products, categories,
    /// manufacturers). Domyślne 30 s bywa za mało dla strony 500 pozycji z katalogu liczącego tysiące
    /// produktów, a wyjątek timeoutu wywracał CAŁY przebieg synchronizacji — zanim poszedł pierwszy produkt.</summary>
    public static readonly TimeSpan ListTimeout = TimeSpan.FromSeconds(180);

    /// <summary>Czy zapytanie wolno powtórzyć. GET/PUT/DELETE są idempotentne — powtórka daje ten sam skutek.
    /// POST-a nie ponawiamy NIGDY: zerwane połączenie nie mówi, czy serwer zdążył utworzyć rekord, więc
    /// druga próba mogłaby zrobić drugie zamówienie albo drugi produkt.</summary>
    private static bool CanRetry(HttpMethod method)
        => method == HttpMethod.Get || method == HttpMethod.Put
        || method == HttpMethod.Delete || method == HttpMethod.Head;

    /// <summary>Awaria łącza warta ponowienia: zerwane połączenie (SocketException 10054), błąd I/O,
    /// timeout klienta HTTP (leci jako TaskCanceledException — własnego tokenu anulowania tu nie ma).</summary>
    private static bool IsTransientFailure(Exception ex)
        => ex is HttpRequestException or IOException or SocketException or TaskCanceledException;

    /// <summary>Odpowiedź warta ponowienia: 408 (timeout), 429 (rate limit) i błędy serwera 5xx.</summary>
    private static bool IsTransientStatus(HttpStatusCode status)
        => status is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests || (int)status >= 500;

    /// <summary>Odstęp przed kolejną próbą — 2 s, potem 4 s (ponad zwykły throttle z konfiguracji).</summary>
    private static TimeSpan RetryDelay(int attempt) => TimeSpan.FromSeconds(2 * attempt);

    /// <summary>Wysyła jedno zapytanie i zwraca surowy status + treść, ponawiając próby dla metod
    /// idempotentnych. Gdy padną wszystkie — wyjątek leci do wołającego (świadomie: „nie wiem" musi być
    /// odróżnialne od „pusto", inaczej cache katalogu wyczyściłby się na cichej awarii sieci).</summary>
    private async Task<(HttpStatusCode Status, string Body)> SendRawAsync(
        string endpoint, HttpMethod method, object? body, TimeSpan? timeout)
    {
        var url = $"{_config.BaseUrl}/{endpoint}";
        var json = body is null ? null : JsonSerializer.Serialize(body, JsonOptions);
        var retryable = CanRetry(method);

        for (int attempt = 1; ; attempt++)
        {
            await ApplyThrottleAsync();

            try
            {
                using var request = new HttpRequestMessage(method, url);
                request.Headers.Add("apiKey", _config.ApiToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (json is not null)
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                // CreateClient oddaje za każdym razem NOWY HttpClient (współdzielony jest tylko handler),
                // więc Timeout wolno ustawić per zapytanie — nie rusza to pozostałych wywołań.
                using var client = httpClientFactory.CreateClient("SellasistApi");
                if (timeout is { } t) client.Timeout = t;

                using var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (retryable && attempt < MaxAttempts && IsTransientStatus(response.StatusCode))
                {
                    logger.LogWarning("Sellasist {Method} /{Endpoint}: HTTP {Status} — próba {Attempt}/{Max}, ponawiam za {Delay} s.",
                        method.Method, endpoint, (int)response.StatusCode, attempt, MaxAttempts, RetryDelay(attempt).TotalSeconds);
                    await Task.Delay(RetryDelay(attempt));
                    continue;
                }

                return (response.StatusCode, content);
            }
            catch (Exception ex) when (retryable && attempt < MaxAttempts && IsTransientFailure(ex))
            {
                logger.LogWarning(ex, "Sellasist {Method} /{Endpoint}: zapytanie nie doszło ({Powod}) — próba {Attempt}/{Max}, ponawiam za {Delay} s.",
                    method.Method, endpoint, ex.GetType().Name, attempt, MaxAttempts, RetryDelay(attempt).TotalSeconds);
                await Task.Delay(RetryDelay(attempt));
            }
        }
    }

    /// <summary>Zapytanie do API z deserializacją odpowiedzi. Zwraca default przy HTTP non-2xx
    /// i przy błędzie parsowania; wyjątki sieciowe (po wyczerpaniu ponowień) przepuszcza do wołającego.</summary>
    /// <param name="timeout">Nadpisanie timeoutu dla tego zapytania (null = 30 s z rejestracji klienta).</param>
    private async Task<T?> SendRequestAsync<T>(string endpoint, HttpMethod method, object? body = null, TimeSpan? timeout = null)
    {
        if (string.IsNullOrWhiteSpace(_config.ApiToken))
        {
            logger.LogError("Sellasist API token is empty");
            return default;
        }

        var (status, responseContent) = await SendRawAsync(endpoint, method, body, timeout);

        if (!IsSuccess(status))
        {
            // 404 dla zasobów listowych w Sellasist znaczy „brak rekordów" — to nie błąd, tylko pusta lista.
            // Nie logujemy warningu, żeby nie zaśmiecać logów (np. /ordersshipments dla świeżego zamówienia).
            if (status != HttpStatusCode.NotFound)
            {
                logger.LogWarning("Sellasist {Method} /{Endpoint} failed: {Status} {Body}",
                    method.Method, endpoint, (int)status, responseContent);
            }
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

    private static bool IsSuccess(HttpStatusCode status) => (int)status is >= 200 and <= 299;

    /// <summary>Jak <see cref="SendRequestAsync{T}"/>, ale rozróżnia BŁĄD (HTTP non-2xx poza 404,
    /// wyjątek sieci/timeout, błąd deserializacji) od pustego wyniku (404/2xx) — Success=false oznacza,
    /// że dane są NIEZNANE, a nie puste. Używać tam, gdzie częściowa odpowiedź jest groźna (paginacja).</summary>
    private async Task<(bool Success, T? Data)> TrySendRequestAsync<T>(
        string endpoint, HttpMethod method, object? body = null, TimeSpan? timeout = null)
    {
        if (string.IsNullOrWhiteSpace(_config.ApiToken))
        {
            logger.LogError("Sellasist API token is empty");
            return (false, default);
        }

        try
        {
            var (status, responseContent) = await SendRawAsync(endpoint, method, body, timeout);

            if (!IsSuccess(status))
            {
                // 404 dla zasobów listowych = brak rekordów (pusta strona) — to sukces, nie błąd.
                if (status == HttpStatusCode.NotFound)
                    return (true, default);

                logger.LogWarning("Sellasist {Method} /{Endpoint} failed: {Status} {Body}",
                    method.Method, endpoint, (int)status, responseContent);
                return (false, default);
            }

            if (typeof(T) == typeof(bool))
                return (true, (T)(object)true);

            return (true, JsonSerializer.Deserialize<T>(responseContent, JsonOptions));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Sellasist {Method} /{Endpoint} — błąd zapytania", method.Method, endpoint);
            return (false, default);
        }
    }

    // === ORDER CREATION ===

    public async Task<SellasistCreateOrderResponse?> CreateOrderAsync(SellasistCreateOrderRequest request)
        => await SendRequestAsync<SellasistCreateOrderResponse>("orders", HttpMethod.Post, request);

    public async Task<(int StatusCode, string RawBody, SellasistCreateOrderResponse? Parsed)> CreateOrderRawAsync(SellasistCreateOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(_config.ApiToken))
        {
            logger.LogError("Sellasist API token is empty");
            return (0, "(API token is empty)", null);
        }

        await ApplyThrottleAsync();

        var url = $"{_config.BaseUrl}/orders";
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
        httpRequest.Headers.Add("apiKey", _config.ApiToken);
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var requestJson = JsonSerializer.Serialize(request, JsonOptions);
        httpRequest.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        using var client = httpClientFactory.CreateClient("SellasistApi");
        using var response = await client.SendAsync(httpRequest);
        var body = await response.Content.ReadAsStringAsync();

        SellasistCreateOrderResponse? parsed = null;
        if (response.IsSuccessStatusCode)
        {
            try { parsed = JsonSerializer.Deserialize<SellasistCreateOrderResponse>(body, JsonOptions); }
            catch (Exception ex) { logger.LogError(ex, "Failed to deserialize Sellasist CreateOrder response"); }
        }
        else
        {
            logger.LogWarning("Sellasist POST /orders failed: {Status} {Body}", (int)response.StatusCode, body);
        }

        return ((int)response.StatusCode, body, parsed);
    }

    // === ORDERS ===

    public async Task<SellasistOrderResponse?> GetOrderAsync(int orderId)
        => await SendRequestAsync<SellasistOrderResponse>($"orders/{orderId}", HttpMethod.Get);

    /// <inheritdoc />
    public async Task<SellasistOrderFetchResult> GetOrderDetailedAsync(int orderId)
    {
        var result = new SellasistOrderFetchResult();

        if (string.IsNullOrWhiteSpace(_config.ApiToken))
        {
            logger.LogError("Sellasist API token is empty");
            result.Error = "brak tokenu API w konfiguracji";
            return result;
        }

        await ApplyThrottleAsync();

        var url = $"{_config.BaseUrl}/orders/{orderId}";

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("apiKey", _config.ApiToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var client = httpClientFactory.CreateClient("SellasistApi");
            using var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            result.HttpStatus = (int)response.StatusCode;

            if (!response.IsSuccessStatusCode)
            {
                var snippet = responseContent.Length > 300 ? responseContent[..300] : responseContent;
                result.Error = $"HTTP {(int)response.StatusCode} — {snippet}";
                if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    logger.LogWarning("Sellasist GET orders/{OrderId} failed: {Status} {Body}",
                        orderId, (int)response.StatusCode, responseContent);
                }
                return result;
            }

            try
            {
                result.Order = JsonSerializer.Deserialize<SellasistOrderResponse>(responseContent, JsonOptions);
                if (result.Order is null) result.Error = "pusta odpowiedź (JSON null)";
            }
            catch (Exception ex)
            {
                // Message wyjątku System.Text.Json zawiera ścieżkę pola (Path: $.carts[0]...) — kluczowa diagnostyka
                // (realny case: option_id Temu przekraczające int → cały GET „znikał" jako null bez śladu).
                logger.LogError(ex, "Failed to deserialize Sellasist response from /orders/{OrderId}", orderId);
                result.Error = $"błąd parsowania odpowiedzi: {ex.Message}";
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Sellasist GET orders/{OrderId} — błąd połączenia", orderId);
            result.Error = $"błąd połączenia: {ex.Message}";
        }

        return result;
    }

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

    public async Task<(bool Success, List<SellasistOrderResponse> Orders)> TryGetOrdersByStatusAsync(int statusId, int limit = 100)
    {
        var all = new List<SellasistOrderResponse>();
        int offset = 0;

        while (true)
        {
            var (ok, batch) = await TrySendRequestAsync<List<SellasistOrderResponse>>(
                $"orders?offset={offset}&limit={limit}&status_id={statusId}", HttpMethod.Get);

            // Błąd w środku stronicowania = lista NIEZNANA — nie wolno zwrócić częściowej jako pełnej
            // (konsument mógłby uznać ucięty zbiór za kompletny i stabilny).
            if (!ok)
                return (false, all);

            if (batch is { Count: > 0 })
            {
                all.AddRange(batch);
                if (batch.Count < limit) break;
                offset += limit;
            }
            else break;
        }
        return (true, all);
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

    public async Task<(bool Success, List<SellasistOrderResponse> Orders)> TryGetOrdersWithCartsByStatusAsync(int statusId, int limit = 100)
    {
        var all = new List<SellasistOrderResponse>();
        int offset = 0;

        while (true)
        {
            var (ok, batch) = await TrySendRequestAsync<List<SellasistOrderResponse>>(
                $"orders_with_carts?offset={offset}&limit={limit}&status_id={statusId}", HttpMethod.Get);

            // Błąd w środku stronicowania = lista NIEZNANA — nie wolno zwrócić częściowej jako pełnej.
            if (!ok)
                return (false, all);

            if (batch is { Count: > 0 })
            {
                all.AddRange(batch);
                if (batch.Count < limit) break;
                offset += limit;
            }
            else break;
        }
        return (true, all);
    }

    public async Task<List<SellasistOrderResponse>> GetOrdersAsync(DateTime dateFrom, int limit = 50)
    {
        var all = new List<SellasistOrderResponse>();
        int offset = 0;
        bool hasMore = true;
        var dateFromStr = dateFrom.ToString("yyyy-MM-dd");

        while (hasMore)
        {
            var batch = await SendRequestAsync<List<SellasistOrderResponse>>(
                $"orders?offset={offset}&limit={limit}&date_from={dateFromStr}", HttpMethod.Get);

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

    /// <summary>Zamówienia z oknem czasowym razem z koszykami — patrz opis w ISellasistService.
    /// Okno to [dateFrom, dateToExclusive); 404 przy wyjściu poza zbiór jest tu normalnym końcem
    /// paginacji (SendRequestAsync zamienia go na null bez logowania ostrzeżenia).</summary>
    public async Task<List<SellasistOrderResponse>> GetOrdersWithCartsByDateAsync(
        DateTime dateFrom, DateTime dateToExclusive, int limit = 100)
    {
        var all = new List<SellasistOrderResponse>();
        int offset = 0;
        bool hasMore = true;
        var od = dateFrom.ToString("yyyy-MM-dd");
        var doWyl = dateToExclusive.ToString("yyyy-MM-dd");

        while (hasMore)
        {
            var batch = await SendRequestAsync<List<SellasistOrderResponse>>(
                $"orders_with_carts?offset={offset}&limit={limit}&date_from={od}&date_to={doWyl}", HttpMethod.Get);

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

    public async Task<bool> UpdateOrderBillAddressAsync(int orderId, SellasistUpdateBillAddressRequest address)
        => await SendRequestAsync<bool>($"orders/{orderId}", HttpMethod.Put, new { bill_address = address });

    public async Task<bool> UpdateOrderAsync(int orderId, object body)
        => await SendRequestAsync<bool>($"orders/{orderId}", HttpMethod.Put, body);

    // === ORDER LINES ===

    public async Task<SellasistCreateOrderLineResponse?> CreateOrderLineAsync(SellasistOrderLineRequest request)
        => await SendRequestAsync<SellasistCreateOrderLineResponse>("orders_lines", HttpMethod.Post, request);

    public async Task<bool> UpdateOrderLineAsync(int lineId, SellasistOrderLineRequest request)
        => await SendRequestAsync<bool>($"orders_lines/{lineId}", HttpMethod.Put, request);

    public async Task<bool> UpdateOrderLineRawAsync(int lineId, object body)
        => await SendRequestAsync<bool>($"orders_lines/{lineId}", HttpMethod.Put, body);

    public async Task<bool> UpdateOrderLineAdditionalFieldsAsync(int lineId, IEnumerable<SellasistFieldUpdate> fields)
        => await SendRequestAsync<bool>($"orders_lines/{lineId}", HttpMethod.Put,
            new SellasistUpdateFieldRequest { AdditionalFields = [.. fields] });

    public async Task<bool> DeleteOrderLineAsync(int lineId)
        => await SendRequestAsync<bool>($"orders_lines/{lineId}", HttpMethod.Delete);

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

    public async Task<SellasistOrdersBulkResponse?> UpdateOrdersBulkAsync(List<SellasistOrderBulkUpdateItem> orders)
    {
        // Limit endpointu: 1000 pozycji (HTTP 413 powyżej) — dzielimy zachowawczo po 500 i scalamy wyniki.
        const int chunkSize = 500;
        if (orders.Count <= chunkSize)
            return await SendRequestAsync<SellasistOrdersBulkResponse>("orders_bulk", HttpMethod.Put, orders);

        var mergedLines = new List<SellasistOrdersBulkLine>();
        var anySuccess = false;
        foreach (var chunk in orders.Chunk(chunkSize))
        {
            var response = await SendRequestAsync<SellasistOrdersBulkResponse>("orders_bulk", HttpMethod.Put, chunk.ToList());
            if (response is null)
                continue; // brak linii dla tej paczki — konsument potraktuje pozycje jako nieudane
            anySuccess = true;
            mergedLines.AddRange(response.Lines ?? []);
        }

        return anySuccess ? new SellasistOrdersBulkResponse { Status = "SUCCESS", Lines = mergedLines } : null;
    }

    public async Task<SellasistOrdershipmentDetail?> GetOrdershipmentAsync(string uuidOrId)
        => await SendRequestAsync<SellasistOrdershipmentDetail>(
            $"ordersshipments/{Uri.EscapeDataString(uuidOrId)}", HttpMethod.Get);

    // === PRODUCTS ===

    public async Task<SellasistCreateProductResponse?> CreateProductAsync(SellasistCreateProductRequest request)
        => await SendRequestAsync<SellasistCreateProductResponse>("products", HttpMethod.Post, request);

    public async Task<SellasistCreateImageResponse?> AddProductImageAsync(SellasistCreateImageRequest request)
        => await SendRequestAsync<SellasistCreateImageResponse>("images", HttpMethod.Post, request);

    public async Task<List<SellasistProductImageItem>> GetProductImagesAsync(int productId)
        => await SendRequestAsync<List<SellasistProductImageItem>>($"images/{productId}", HttpMethod.Get) ?? [];

    public async Task<bool> DeleteProductImageAsync(int imageId)
        => await SendRequestAsync<bool>($"images/{imageId}", HttpMethod.Delete);

    public async Task<bool> UpdateProductQuantityAsync(int productId, string quantity)
        => await SendRequestAsync<bool>($"products/{productId}", HttpMethod.Put, new { quantity });

    public async Task<bool> UpdateProductAsync(int productId, object body)
        => await SendRequestAsync<bool>($"products/{productId}", HttpMethod.Put, body);

    public async Task<SellasistProductBulkUpdateResponse?> UpdateProductsBulkAsync(List<SellasistProductBulkUpdateItem> items)
        => await SendRequestAsync<SellasistProductBulkUpdateResponse>("products_bulk", HttpMethod.Put, items);

    /// <summary>Pełna lista produktów z /products_bulk (paginacja po <paramref name="limit"/>).
    /// ⚠️ Rzuca wyjątkiem, gdy którejś strony nie udało się pobrać: lista urwana w połowie wygląda jak
    /// kompletna, a konsument (cache katalogu) skasowałby na jej podstawie „brakujące" produkty i przy
    /// najbliższej synchronizacji utworzył duplikaty. Strony czytamy z dłuższym timeoutem.</summary>
    public async Task<List<SellasistProductBulkItem>> GetProductsBulkAsync(int limit = 500)
    {
        var all = new List<SellasistProductBulkItem>();
        int offset = 0;

        while (true)
        {
            var (ok, batch) = await TrySendRequestAsync<List<SellasistProductBulkItem>>(
                $"products_bulk?offset={offset}&limit={limit}", HttpMethod.Get, timeout: ListTimeout);

            if (!ok)
                throw new HttpRequestException(
                    $"Sellasist /products_bulk: nie udało się pobrać strony offset={offset} — lista produktów byłaby niepełna, przerywam.");

            if (batch is not { Count: > 0 }) break;

            all.AddRange(batch);
            if (batch.Count < limit) break;
            offset += limit;
        }
        return all;
    }

    public async Task<List<SellasistProductStock>> GetProductsStockAsync(int limit = 100, CancellationToken ct = default)
    {
        var all = new List<SellasistProductStock>();
        int offset = 0;

        while (true)
        {
            ct.ThrowIfCancellationRequested();

            var (ok, batch) = await TrySendRequestAsync<List<SellasistProductStock>>(
                $"products_stock?offset={offset}&limit={limit}", HttpMethod.Get, timeout: ListTimeout);

            if (!ok)
                throw new HttpRequestException(
                    $"Sellasist /products_stock: nie udało się pobrać strony offset={offset} — lista stanów byłaby niepełna, przerywam.");

            if (batch is not { Count: > 0 }) break;

            all.AddRange(batch);
            if (batch.Count < limit) break;
            offset += limit;
        }
        return all;
    }

    public async Task<List<SellasistProductStock>> GetProductsStockPageAsync(int offset = 0, int limit = 100, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var strona = await SendRequestAsync<List<SellasistProductStock>>(
            $"products_stock?offset={offset}&limit={limit}", HttpMethod.Get);
        return strona ?? [];
    }

    public async Task<SellasistProductStock?> GetProductStockBySymbolAsync(string symbol, CancellationToken ct = default)
    {
        // Endpoint z filtrem zwraca tablicę (zwykle 0 lub 1 element); 404 = brak produktu.
        var lista = await SendRequestAsync<List<SellasistProductStock>>(
            $"products_stock?symbol={Uri.EscapeDataString(symbol)}", HttpMethod.Get);
        return lista is { Count: > 0 } ? lista[0] : null;
    }

    public async Task<List<SellasistProductStock>> UpdateProductsStockAsync(List<SellasistProductStockUpdate> items, CancellationToken ct = default)
    {
        // Twardy limit endpointu to 1000 pozycji (powyżej HTTP 422) — dzielimy zachowawczo po 500.
        const int chunkSize = 500;
        var wynik = new List<SellasistProductStock>();

        foreach (var chunk in items.Chunk(chunkSize))
        {
            ct.ThrowIfCancellationRequested();

            var request = new SellasistProductStockUpdateRequest { Products = chunk.ToList() };
            var response = await SendRequestAsync<List<SellasistProductStock>>(
                "products_stock", HttpMethod.Put, request, timeout: ListTimeout);

            if (response is not null) wynik.AddRange(response);
        }

        return wynik;
    }

    public async Task<List<SellasistProductListItem>> GetProductsAsync(int limit = 100)
    {
        var all = new List<SellasistProductListItem>();
        int offset = 0;
        bool hasMore = true;

        while (hasMore)
        {
            var batch = await SendRequestAsync<List<SellasistProductListItem>>(
                $"products?offset={offset}&limit={limit}", HttpMethod.Get, timeout: ListTimeout);

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
                $"categories?offset={offset}&limit={limit}", HttpMethod.Get, timeout: ListTimeout);

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
                $"manufacturers?offset={offset}&limit={limit}", HttpMethod.Get, timeout: ListTimeout);

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

    public async Task<List<SellasistCountry>> GetCountriesAsync()
        => await SendRequestAsync<List<SellasistCountry>>("countries", HttpMethod.Get) ?? [];

    // === EXTRA FIELDS ===

    public async Task<List<SellasistExtraFieldResponse>> GetExtraFieldsAsync()
    {
        var result = await SendRequestAsync<List<SellasistExtraFieldResponse>>("orders_fields", HttpMethod.Get);
        return result ?? [];
    }

    // === OPERATION DOCUMENTS (PZ, WZ) ===

    public async Task<List<SellasistOperationDocumentListItem>> GetOperationDocumentsAsync(
        string type = "stock", string subtype = "admission", string sort = "desc", int? seriesId = null)
    {
        var url = $"operationdocuments?type={type}&subtype={subtype}&sort={sort}";
        if (seriesId is > 0) url += $"&series_id={seriesId.Value}";
        var result = await SendRequestAsync<List<SellasistOperationDocumentListItem>>(url, HttpMethod.Get);
        return result ?? [];
    }

    /// <summary>Pobiera szczegóły dokumentu magazynowego. UWAGA: Sellasist API zwraca dla detail
    /// TABLICĘ 1-elementową <c>[{...}]</c> zamiast pojedynczego obiektu — deserializujemy jako List
    /// i bierzemy pierwszy element (FirstOrDefault).</summary>
    public async Task<SellasistOperationDocumentDetail?> GetOperationDocumentAsync(int documentId)
    {
        var list = await SendRequestAsync<List<SellasistOperationDocumentDetail>>(
            $"operationdocuments/{documentId}", HttpMethod.Get);
        return list?.FirstOrDefault();
    }

    public async Task<List<SellasistOperationDocumentSeriesListItem>> GetOperationDocumentSeriesAsync()
    {
        var result = await SendRequestAsync<List<SellasistOperationDocumentSeriesListItem>>(
            "operationdocuments_series", HttpMethod.Get);
        return result ?? [];
    }

    public async Task<SellasistCreateOperationDocumentResponse?> CreateOperationDocumentAsync(SellasistCreateOperationDocumentRequest request)
        => await SendRequestAsync<SellasistCreateOperationDocumentResponse>("operationdocuments", HttpMethod.Post, request);

    /// <summary>PUT /operationdocuments/{id} — partial update. Sellasist akceptuje <c>{"comments":"..."}</c>
    /// jako tagging dokumentu po utworzeniu (POST z comments łamie parser, ale PUT działa).</summary>
    public async Task<bool> UpdateOperationDocumentAsync(int documentId, object body)
        => await SendRequestAsync<bool>($"operationdocuments/{documentId}", HttpMethod.Put, body);

    // === CLOUD PRINT ===
    // /printfile i /printerpoints są na głównym API Sellasist (ten sam host i apiKey co reszta metod).
    // Zlecenie wydruku odbiera i drukuje aplikacja desktopowa Sellasist Cloud Print na stanowisku WMS.

    public async Task<(int StatusCode, string RawBody)> PrintFileAsync(SellasistPrintFileRequest request)
    {
        if (string.IsNullOrWhiteSpace(_config.ApiToken))
        {
            logger.LogError("Sellasist API token is empty");
            return (0, "(API token is empty)");
        }

        await ApplyThrottleAsync();

        var url = $"{_config.BaseUrl}/printfile";
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
        httpRequest.Headers.Add("apiKey", _config.ApiToken);
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var requestJson = JsonSerializer.Serialize(request, JsonOptions);
        httpRequest.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // Dedykowany klient z długim timeoutem — duże dokumenty base64 przekraczają standardowe 30 s.
        using var client = httpClientFactory.CreateClient("SellasistApiPrint");
        using var response = await client.SendAsync(httpRequest);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            logger.LogWarning("Sellasist POST /printfile failed: {Status} {Body}", (int)response.StatusCode, body);

        return ((int)response.StatusCode, body);
    }

    public async Task<List<SellasistPrinterPoint>> GetPrinterPointsAsync()
        => await SendRequestAsync<List<SellasistPrinterPoint>>("printerpoints", HttpMethod.Get) ?? [];
}
