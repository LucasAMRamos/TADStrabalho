namespace LocadoraVeiculos.API.Modelos;

/// <summary>
/// Carro da frota. Todo veiculo tem uma marca e guarda
/// modelo, ano e quilometragem.
/// </summary>
public class Veiculo
{
    /// <summary>Chave primaria.</summary>
    public int Id { get; set; }

    /// <summary>Modelo do carro, tipo "Argo 1.0".</summary>
    public string Modelo { get; set; } = string.Empty;

    /// <summary>Placa do carro, nao pode repetir.</summary>
    public string Placa { get; set; } = string.Empty;

    /// <summary>Ano de fabricacao.</summary>
    public int AnoFabricacao { get; set; }

    /// <summary>Quanto o carro ja rodou, o numero do odometro.</summary>
    public int Quilometragem { get; set; }

    public string? Cor { get; set; }

    public TipoCombustivel Combustivel { get; set; } = TipoCombustivel.Flex;

    /// <summary>Quantas pessoas cabem no carro.</summary>
    public int NumeroPassageiros { get; set; } = 5;

    /// <summary>Quanto custa cada dia de aluguel.</summary>
    public decimal ValorDiaria { get; set; }

    /// <summary>Diz se o carro esta livre para alugar.</summary>
    public bool Disponivel { get; set; } = true;

    // Chaves estrangeiras

    /// <summary>FK da marca, obrigatoria.</summary>
    public int FabricanteId { get; set; }
    public Fabricante? Fabricante { get; set; }

    /// <summary>FK da categoria, obrigatoria.</summary>
    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    /// <summary>FK da filial. E opcional, o carro pode estar sem loja.</summary>
    public int? FilialId { get; set; }
    public Filial? Filial { get; set; }

    public ICollection<Aluguel> Alugueis { get; set; } = new List<Aluguel>();
}
