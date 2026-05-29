using System.ComponentModel.DataAnnotations;

namespace Almoxarifado.Domain.Entities;

public class Catalogo
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome do item é obrigatório.")]
    // Permite letras, números, espaços, hífens e pontos (ex: "Cabo RJ-45", "Tablet 10.1")
    [RegularExpression(@"^[a-zA-ZÀ-ÿ0-9\s\-\.]+$", ErrorMessage = "O nome contém caracteres inválidos.")]
    [StringLength(100, ErrorMessage = "O nome não pode exceder os 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    public bool IsConsumivel { get; set; }

    [StringLength(200, ErrorMessage = "O detalhe/observação é muito longo.")]
    public string Descricao { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "O stock não pode ser negativo.")]
    public int EstoqueAtual { get; set; }

    [Range(0, 10000, ErrorMessage = "O stock mínimo deve estar entre 0 e 10.000.")]
    public int EstoqueMinimo { get; set; }

    public ICollection<Equipamento> Equipamentos { get; set; } = new List<Equipamento>();
}