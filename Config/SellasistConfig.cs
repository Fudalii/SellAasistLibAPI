namespace Sellasist.Config;

public class SellasistConfig
{
    public string Username { get; set; } = string.Empty;
    public string ApiToken { get; set; } = string.Empty;
    public string BaseUrl => $"https://{Username}.sellasist.pl/api/v1";

    /// <summary>Minimalny odstęp (w ms) między kolejnymi requestami do API Sellasist. Chroni przed
    /// rate-limitem / blokadą endpointa przy bulkowych operacjach (np. pętla po setkach zamówień).
    /// 0 = bez throttlingu (default — zachowuje wsteczną kompatybilność dla projektów które same
    /// kontrolują tempo). Zalecane 200-500 ms dla aplikacji z wielokrotnymi pętlami w tle.</summary>
    public int MinDelayBetweenRequestsMs { get; set; } = 0;
}
