namespace LocadoraVeiculos.API.Models;

/// <summary>
/// Pagamento de um aluguel. Um aluguel pode ter mais de um.
/// </summary>
public class Pagamento
{
    /// <summary>Chave primaria.</summary>
    public int Id { get; set; }

    /// <summary>FK do aluguel, obrigatoria.</summary>
    public int AluguelId { get; set; }
    public Aluguel? Aluguel { get; set; }

    public decimal Valor { get; set; }

    public FormaPagamento FormaPagamento { get; set; }

    public StatusPagamento Status { get; set; } = StatusPagamento.Pendente;

    public DateTime DataPagamento { get; set; } = DateTime.Now;

    /// <summary>Codigo que a operadora devolve.</summary>
    public string? CodigoTransacao { get; set; }
}
