namespace LocadoraVeiculos.API.Models;

/// <summary>
/// Grupo do carro: economico, SUV, premium e por ai vai.
/// Serve de base para o valor da diaria.
/// </summary>
public class Categoria
{
    /// <summary>Chave primaria.</summary>
    public int Id { get; set; }

    /// <summary>Nome da categoria, nao pode repetir.</summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>Um texto curto explicando a categoria.</summary>
    public string? Descricao { get; set; }

    /// <summary>Diaria sugerida para os carros dessa categoria.</summary>
    public decimal ValorDiariaSugerido { get; set; }

    public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
}
