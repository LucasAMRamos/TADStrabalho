using System.ComponentModel.DataAnnotations;

namespace LocadoraVeiculos.API.Validacoes;

/// <summary>
/// Confere o CPF pela conta dos digitos verificadores.
/// Usa assim: [Cpf] public string Cpf { get; set; }
/// </summary>
public class CpfAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is null)
        {
            return true; // quem exige o preenchimento e o [Required]
        }

        var cpf = SomenteDigitos(value.ToString() ?? string.Empty);

        if (cpf.Length != 11)
        {
            return false;
        }

        // Corta os repetidos, tipo 00000000000 ou 11111111111.
        if (cpf.Distinct().Count() == 1)
        {
            return false;
        }

        return CalcularDigito(cpf, 9) == cpf[9] - '0'
            && CalcularDigito(cpf, 10) == cpf[10] - '0';
    }

    public override string FormatErrorMessage(string name) => $"O campo {name} nao contem um CPF valido.";

    /// <summary>Tira ponto, traco e espaco, sobra so numero.</summary>
    public static string SomenteDigitos(string texto)
        => new(texto.Where(char.IsDigit).ToArray());

    private static int CalcularDigito(string cpf, int quantidadeDigitos)
    {
        var soma = 0;
        var peso = quantidadeDigitos + 1;

        for (var i = 0; i < quantidadeDigitos; i++)
        {
            soma += (cpf[i] - '0') * peso;
            peso--;
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
