using System.ComponentModel.DataAnnotations;

namespace LocadoraVeiculos.API.DTOs;

/// <summary>O que o POST e o PUT de fabricante aceitam.</summary>
public class FabricanteRequestDto
{
    /// <example>Fiat</example>
    [Required(ErrorMessage = "O nome do fabricante e obrigatorio.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 80 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    /// <example>Italia</example>
    [Required(ErrorMessage = "O pais de origem e obrigatorio.")]
    [StringLength(60)]
    public string PaisOrigem { get; set; } = string.Empty;

    /// <example>1899</example>
    [Range(1800, 2100, ErrorMessage = "O ano de fundacao deve estar entre 1800 e 2100.")]
    public int? AnoFundacao { get; set; }
}

/// <summary>O que a API devolve de um fabricante.</summary>
public class FabricanteResponseDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string PaisOrigem { get; set; } = string.Empty;
    public int? AnoFundacao { get; set; }
    public int TotalVeiculos { get; set; }
}
