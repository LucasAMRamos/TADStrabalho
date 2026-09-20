namespace LocadoraVeiculos.API.Modelos;

/// <summary>
/// Loja da locadora onde o carro fica guardado e o cliente retira.
/// </summary>
public class Filial
{
    /// <summary>Chave primaria.</summary>
    public int Id { get; set; }

    /// <summary>Nome da loja, tipo "Filial Centro - BH".</summary>
    public string Nome { get; set; } = string.Empty;

    public string Cidade { get; set; } = string.Empty;

    /// <summary>Sigla do estado, com 2 letras.</summary>
    public string Estado { get; set; } = string.Empty;

    public string? Endereco { get; set; }

    public string? Telefone { get; set; }

    public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
}
