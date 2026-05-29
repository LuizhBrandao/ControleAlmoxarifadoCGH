namespace Almoxarifado.Domain.Entities;

public class Agente
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Matricula { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
}
