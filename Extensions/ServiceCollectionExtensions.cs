using Microsoft.Extensions.DependencyInjection;

namespace Sellasist.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSellasist(this IServiceCollection services)
    {
        services.AddHttpClient("SellasistApi", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Osobny klient dla POST /printfile — wielomegabajtowe dokumenty base64 (np. scalone etykiety
        // dużej partii) nie mieszczą się w 30 s; 30-sekundowy timeout dawał deterministyczną porażkę.
        services.AddHttpClient("SellasistApiPrint", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(300);
        });

        return services;
    }
}
