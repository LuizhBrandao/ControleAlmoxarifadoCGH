using Almoxarifado.Domain.Enums;

namespace Almoxarifado.Domain.Entities;

public class Movimentacao
{
    public int Id { get; set; }
    public DateTime DataHora { get; set; } = DateTime.Now;
    public TipoMovimentacao Tipo { get; set; }
    public int Quantidade { get; set; }
    public string NomeAlmoxarife { get; set; } = string.Empty; // Quem operou o sistema

    // Quem está retirando/devolvendo
    public int AgenteId { get; set; }
    public Agente? Agente { get; set; }

    // Qual item do catálogo está sendo movimentado (Obrigatório)
    public int CatalogoId { get; set; }
    public Catalogo? Catalogo { get; set; }

    // Qual equipamento físico (Opcional, preenchido apenas se NÃO for consumível)
    public int? EquipamentoId { get; set; }
    public Equipamento? Equipamento { get; set; }
}
