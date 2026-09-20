namespace LocadoraVeiculos.API.Models;

/// <summary>
/// Aluguel de um carro por um cliente, dentro de um periodo.
/// Guarda a devolucao, a quilometragem de saida e de volta,
/// o valor da diaria e o total.
/// </summary>
public class Aluguel
{
    /// <summary>Chave primaria.</summary>
    public int Id { get; set; }

    // Chaves estrangeiras

    /// <summary>FK do cliente, obrigatoria.</summary>
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    /// <summary>FK do carro, obrigatoria.</summary>
    public int VeiculoId { get; set; }
    public Veiculo? Veiculo { get; set; }

    // Periodo

    /// <summary>Dia em que o cliente pega o carro.</summary>
    public DateTime DataRetirada { get; set; }

    /// <summary>Dia combinado para devolver.</summary>
    public DateTime DataPrevistaDevolucao { get; set; }

    /// <summary>Dia em que devolveu de verdade. Fica nula ate o carro voltar.</summary>
    public DateTime? DataDevolucao { get; set; }

    // Quilometragem

    /// <summary>Km do carro na hora de sair.</summary>
    public int QuilometragemInicial { get; set; }

    /// <summary>Km do carro na volta. Fica nulo ate devolver.</summary>
    public int? QuilometragemFinal { get; set; }

    // Valores

    /// <summary>Diaria combinada nesse aluguel.</summary>
    public decimal ValorDiaria { get; set; }

    /// <summary>Quantas diarias foram cobradas.</summary>
    public int QuantidadeDiarias { get; set; }

    /// <summary>Total do aluguel: diaria x dias, mais a multa.</summary>
    public decimal ValorTotal { get; set; }

    /// <summary>Multa cobrada quando devolve atrasado.</summary>
    public decimal ValorMulta { get; set; }

    public StatusAluguel Status { get; set; } = StatusAluguel.Reservado;

    public string? Observacoes { get; set; }

    /// <summary>
    /// Quantos km o cliente rodou. Nao vai para o banco, e so uma conta.
    /// </summary>
    public int? KmRodados => QuilometragemFinal.HasValue
        ? QuilometragemFinal.Value - QuilometragemInicial
        : null;

    public ICollection<Pagamento> Pagamentos { get; set; } = new List<Pagamento>();
}
