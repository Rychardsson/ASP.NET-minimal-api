using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace MinimalApi.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddCustomRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Rate limit para login - mais restritivo
            options.AddFixedWindowLimiter("AuthPolicy", rateLimitOptions =>
            {
                rateLimitOptions.PermitLimit = 5;
                rateLimitOptions.Window = TimeSpan.FromMinutes(1);
                rateLimitOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                rateLimitOptions.QueueLimit = 3;
            });

            // Rate limit geral para API
            options.AddFixedWindowLimiter("GeneralPolicy", rateLimitOptions =>
            {
                rateLimitOptions.PermitLimit = 100;
                rateLimitOptions.Window = TimeSpan.FromMinutes(1);
                rateLimitOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                rateLimitOptions.QueueLimit = 10;
            });

            // Rate limit para criação de recursos
            options.AddFixedWindowLimiter("CreatePolicy", rateLimitOptions =>
            {
                rateLimitOptions.PermitLimit = 20;
                rateLimitOptions.Window = TimeSpan.FromMinutes(1);
                rateLimitOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                rateLimitOptions.QueueLimit = 5;
            });

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 1000,
                        Window = TimeSpan.FromHours(1)
                    }));

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = 429;
                await context.HttpContext.Response.WriteAsync("Muitas requisições. Tente novamente mais tarde.", token);
            };
        });

        return services;
    }
}
