using Almoxarifado.Domain.Enums;

namespace Almoxarifado.Domain.Entities;

public class Equipamento
{
    public int Id { get; set; }
    public string NumeroSerie { get; set; } = string.Empty; // Patrimônio único
    public StatusEquipamento Status { get; set; } = StatusEquipamento.Disponivel;

    // Relacionamento com o Catálogo (Ex: Este equipamento é um Rádio T400)
    public int CatalogoId { get; set; }
    public Catalogo? Catalogo { get; set; }
}
