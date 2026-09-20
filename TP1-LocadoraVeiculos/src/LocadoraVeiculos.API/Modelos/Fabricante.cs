namespace LocadoraVeiculos.API.Modelos;

/// <summary>
/// Marca do carro, tipo Fiat ou Toyota. Uma marca tem varios veiculos.
/// </summary>
public class Fabricante
{
    /// <summary>Chave primaria.</summary>
    public int Id { get; set; }

    /// <summary>Nome da marca, nao pode repetir.</summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>Pais de origem da marca.</summary>
    public string PaisOrigem { get; set; } = string.Empty;

    /// <summary>Ano em que a montadora foi criada.</summary>
    public int? AnoFundacao { get; set; }

    // Lista dos carros dessa marca.
    public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
}
