using Microsoft.AspNetCore.Mvc;

namespace MinimalApi.Extensions;

public static class ApiVersioningExtensions
{
    public static IServiceCollection AddApiVersioningSupport(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new QueryStringApiVersionReader("version"),
                new HeaderApiVersionReader("X-Version"),
                new UrlSegmentApiVersionReader()
            );
        });

        services.AddVersionedApiExplorer(setup =>
        {
            setup.GroupNameFormat = "'v'VVV";
            setup.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    public static WebApplication MapVersionedEndpoints(this WebApplication app)
    {
        var v1 = app.MapGroup("/api/v1")
            .HasApiVersion(1, 0)
            .WithTags("V1");

        var v2 = app.MapGroup("/api/v2")
            .HasApiVersion(2, 0)
            .WithTags("V2");

        // Endpoints V1 (mantém compatibilidade)
        MapV1Endpoints(v1);
        
        // Endpoints V2 (novas funcionalidades)
        MapV2Endpoints(v2);

        return app;
    }

    private static void MapV1Endpoints(RouteGroupBuilder group)
    {
        group.MapGet("/info", () => new { Version = "1.0", Status = "Stable" })
            .WithSummary("API V1 Info")
            .WithDescription("Informações sobre a versão 1.0 da API");
    }

    private static void MapV2Endpoints(RouteGroupBuilder group)
    {
        group.MapGet("/info", () => new { 
            Version = "2.0", 
            Status = "Latest",
            Features = new[] { "Enhanced Security", "Better Performance", "New Endpoints" }
        })
        .WithSummary("API V2 Info")
        .WithDescription("Informações sobre a versão 2.0 da API");

        group.MapGet("/features", () => new {
            NewFeatures = new[]
            {
                "Advanced Caching",
                "Rate Limiting",
                "Enhanced Logging",
                "Health Checks",
                "Metrics"
            }
        })
        .WithSummary("V2 Features")
        .WithDescription("Lista das novas funcionalidades da V2");
    }
}
