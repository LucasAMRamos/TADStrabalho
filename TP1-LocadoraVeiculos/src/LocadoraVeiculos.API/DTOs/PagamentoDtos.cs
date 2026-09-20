using System.ComponentModel.DataAnnotations;
using LocadoraVeiculos.API.Modelos;

namespace LocadoraVeiculos.API.DTOs;

public class PagamentoRequestDto
{
    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "Informe um aluguel valido.")]
    public int AluguelId { get; set; }

    /// <example>600.00</example>
    [Range(0.01, 99999999.99, ErrorMessage = "O valor do pagamento deve ser maior que zero.")]
    public decimal Valor { get; set; }

    /// <example>Pix</example>
    [EnumDataType(typeof(FormaPagamento), ErrorMessage = "Forma de pagamento invalida.")]
    public FormaPagamento FormaPagamento { get; set; }

    /// <example>Aprovado</example>
    [EnumDataType(typeof(StatusPagamento), ErrorMessage = "Status de pagamento invalido.")]
    public StatusPagamento Status { get; set; } = StatusPagamento.Pendente;

    [StringLength(50)]
    public string? CodigoTransacao { get; set; }
}

public class PagamentoResponseDto
{
    public int Id { get; set; }
    public int AluguelId { get; set; }
    public decimal Valor { get; set; }
    public string FormaPagamento { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DataPagamento { get; set; }
    public string? CodigoTransacao { get; set; }
}
