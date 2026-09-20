using System.ComponentModel.DataAnnotations;
using LocadoraVeiculos.API.Validacoes;

namespace LocadoraVeiculos.API.DTOs;

public class ClienteRequestDto
{
    /// <example>Maria Oliveira</example>
    [Required(ErrorMessage = "O nome e obrigatorio.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 120 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    /// <summary>CPF com ou sem ponto e traco. So os numeros vao para o banco.</summary>
    /// <example>529.982.247-25</example>
    [Required(ErrorMessage = "O CPF e obrigatorio.")]
    [Cpf]
    public string Cpf { get; set; } = string.Empty;

    /// <example>maria.oliveira@email.com</example>
    [Required(ErrorMessage = "O e-mail e obrigatorio.")]
    [EmailAddress(ErrorMessage = "E-mail em formato invalido.")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    /// <example>(31) 99999-0001</example>
    [Phone(ErrorMessage = "Telefone em formato invalido.")]
    [StringLength(20)]
    public string? Telefone { get; set; }

    /// <example>1995-04-12</example>
    public DateTime? DataNascimento { get; set; }

    /// <example>01234567890</example>
    [RegularExpression(@"^\d{11}$", ErrorMessage = "A CNH deve conter 11 digitos.")]
    public string? NumeroCnh { get; set; }
}

public class ClienteResponseDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? NumeroCnh { get; set; }
    public DateTime DataCadastro { get; set; }
    public int TotalAlugueis { get; set; }
}
