namespace MinimalApi.Dominio.ModelViews;

public record EstatisticasAPI
{
    public int TotalVeiculos { get; set; }
    public int TotalAdministradores { get; set; }
    public string VersaoAPI { get; set; } = default!;
    public DateTime DataConsulta { get; set; } = DateTime.Now;
    public List<string> FuncionalidadesDisponiveis { get; set; } = new List<string>();
}
