namespace LocadoraVeiculos.API.Modelos;

/// <summary>Em que pe esta o aluguel.</summary>
public enum StatusAluguel
{
    /// <summary>Reservado, mas o carro ainda nao saiu.</summary>
    Reservado = 1,
    /// <summary>Cliente pegou o carro, aluguel rolando.</summary>
    EmAndamento = 2,
    /// <summary>Carro devolvido, aluguel fechado.</summary>
    Finalizado = 3,
    /// <summary>Cancelado antes de pegar o carro.</summary>
    Cancelado = 4
}

/// <summary>Como o cliente escolheu pagar.</summary>
public enum FormaPagamento
{
    Dinheiro = 1,
    Pix = 2,
    CartaoDebito = 3,
    CartaoCredito = 4,
    Boleto = 5
}

/// <summary>Em que pe esta o pagamento.</summary>
public enum StatusPagamento
{
    Pendente = 1,
    Aprovado = 2,
    Recusado = 3,
    Estornado = 4
}

/// <summary>Combustivel que o carro usa.</summary>
public enum TipoCombustivel
{
    Gasolina = 1,
    Etanol = 2,
    Flex = 3,
    Diesel = 4,
    Eletrico = 5,
    Hibrido = 6
}
