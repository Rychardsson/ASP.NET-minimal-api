using MinimalApi.Services;

namespace MinimalApi.BackgroundServices;

public class CacheWarmupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CacheWarmupService> _logger;

    public CacheWarmupService(IServiceProvider serviceProvider, ILogger<CacheWarmupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Cache Warmup Service iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
                
                // Pré-carregar dados frequentemente acessados
                await WarmupFrequentlyAccessedData(scope.ServiceProvider, cacheService);
                
                // Aguardar 30 minutos antes da próxima execução
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante warmup do cache");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private async Task WarmupFrequentlyAccessedData(IServiceProvider serviceProvider, ICacheService cacheService)
    {
        // Implementar lógica de pre-cache dos dados mais acessados
        _logger.LogInformation("Executando warmup do cache...");
        
        // Exemplo: pré-carregar estatísticas
        // var estatisticas = await GetEstatisticas(serviceProvider);
        // await cacheService.SetAsync("estatisticas:geral", estatisticas, TimeSpan.FromHours(1));
        
        _logger.LogInformation("Warmup do cache concluído");
    }
}

public class DatabaseCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseCleanupService> _logger;

    public DatabaseCleanupService(IServiceProvider serviceProvider, ILogger<DatabaseCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Database Cleanup Service iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Executar limpeza diariamente às 2h da manhã
                var now = DateTime.Now;
                var next2AM = now.Date.AddDays(1).AddHours(2);
                var delay = next2AM - now;

                if (delay.TotalMilliseconds > 0)
                {
                    await Task.Delay(delay, stoppingToken);
                }

                using var scope = _serviceProvider.CreateScope();
                await PerformDatabaseCleanup(scope.ServiceProvider);
                
                // Aguardar até o próximo dia
                await Task.Delay(TimeSpan.FromHours(23), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante limpeza do banco de dados");
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }

    private async Task PerformDatabaseCleanup(IServiceProvider serviceProvider)
    {
        _logger.LogInformation("Executando limpeza do banco de dados...");
        
        // Implementar lógicas de limpeza:
        // - Remover logs antigos
        // - Limpar dados temporários
        // - Otimizar índices
        
        _logger.LogInformation("Limpeza do banco de dados concluída");
        await Task.CompletedTask;
    }
}

public class HealthCheckBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HealthCheckBackgroundService> _logger;

    public HealthCheckBackgroundService(IServiceProvider serviceProvider, ILogger<HealthCheckBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Health Check Background Service iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                await PerformSystemHealthCheck(scope.ServiceProvider);
                
                // Verificar a cada 5 minutos
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante verificação de saúde do sistema");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    private async Task PerformSystemHealthCheck(IServiceProvider serviceProvider)
    {
        // Verificar métricas críticas:
        // - Uso de memória
        // - Conexões com banco
        // - Performance de endpoints críticos
        // - Status de serviços externos
        
        var process = System.Diagnostics.Process.GetCurrentProcess();
        var memoryUsage = process.WorkingSet64 / 1024 / 1024; // MB

        if (memoryUsage > 500)
        {
            _logger.LogWarning("Alto uso de memória detectado: {MemoryUsage}MB", memoryUsage);
        }

        await Task.CompletedTask;
    }
}
