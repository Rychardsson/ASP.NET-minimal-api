using System.Net;
using System.Text.Json;
using FluentValidation;

namespace MinimalApi.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse();

        switch (exception)
        {
            case ValidationException validationEx:
                response.Title = "Erro de Validação";
                response.Status = (int)HttpStatusCode.BadRequest;
                response.Detail = "Um ou mais campos são inválidos";
                response.Errors = validationEx.Errors.GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.ErrorMessage).ToArray()
                    );
                break;

            case UnauthorizedAccessException:
                response.Title = "Não Autorizado";
                response.Status = (int)HttpStatusCode.Unauthorized;
                response.Detail = "Você não tem permissão para acessar este recurso";
                break;

            case KeyNotFoundException:
                response.Title = "Recurso Não Encontrado";
                response.Status = (int)HttpStatusCode.NotFound;
                response.Detail = "O recurso solicitado não foi encontrado";
                break;

            case TimeoutException:
                response.Title = "Timeout";
                response.Status = (int)HttpStatusCode.RequestTimeout;
                response.Detail = "A operação excedeu o tempo limite";
                break;

            case InvalidOperationException:
                response.Title = "Operação Inválida";
                response.Status = (int)HttpStatusCode.BadRequest;
                response.Detail = exception.Message;
                break;

            default:
                response.Title = "Erro Interno do Servidor";
                response.Status = (int)HttpStatusCode.InternalServerError;
                response.Detail = "Ocorreu um erro interno. Tente novamente mais tarde.";
                break;
        }

        context.Response.StatusCode = response.Status;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(response, jsonOptions);
        await context.Response.WriteAsync(json);
    }
}

public class ErrorResponse
{
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Detail { get; set; } = string.Empty;
    public string? Instance { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, string[]>? Errors { get; set; }
}
