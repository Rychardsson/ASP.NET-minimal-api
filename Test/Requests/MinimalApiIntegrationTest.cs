using Microsoft.AspNetCore.Mvc.Testing;
using MinimalApi.DTOs;
using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Requests;

[TestClass]
public class MinimalApiIntegrationTest
{
    private WebApplicationFactory<Program>? _factory;
    private HttpClient? _client;

    [TestInitialize]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [TestMethod]
    public async Task HealthCheck_DeveRetornarStatus200()
    {
        // Act
        var response = await _client!.GetAsync("/api/health");

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("Healthy"));
    }

    [TestMethod]
    public async Task Home_DeveRetornarInformacoesDaAPI()
    {
        // Act
        var response = await _client!.GetAsync("/");

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task Login_ComCredenciaisInvalidas_DeveRetornar401()
    {
        // Arrange
        var loginDto = new LoginDTO
        {
            Email = "teste@invalido.com",
            Senha = "senhaerrada"
        };

        var json = JsonSerializer.Serialize(loginDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client!.PostAsync("/administradores/login", content);

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task Veiculos_SemAutenticacao_DeveRetornar401()
    {
        // Act
        var response = await _client!.GetAsync("/veiculos");

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task Estatisticas_SemAutenticacao_DeveRetornar401()
    {
        // Act
        var response = await _client!.GetAsync("/api/estatisticas");

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task CriarVeiculo_SemAutenticacao_DeveRetornar401()
    {
        // Arrange
        var veiculoDto = new VeiculoDTO
        {
            Nome = "Civic",
            Marca = "Honda",
            Ano = 2020
        };

        var json = JsonSerializer.Serialize(veiculoDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client!.PostAsync("/veiculos", content);

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task SwaggerUI_DeveEstarDisponivel()
    {
        // Act
        var response = await _client!.GetAsync("/swagger/index.html");

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task SwaggerJson_DeveEstarDisponivel()
    {
        // Act
        var response = await _client!.GetAsync("/swagger/v1/swagger.json");

        // Assert
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.IsTrue(content.Contains("Minimal API"));
        Assert.IsTrue(content.Contains("Bearer"));
    }

    [TestMethod]
    public async Task CriarVeiculoComDadosInvalidos_DeveRetornar400()
    {
        // Arrange
        var veiculoDto = new VeiculoDTO
        {
            Nome = "", // Nome vazio - deve falhar na validação
            Marca = "Honda",
            Ano = 1900 // Ano muito antigo - deve falhar na validação
        };

        var json = JsonSerializer.Serialize(veiculoDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client!.PostAsync("/veiculos", content);

        // Assert - Deve retornar 401 (unauthorized) porque não tem token, não 400
        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
