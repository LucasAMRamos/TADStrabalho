using System.ComponentModel.DataAnnotations;
using LocadoraVeiculos.API.Modelos;

namespace LocadoraVeiculos.API.DTOs;

/// <summary>Dados para abrir um novo aluguel (POST /api/alugueis).</summary>
public class AluguelRequestDto : IValidatableObject
{
    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "Informe um cliente valido.")]
    public int ClienteId { get; set; }

    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "Informe um veiculo valido.")]
    public int VeiculoId { get; set; }

    /// <example>2026-10-01T09:00:00</example>
    [Required(ErrorMessage = "A data de retirada e obrigatoria.")]
    public DateTime DataRetirada { get; set; }

    /// <example>2026-10-05T09:00:00</example>
    [Required(ErrorMessage = "A data prevista de devolucao e obrigatoria.")]
    public DateTime DataPrevistaDevolucao { get; set; }

    /// <summary>
    /// Valor da diaria. Se vier vazio, usa o que esta cadastrado no carro.
    /// </summary>
    /// <example>150.00</example>
    [Range(0.01, 99999999.99, ErrorMessage = "O valor da diaria deve ser maior que zero.")]
    public decimal? ValorDiaria { get; set; }

    [StringLength(500)]
    public string? Observacoes { get; set; }

    /// <summary>
    /// Validacao que olha mais de um campo junto. O [Range] e o [Required]
    /// so sabem olhar um campo por vez.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DataPrevistaDevolucao <= DataRetirada)
        {
            yield return new ValidationResult(
                "A data prevista de devolucao deve ser posterior a data de retirada.",
                new[] { nameof(DataPrevistaDevolucao) });
        }

        if ((DataPrevistaDevolucao - DataRetirada).TotalDays > 365)
        {
            yield return new ValidationResult(
                "O periodo de locacao nao pode passar de 365 dias.",
                new[] { nameof(DataPrevistaDevolucao) });
        }
    }
}

/// <summary>Dados para registrar a devolucao (PUT /api/alugueis/{id}/devolucao).</summary>
public class DevolucaoRequestDto
{
    /// <example>2026-10-06T18:30:00</example>
    [Required(ErrorMessage = "A data de devolucao e obrigatoria.")]
    public DateTime DataDevolucao { get; set; }

    /// <summary>Km do carro na volta. Tem que ser maior ou igual ao km da saida.</summary>
    /// <example>15900</example>
    [Range(0, int.MaxValue, ErrorMessage = "A quilometragem final nao pode ser negativa.")]
    public int QuilometragemFinal { get; set; }

    [StringLength(500)]
    public string? Observacoes { get; set; }
}

/// <summary>Dados devolvidos pela API para um aluguel.</summary>
public class AluguelResponseDto
{
    public int Id { get; set; }

    public int ClienteId { get; set; }
    public string? ClienteNome { get; set; }
    public string? ClienteCpf { get; set; }

    public int VeiculoId { get; set; }
    public string? VeiculoModelo { get; set; }
    public string? VeiculoPlaca { get; set; }
    public string? FabricanteNome { get; set; }

    public DateTime DataRetirada { get; set; }
    public DateTime DataPrevistaDevolucao { get; set; }
    public DateTime? DataDevolucao { get; set; }

    public int QuilometragemInicial { get; set; }
    public int? QuilometragemFinal { get; set; }
    public int? KmRodados { get; set; }

    public decimal ValorDiaria { get; set; }
    public int QuantidadeDiarias { get; set; }
    public decimal ValorMulta { get; set; }
    public decimal ValorTotal { get; set; }

    public string Status { get; set; } = string.Empty;
    public string? Observacoes { get; set; }
}
