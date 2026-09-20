using System.ComponentModel.DataAnnotations;

namespace LocadoraVeiculos.API.DTOs;

public class CategoriaRequestDto
{
    /// <example>SUV</example>
    [Required(ErrorMessage = "O nome da categoria e obrigatorio.")]
    [StringLength(60, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(250)]
    public string? Descricao { get; set; }

    /// <example>250.00</example>
    [Range(0, 99999999.99, ErrorMessage = "O valor sugerido nao pode ser negativo.")]
    public decimal ValorDiariaSugerido { get; set; }
}

public class CategoriaResponseDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal ValorDiariaSugerido { get; set; }
    public int TotalVeiculos { get; set; }
}
