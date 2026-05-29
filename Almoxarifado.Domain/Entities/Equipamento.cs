using System.ComponentModel.DataAnnotations;
using Almoxarifado.Domain.Enums;

namespace Almoxarifado.Domain.Entities;

public class Equipamento
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O número de série / património é obrigatório.")]
    // Permite apenas letras, números e hífens, SEM ESPAÇOS (ex: "CGH-001", "12345ABC")
    [RegularExpression(@"^[a-zA-Z0-9\-]+$", ErrorMessage = "O património deve conter apenas letras, números ou hífens (sem espaços).")]
    [StringLength(50, ErrorMessage = "O património não pode exceder os 50 caracteres.")]
    public string NumeroSerie { get; set; } = string.Empty;

    public StatusEquipamento Status { get; set; } = StatusEquipamento.Disponivel;

    public int CatalogoId { get; set; }
    public Catalogo Catalogo { get; set; } = null!;
}