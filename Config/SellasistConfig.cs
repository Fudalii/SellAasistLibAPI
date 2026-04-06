namespace Sellasist.Config;

public class SellasistConfig
{
    public string Username { get; set; } = string.Empty;
    public string ApiToken { get; set; } = string.Empty;
    public string BaseUrl => $"https://{Username}.sellasist.pl/api/v1";
}
