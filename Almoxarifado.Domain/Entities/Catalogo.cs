namespace Almoxarifado.Domain.Entities;

public class Catalogo
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool IsConsumivel { get; set; }
    public int EstoqueAtual { get; set; } // Usado principalmente para itens de consumo (ex: bobinas)
    public int EstoqueMinimo { get; set; }

    public List<Equipamento> Equipamentos { get; set; } = new();
}