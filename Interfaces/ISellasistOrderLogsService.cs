using Sellasist.Config;
using Sellasist.DTOs;

namespace Sellasist.Interfaces;

/// <summary>Serwis do pobierania logów operacji zamówień z Sellasist (GET /orders_logs).
/// Osobny od <see cref="ISellasistService"/> — używany głównie przez polling background service
/// który leniwie konfiguruje credentials per-user (multi-tenant).</summary>
public interface ISellasistOrderLogsService
{
    /// <summary>Ustawia konfiguracje dynamicznie (np. credentials z bazy per-user).</summary>
    void Configure(SellasistConfig newConfig);

    /// <summary>Pobiera logi operacji z Sellasist przefiltrowane po tagu i dacie.</summary>
    /// <param name="operationTag">Pojedynczy tag (np. "order_create"). Wielokrotne tagi nie są obsługiwane przez API —
    /// wywołaj metodę osobno dla każdego tagu.</param>
    /// <param name="dateFrom">Zwraca logi z <c>date >= dateFrom</c> (format przekazywany jako "yyyy-MM-dd HH:mm:ss"
    /// w czasie lokalnym Sellasist).</param>
    /// <returns>Lista logów lub null gdy request zawiódł (zaloguje warning).</returns>
    Task<List<SellasistOrderLog>?> GetOrderLogsAsync(string operationTag, DateTime dateFrom, CancellationToken ct = default);
}
