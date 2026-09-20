using System.ComponentModel.DataAnnotations;
using LocadoraVeiculos.API.Modelos;

namespace LocadoraVeiculos.API.DTOs;

public class VeiculoRequestDto
{
    /// <example>Argo 1.0 Drive</example>
    [Required(ErrorMessage = "O modelo e obrigatorio.")]
    [StringLength(80, MinimumLength = 1)]
    public string Modelo { get; set; } = string.Empty;

    /// <summary>Placa no formato antigo (ABC-1234) ou no Mercosul (ABC1D23).</summary>
    /// <example>RKA1B23</example>
    [Required(ErrorMessage = "A placa e obrigatoria.")]
    [RegularExpression(@"^([A-Za-z]{3}-?\d{4}|[A-Za-z]{3}\d[A-Za-z]\d{2})$",
        ErrorMessage = "Placa invalida. Use ABC-1234 ou ABC1D23.")]
    public string Placa { get; set; } = string.Empty;

    /// <example>2023</example>
    [Range(1900, 2100, ErrorMessage = "Ano de fabricacao fora do intervalo permitido.")]
    public int AnoFabricacao { get; set; }

    /// <example>15000</example>
    [Range(0, int.MaxValue, ErrorMessage = "A quilometragem nao pode ser negativa.")]
    public int Quilometragem { get; set; }

    [StringLength(30)]
    public string? Cor { get; set; }

    /// <example>Flex</example>
    [EnumDataType(typeof(TipoCombustivel), ErrorMessage = "Tipo de combustivel invalido.")]
    public TipoCombustivel Combustivel { get; set; } = TipoCombustivel.Flex;

    /// <example>5</example>
    [Range(1, 60, ErrorMessage = "Numero de passageiros deve estar entre 1 e 60.")]
    public int NumeroPassageiros { get; set; } = 5;

    /// <example>150.00</example>
    [Range(0.01, 99999999.99, ErrorMessage = "O valor da diaria deve ser maior que zero.")]
    public decimal ValorDiaria { get; set; }

    public bool Disponivel { get; set; } = true;

    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "Informe um fabricante valido.")]
    public int FabricanteId { get; set; }

    /// <example>1</example>
    [Range(1, int.MaxValue, ErrorMessage = "Informe uma categoria valida.")]
    public int CategoriaId { get; set; }

    /// <example>1</example>
    public int? FilialId { get; set; }
}

public class VeiculoResponseDto
{
    public int Id { get; set; }
    public string Modelo { get; set; } = string.Empty;
    public string Placa { get; set; } = string.Empty;
    public int AnoFabricacao { get; set; }
    public int Quilometragem { get; set; }
    public string? Cor { get; set; }
    public string Combustivel { get; set; } = string.Empty;
    public int NumeroPassageiros { get; set; }
    public decimal ValorDiaria { get; set; }
    public bool Disponivel { get; set; }

    public int FabricanteId { get; set; }
    public string? FabricanteNome { get; set; }

    public int CategoriaId { get; set; }
    public string? CategoriaNome { get; set; }

    public int? FilialId { get; set; }
    public string? FilialNome { get; set; }
}
