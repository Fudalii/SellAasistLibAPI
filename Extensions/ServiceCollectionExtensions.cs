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

        return services;
    }
}
