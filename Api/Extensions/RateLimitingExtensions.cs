using Microsoft.Extensions.Caching.Memory;

namespace MinimalApi.Extensions;

/// <summary>
/// Extensões para configuração de Rate Limiting básico
/// </summary>
public static class RateLimitingExtensions
{
    /// <summary>
    /// Adiciona middleware simples de rate limiting
    /// </summary>
    public static IServiceCollection AddCustomRateLimiting(this IServiceCollection services)
    {
        services.AddMemoryCache();
        return services;
    }

    /// <summary>
    /// Adiciona middleware de rate limiting
    /// </summary>
    public static IApplicationBuilder UseCustomRateLimiting(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RateLimitingMiddleware>();
    }
}

/// <summary>
/// Middleware básico de rate limiting
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    
    // Configurações
    private readonly int _requestLimit = 100; // 100 requests
    private readonly TimeSpan _timeWindow = TimeSpan.FromMinutes(1); // por minuto

    public RateLimitingMiddleware(RequestDelegate next, IMemoryCache cache, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        var key = $"rate_limit_{clientId}";

        if (_cache.TryGetValue(key, out int requestCount))
        {
            if (requestCount >= _requestLimit)
            {
                _logger.LogWarning("Rate limit exceeded for client {ClientId}", clientId);
                context.Response.StatusCode = 429; // Too Many Requests
                await context.Response.WriteAsync("Muitas requisições. Tente novamente mais tarde.");
                return;
            }

            _cache.Set(key, requestCount + 1, _timeWindow);
        }
        else
        {
            _cache.Set(key, 1, _timeWindow);
        }

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Usa IP + User-Agent como identificador
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = context.Request.Headers.UserAgent.ToString();
        return $"{ip}_{userAgent.GetHashCode()}";
    }
}
