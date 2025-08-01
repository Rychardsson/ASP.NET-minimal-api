using Microsoft.Extensions.Diagnostics.HealthChecks;
using MinimalApi.Infraestrutura.Db;
using Microsoft.EntityFrameworkCore;

namespace MinimalApi.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly DbContexto _context;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(DbContexto context, ILogger<DatabaseHealthCheck> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Database.CanConnectAsync(cancellationToken);
            
            var veiculosCount = await _context.Veiculos.CountAsync(cancellationToken);
            var adminsCount = await _context.Administradores.CountAsync(cancellationToken);

            var data = new Dictionary<string, object>
            {
                { "veiculos_count", veiculosCount },
                { "administradores_count", adminsCount },
                { "database_server", _context.Database.GetConnectionString() ?? "Unknown" }
            };

            return HealthCheckResult.Healthy("Database is healthy", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy("Database is unhealthy", ex);
        }
    }
}

public class MemoryHealthCheck : IHealthCheck
{
    private readonly ILogger<MemoryHealthCheck> _logger;

    public MemoryHealthCheck(ILogger<MemoryHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var workingSet = process.WorkingSet64;
            var privateMemory = process.PrivateMemorySize64;

            var data = new Dictionary<string, object>
            {
                { "working_set_mb", workingSet / 1024 / 1024 },
                { "private_memory_mb", privateMemory / 1024 / 1024 },
                { "gc_gen0_collections", GC.CollectionCount(0) },
                { "gc_gen1_collections", GC.CollectionCount(1) },
                { "gc_gen2_collections", GC.CollectionCount(2) }
            };

            // Alerta se usar mais de 500MB
            if (workingSet > 500 * 1024 * 1024)
            {
                return Task.FromResult(HealthCheckResult.Degraded("High memory usage detected", null, data));
            }

            return Task.FromResult(HealthCheckResult.Healthy("Memory usage is normal", data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Memory health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy("Memory health check failed", ex));
        }
    }
}

public static class HealthCheckExtensions
{
    public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "database", "sql" })
            .AddCheck<MemoryHealthCheck>("memory", tags: new[] { "memory", "performance" })
            .AddCheck("self", () => HealthCheckResult.Healthy("API is running"), tags: new[] { "api" });

        // Se tiver Redis configurado
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddHealthChecks()
                .AddRedis(redisConnection, name: "redis", tags: new[] { "cache", "redis" });
        }

        return services;
    }

    public static WebApplication MapCustomHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = WriteHealthCheckResponse,
            AllowCachingResponses = false
        });

        app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteHealthCheckResponse
        });

        app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = WriteHealthCheckResponse
        });

        return app;
    }

    private static async Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            duration = report.TotalDuration,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                duration = entry.Value.Duration,
                description = entry.Value.Description,
                data = entry.Value.Data,
                exception = entry.Value.Exception?.Message,
                tags = entry.Value.Tags
            })
        };

        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        }));
    }
}
