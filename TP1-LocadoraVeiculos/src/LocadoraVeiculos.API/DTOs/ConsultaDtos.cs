namespace LocadoraVeiculos.API.DTOs;

// Classes usadas so pelas consultas com JOIN.
// Cada uma e o formato da linha que a consulta devolve.

/// <summary>Linha do filtro 1: veiculos disponiveis com marca e categoria.</summary>
public class VeiculoDisponivelDto
{
    public int VeiculoId { get; set; }
    public string Modelo { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public int AnoFabricacao { get; set; }
    public int Quilometragem { get; set; }
    public decimal ValorDiaria { get; set; }
    public string Fabricante { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string? Filial { get; set; }
    public string? Cidade { get; set; }
}

/// <summary>Linha do filtro 2: historico de alugueis de um cliente.</summary>
public class HistoricoAluguelDto
{
    public int AluguelId { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Veiculo { get; set; } = string.Empty;
    public string Fabricante { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public DateTime DataRetirada { get; set; }
    public DateTime DataPrevistaDevolucao { get; set; }
    public DateTime? DataDevolucao { get; set; }
    public int? KmRodados { get; set; }
    public decimal ValorTotal { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>Linha do filtro 3: veiculos que nunca foram alugados (LEFT JOIN).</summary>
public class VeiculoSemAluguelDto
{
    public int VeiculoId { get; set; }
    public string Modelo { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public string Fabricante { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal ValorDiaria { get; set; }
    public bool Disponivel { get; set; }
}

/// <summary>Linha do filtro 4: quanto cada cliente ja gastou (LEFT JOIN + GROUP BY).</summary>
public class FaturamentoClienteDto
{
    public int ClienteId { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int QuantidadeAlugueis { get; set; }
    public decimal ValorTotalGasto { get; set; }
    public DateTime? UltimoAluguel { get; set; }
}

/// <summary>Linha do filtro 5: alugueis atrasados (nao devolvidos no prazo).</summary>
public class AluguelAtrasadoDto
{
    public int AluguelId { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string Veiculo { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public string? Filial { get; set; }
    public DateTime DataPrevistaDevolucao { get; set; }
    public int DiasDeAtraso { get; set; }
    public decimal ValorDiaria { get; set; }
}

/// <summary>Linha do filtro 6: faturamento por categoria de veiculo.</summary>
public class FaturamentoCategoriaDto
{
    public int CategoriaId { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public int QuantidadeVeiculos { get; set; }
    public int QuantidadeAlugueis { get; set; }
    public decimal ValorFaturado { get; set; }
    public decimal TicketMedio { get; set; }
}
