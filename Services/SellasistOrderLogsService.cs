using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Sellasist.Config;
using Sellasist.DTOs;
using Sellasist.Interfaces;

namespace Sellasist.Services;

/// <summary>Implementacja <see cref="ISellasistOrderLogsService"/>. Używa tego samego named HttpClient
/// ("SellasistApi") co <see cref="SellasistService"/>, ale trzyma własną kopię <see cref="SellasistConfig"/>
/// żeby konsument mógł konfigurować credentials niezależnie (multi-tenant polling).</summary>
public class SellasistOrderLogsService(
    IHttpClientFactory httpClientFactory,
    SellasistConfig config,
    ILogger<SellasistOrderLogsService> logger)
    : ISellasistOrderLogsService
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

    /// <inheritdoc />
    public async Task<List<SellasistOrderLog>?> GetOrderLogsAsync(string operationTag, DateTime dateFrom, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_config.ApiToken))
        {
            logger.LogError("Sellasist API token is empty");
            return null;
        }

        var dateFromStr = dateFrom.ToString("yyyy-MM-dd HH:mm:ss");
        var endpoint = $"orders_logs?operation_tag={Uri.EscapeDataString(operationTag)}&date_from={Uri.EscapeDataString(dateFromStr)}";
        var url = $"{_config.BaseUrl}/{endpoint}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("apiKey", _config.ApiToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var client = httpClientFactory.CreateClient("SellasistApi");
        using var response = await client.SendAsync(request, ct);
        var responseContent = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Sellasist GET /orders_logs failed (tag={Tag}, dateFrom={DateFrom}): {Status} {Body}",
                operationTag, dateFromStr, (int)response.StatusCode, responseContent);
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<List<SellasistOrderLog>>(responseContent, JsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to deserialize /orders_logs response (tag={Tag})", operationTag);
            return null;
        }
    }
}
