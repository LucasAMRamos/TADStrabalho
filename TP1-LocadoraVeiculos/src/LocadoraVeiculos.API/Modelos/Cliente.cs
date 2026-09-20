namespace LocadoraVeiculos.API.Modelos;

/// <summary>
/// Cliente da locadora. Precisa ter pelo menos nome, CPF e e-mail.
/// </summary>
public class Cliente
{
    /// <summary>Chave primaria.</summary>
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    /// <summary>CPF so com numeros, 11 digitos. Nao pode repetir.</summary>
    public string Cpf { get; set; } = string.Empty;

    /// <summary>E-mail do cliente, nao pode repetir.</summary>
    public string Email { get; set; } = string.Empty;

    public string? Telefone { get; set; }

    public DateTime? DataNascimento { get; set; }

    /// <summary>Numero da carteira de motorista.</summary>
    public string? NumeroCnh { get; set; }

    /// <summary>Dia em que o cliente foi cadastrado.</summary>
    public DateTime DataCadastro { get; set; } = DateTime.Now;

    public ICollection<Aluguel> Alugueis { get; set; } = new List<Aluguel>();
}
