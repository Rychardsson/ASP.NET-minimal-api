using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.ModelViews;
using MinimalApi.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MinimalApi.Extensions;

public static class EndpointsExtensions
{
    public static void MapAdministradorEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/administradores")
            .WithTags("Administradores");

        group.MapPost("/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico, IConfiguration configuration) => {
            var adm = administradorServico.Login(loginDTO);
            if(adm != null)
            {
                string token = GerarTokenJwt(adm, configuration);
                return Results.Ok(new AdministradorLogado
                {
                    Email = adm.Email,
                    Perfil = adm.Perfil,
                    Token = token
                });
            }
            else
                return Results.Unauthorized();
        })
        .AllowAnonymous()
        .WithName("LoginAdministrador")
        .WithSummary("Fazer login como administrador")
        .WithDescription("Realiza o login e retorna um token JWT para acesso às funcionalidades protegidas")
        .Produces<AdministradorLogado>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/", ([FromQuery] int? pagina, IAdministradorServico administradorServico) => {
            var adms = new List<AdministradorModelView>();
            var administradores = administradorServico.Todos(pagina);
            foreach(var adm in administradores)
            {
                adms.Add(new AdministradorModelView{
                    Id = adm.Id,
                    Email = adm.Email,
                    Perfil = adm.Perfil
                });
            }
            return Results.Ok(adms);
        })
        .RequireAuthorization()
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
        .WithName("ListarAdministradores")
        .WithSummary("Listar todos os administradores")
        .WithDescription("Lista todos os administradores cadastrados (apenas para Administradores)")
        .Produces<List<AdministradorModelView>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) => {
            var administrador = administradorServico.BuscaPorId(id);
            if(administrador == null) return Results.NotFound();
            return Results.Ok(new AdministradorModelView{
                    Id = administrador.Id,
                    Email = administrador.Email,
                    Perfil = administrador.Perfil
            });
        })
        .RequireAuthorization()
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
        .WithName("BuscarAdministradorPorId")
        .WithSummary("Buscar administrador por ID")
        .WithDescription("Busca um administrador específico pelo seu ID")
        .Produces<AdministradorModelView>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) => {
            var validacao = new ErrosDeValidacao{
                Mensagens = new List<string>()
            };

            if(string.IsNullOrEmpty(administradorDTO.Email))
                validacao.Mensagens.Add("Email não pode ser vazio");
            if(string.IsNullOrEmpty(administradorDTO.Senha))
                validacao.Mensagens.Add("Senha não pode ser vazia");
            if(administradorDTO.Perfil == null)
                validacao.Mensagens.Add("Perfil não pode ser vazio");

            if(validacao.Mensagens.Count > 0)
                return Results.BadRequest(validacao);
            
            var administrador = new Administrador{
                Email = administradorDTO.Email,
                Senha = administradorDTO.Senha,
                Perfil = administradorDTO.Perfil.ToString() ?? MinimalApi.Dominio.Enuns.Perfil.Editor.ToString()
            };

            administradorServico.Incluir(administrador);

            return Results.Created($"/administradores/{administrador.Id}", new AdministradorModelView{
                Id = administrador.Id,
                Email = administrador.Email,
                Perfil = administrador.Perfil
            });
            
        })
        .RequireAuthorization()
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
        .WithName("CriarAdministrador")
        .WithSummary("Criar novo administrador")
        .WithDescription("Cria um novo administrador no sistema")
        .Produces<AdministradorModelView>(StatusCodes.Status201Created)
        .Produces<ErrosDeValidacao>(StatusCodes.Status400BadRequest);
    }

    public static void MapVeiculoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/veiculos")
            .WithTags("Veículos");

        group.MapPost("/", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) => {
            var validacao = ValidaVeiculoDTO(veiculoDTO);
            if(validacao.Mensagens.Count > 0)
                return Results.BadRequest(validacao);
            
            var veiculo = new Veiculo{
                Nome = veiculoDTO.Nome,
                Marca = veiculoDTO.Marca,
                Ano = veiculoDTO.Ano
            };
            veiculoServico.Incluir(veiculo);

            return Results.Created($"/veiculos/{veiculo.Id}", veiculo);
        })
        .RequireAuthorization()
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" })
        .WithName("CriarVeiculo")
        .WithSummary("Criar novo veículo")
        .WithDescription("Adiciona um novo veículo ao sistema")
        .Produces<Veiculo>(StatusCodes.Status201Created)
        .Produces<ErrosDeValidacao>(StatusCodes.Status400BadRequest);

        group.MapGet("/", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) => {
            var veiculos = veiculoServico.Todos(pagina);
            return Results.Ok(veiculos);
        })
        .RequireAuthorization()
        .WithName("ListarVeiculos")
        .WithSummary("Listar todos os veículos")
        .WithDescription("Lista todos os veículos cadastrados com paginação opcional")
        .Produces<List<Veiculo>>(StatusCodes.Status200OK);

        group.MapGet("/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) => {
            var veiculo = veiculoServico.BuscaPorId(id);
            if(veiculo == null) return Results.NotFound();
            return Results.Ok(veiculo);
        })
        .RequireAuthorization()
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" })
        .WithName("BuscarVeiculoPorId")
        .WithSummary("Buscar veículo por ID")
        .WithDescription("Busca um veículo específico pelo seu ID")
        .Produces<Veiculo>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) => {
            var veiculo = veiculoServico.BuscaPorId(id);
            if(veiculo == null) return Results.NotFound();
            
            var validacao = ValidaVeiculoDTO(veiculoDTO);
            if(validacao.Mensagens.Count > 0)
                return Results.BadRequest(validacao);
            
            veiculo.Nome = veiculoDTO.Nome;
            veiculo.Marca = veiculoDTO.Marca;
            veiculo.Ano = veiculoDTO.Ano;

            veiculoServico.Atualizar(veiculo);

            return Results.Ok(veiculo);
        })
        .RequireAuthorization()
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
        .WithName("AtualizarVeiculo")
        .WithSummary("Atualizar veículo")
        .WithDescription("Atualiza os dados de um veículo existente (apenas Administradores)")
        .Produces<Veiculo>(StatusCodes.Status200OK)
        .Produces<ErrosDeValidacao>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) => {
            var veiculo = veiculoServico.BuscaPorId(id);
            if(veiculo == null) return Results.NotFound();

            veiculoServico.Apagar(veiculo);

            return Results.NoContent();
        })
        .RequireAuthorization()
        .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
        .WithName("DeletarVeiculo")
        .WithSummary("Deletar veículo")
        .WithDescription("Remove um veículo do sistema (apenas Administradores)")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }

    private static string GerarTokenJwt(Administrador administrador, IConfiguration configuration)
    {
        var key = configuration.GetSection("Jwt").Get<string>();
        if(string.IsNullOrEmpty(key)) return string.Empty;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>()
        {
            new Claim("Email", administrador.Email),
            new Claim("Perfil", administrador.Perfil),
            new Claim(ClaimTypes.Role, administrador.Perfil),
        };
        
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static ErrosDeValidacao ValidaVeiculoDTO(VeiculoDTO veiculoDTO)
    {
        var validacao = new ErrosDeValidacao{
            Mensagens = new List<string>()
        };

        if(string.IsNullOrEmpty(veiculoDTO.Nome))
            validacao.Mensagens.Add("O nome não pode ser vazio");

        if(string.IsNullOrEmpty(veiculoDTO.Marca))
            validacao.Mensagens.Add("A Marca não pode ficar em branco");

        if(veiculoDTO.Ano < 1950)
            validacao.Mensagens.Add("Veículo muito antigo, aceito somete anos superiores a 1950");

        return validacao;
    }
}
