using FluentValidation;
using MinimalApi.DTOs;

namespace MinimalApi.Validators;

public class VeiculoValidator : AbstractValidator<VeiculoDTO>
{
    public VeiculoValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório")
            .Length(2, 150).WithMessage("O nome deve ter entre 2 e 150 caracteres")
            .Matches("^[a-zA-Z0-9\\s\\-\\.]+$").WithMessage("Nome contém caracteres inválidos");

        RuleFor(x => x.Marca)
            .NotEmpty().WithMessage("A marca é obrigatória")
            .Length(2, 100).WithMessage("A marca deve ter entre 2 e 100 caracteres")
            .Matches("^[a-zA-Z\\s\\-]+$").WithMessage("Marca contém caracteres inválidos");

        RuleFor(x => x.Ano)
            .InclusiveBetween(1950, DateTime.Now.Year + 1)
            .WithMessage($"O ano deve estar entre 1950 e {DateTime.Now.Year + 1}");
    }
}

public class AdministradorValidator : AbstractValidator<AdministradorDTO>
{
    public AdministradorValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O email é obrigatório")
            .EmailAddress().WithMessage("Email deve ter um formato válido")
            .MaximumLength(255).WithMessage("Email não pode ter mais que 255 caracteres");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("A senha é obrigatória")
            .MinimumLength(6).WithMessage("A senha deve ter pelo menos 6 caracteres")
            .MaximumLength(50).WithMessage("A senha não pode ter mais que 50 caracteres")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)")
            .WithMessage("A senha deve conter pelo menos uma letra minúscula, uma maiúscula e um número");

        RuleFor(x => x.Perfil)
            .NotNull().WithMessage("O perfil é obrigatório")
            .IsInEnum().WithMessage("Perfil deve ser Adm ou Editor");
    }
}

public class LoginValidator : AbstractValidator<LoginDTO>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O email é obrigatório")
            .EmailAddress().WithMessage("Email deve ter um formato válido");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("A senha é obrigatória");
    }
}
