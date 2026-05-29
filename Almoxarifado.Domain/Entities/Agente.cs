using System.ComponentModel.DataAnnotations;

namespace Almoxarifado.Domain.Entities;

public class Agente
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome do agente é obrigatório.")]
    // A Regex abaixo permite apenas letras (maiúsculas/minúsculas), acentos e espaços. Nada de números ou símbolos.
    [RegularExpression(@"^[a-zA-ZÀ-ÿ\s]+$", ErrorMessage = "O nome não pode conter números ou caracteres especiais.")]
    [StringLength(100, ErrorMessage = "O nome não pode ter mais de 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "A matrícula é obrigatória.")]
    // A Regex abaixo exige que seja composto exclusivamente por números.
    [RegularExpression(@"^[0-9]+$", ErrorMessage = "A matrícula deve conter apenas caracteres numéricos.")]
    [StringLength(20, ErrorMessage = "A matrícula é muito longa.")]
    public string Matricula { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;
}