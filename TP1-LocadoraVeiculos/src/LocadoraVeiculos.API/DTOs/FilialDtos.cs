using System.ComponentModel.DataAnnotations;

namespace LocadoraVeiculos.API.DTOs;

public class FilialRequestDto
{
    /// <example>Filial Centro - BH</example>
    [Required(ErrorMessage = "O nome da filial e obrigatorio.")]
    [StringLength(100, MinimumLength = 3)]
    public string Nome { get; set; } = string.Empty;

    /// <example>Belo Horizonte</example>
    [Required(ErrorMessage = "A cidade e obrigatoria.")]
    [StringLength(80)]
    public string Cidade { get; set; } = string.Empty;

    /// <example>MG</example>
    [Required(ErrorMessage = "O estado e obrigatorio.")]
    [RegularExpression("^[A-Za-z]{2}$", ErrorMessage = "O estado deve ser a sigla da UF com 2 letras.")]
    public string Estado { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Endereco { get; set; }

    [Phone(ErrorMessage = "Telefone em formato invalido.")]
    [StringLength(20)]
    public string? Telefone { get; set; }
}

public class FilialResponseDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string? Endereco { get; set; }
    public string? Telefone { get; set; }
    public int TotalVeiculos { get; set; }
}
