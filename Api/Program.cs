using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Enuns;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.ModelViews;
using MinimalApi.Dominio.Servicos;
using MinimalApi.DTOs;
using MinimalApi.Infraestrutura.Db;
using MinimalApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

var key = builder.Configuration.GetSection("Jwt").Get<string>();

// Configuração de Autenticação JWT
builder.Services.AddAuthentication(option => {
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option => {
    option.TokenValidationParameters = new TokenValidationParameters{
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key ?? "")),
        ValidateIssuer = false,
        ValidateAudience = false,
    };
});

builder.Services.AddAuthorization();

// Registro de Serviços
builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

// Configuração do Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Minimal API - Sistema de Veículos", 
        Version = "v1",
        Description = "API para gerenciamento de veículos e administradores usando Minimal APIs com autenticação JWT",
        Contact = new OpenApiContact
        {
            Name = "Sistema de Veículos",
            Email = "contato@sistemaveiculos.com"
        }
    });
    
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme{
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT no formato: Bearer {seu-token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme{
                Reference = new OpenApiReference 
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Configuração do Entity Framework
builder.Services.AddDbContext<DbContexto>(options => {
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql"))
    );
});

// Configuração do CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configuração do pipeline de requisições
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Minimal API v1");
        c.RoutePrefix = "";
        c.DocumentTitle = "Sistema de Veículos API";
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.UseCors();

#region Endpoints Home
app.MapGet("/", () => Results.Json(new Home()))
    .AllowAnonymous()
    .WithName("Home")
    .WithTags("Home")
    .WithSummary("Página inicial da API")
    .WithDescription("Retorna informações básicas sobre a API")
    .Produces<Home>(StatusCodes.Status200OK);

app.MapGet("/api/estatisticas", (IVeiculoServico veiculoServico, IAdministradorServico administradorServico) => {
    var totalVeiculos = veiculoServico.Todos(null).Count();
    var totalAdministradores = administradorServico.Todos(null).Count();
    
    var estatisticas = new EstatisticasAPI
    {
        TotalVeiculos = totalVeiculos,
        TotalAdministradores = totalAdministradores,
        VersaoAPI = "v1.0",
        DataConsulta = DateTime.Now,
        FuncionalidadesDisponiveis = new List<string>
        {
            "Gerenciamento de Veículos",
            "Autenticação JWT",
            "Controle de Acesso por Roles",
            "Documentação Swagger",
            "Paginação de Resultados",
            "Validações Automáticas"
        }
    };
    
    return Results.Ok(estatisticas);
})
.RequireAuthorization()
.WithName("EstatisticasAPI")
.WithTags("Home")
.WithSummary("Estatísticas da API")
.WithDescription("Retorna estatísticas gerais sobre o sistema")
.Produces<EstatisticasAPI>(StatusCodes.Status200OK);

app.MapGet("/api/health", () => Results.Ok(new { 
    Status = "Healthy", 
    Timestamp = DateTime.UtcNow,
    Version = "1.0.0"
}))
.AllowAnonymous()
.WithName("HealthCheck")
.WithTags("Home")
.WithSummary("Verificação de saúde da API")
.WithDescription("Endpoint para verificar se a API está funcionando corretamente");
#endregion

// Mapear endpoints usando extensões
app.MapAdministradorEndpoints();
app.MapVeiculoEndpoints();

app.Run();

// Torna a classe Program acessível para testes
public partial class Program { }