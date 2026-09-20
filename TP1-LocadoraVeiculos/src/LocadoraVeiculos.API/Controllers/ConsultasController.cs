using LocadoraVeiculos.API.DTOs;
using LocadoraVeiculos.API.Data;
using LocadoraVeiculos.API.Models;
using LocadoraVeiculos.API.Validacoes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.API.Controllers;

/// <summary>
/// Consultas que cruzam varias tabelas.
/// Usa INNER JOIN ("join ... on ... equals") e LEFT JOIN
/// ("join ... into grupo" + "grupo.DefaultIfEmpty()"), e duas delas agrupam com GROUP BY.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ConsultasController : ControllerBase
{
    private readonly ApplicationContext _contexto;

    public ConsultasController(ApplicationContext contexto) => _contexto = contexto;

    // FILTRO 1 - INNER JOIN com Fabricante/Categoria + LEFT JOIN com Filial

    /// <summary>
    /// Filtro 1: veiculos disponiveis para locacao, com marca, categoria e filial.
    /// </summary>
    /// <remarks>
    /// INNER JOIN com Fabricante e Categoria, que todo carro tem.
    /// LEFT JOIN com Filial, porque a filial e opcional.
    /// </remarks>
    /// <param name="fabricanteId">Filtra por marca.</param>
    /// <param name="categoriaId">Filtra por categoria.</param>
    /// <param name="cidade">Filtra pela cidade da filial.</param>
    /// <param name="valorDiariaMaximo">Traz apenas veiculos ate esse valor de diaria.</param>
    /// <response code="200">Consulta executada com sucesso.</response>
    [HttpGet("veiculos-disponiveis")]
    [ProducesResponseType(typeof(IEnumerable<VeiculoDisponivelDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VeiculoDisponivelDto>>> VeiculosDisponiveis(
        [FromQuery] int? fabricanteId,
        [FromQuery] int? categoriaId,
        [FromQuery] string? cidade,
        [FromQuery] decimal? valorDiariaMaximo)
    {
        // String vazia vira null, fica mais facil de testar na consulta.
        cidade = string.IsNullOrWhiteSpace(cidade) ? null : cidade.Trim();

        var consulta =
            from veiculo in _contexto.Veiculos.AsNoTracking()
            join fabricante in _contexto.Fabricantes on veiculo.FabricanteId equals fabricante.Id  // INNER JOIN
            join categoria in _contexto.Categorias on veiculo.CategoriaId equals categoria.Id      // INNER JOIN
            join filial in _contexto.Filiais on veiculo.FilialId equals (int?)filial.Id into filiais
            from filial in filiais.DefaultIfEmpty()                                                // LEFT JOIN
            where veiculo.Disponivel
               // O filtro so entra se o parametro veio preenchido.
               && (fabricanteId == null || veiculo.FabricanteId == fabricanteId)
               && (categoriaId == null || veiculo.CategoriaId == categoriaId)
               && (cidade == null || (filial != null && filial.Cidade.Contains(cidade)))
               && (valorDiariaMaximo == null || veiculo.ValorDiaria <= valorDiariaMaximo)
            select new VeiculoDisponivelDto
            {
                VeiculoId = veiculo.Id,
                Modelo = veiculo.Modelo,
                Placa = veiculo.Placa,
                AnoFabricacao = veiculo.AnoFabricacao,
                Quilometragem = veiculo.Quilometragem,
                ValorDiaria = veiculo.ValorDiaria,
                Fabricante = fabricante.Nome,
                Categoria = categoria.Nome,
                Filial = filial != null ? filial.Nome : null,
                Cidade = filial != null ? filial.Cidade : null
            };

        return Ok(await consulta.OrderBy(x => x.ValorDiaria).ToListAsync());
    }

    // FILTRO 2 - INNER JOIN entre Aluguel, Cliente, Veiculo e Fabricante

    /// <summary>
    /// Filtro 2: historico de alugueis de um cliente (por Id ou por CPF).
    /// </summary>
    /// <remarks>
    /// INNER JOIN ligando as quatro tabelas. Passe <c>clienteId</c> ou <c>cpf</c>.
    /// </remarks>
    /// <param name="clienteId">Id do cliente.</param>
    /// <param name="cpf">CPF do cliente, com ou sem pontuacao.</param>
    /// <param name="status">Filtra por situacao do aluguel.</param>
    /// <response code="200">Consulta executada com sucesso.</response>
    /// <response code="400">Nenhum identificador de cliente foi informado.</response>
    /// <response code="404">Cliente nao encontrado.</response>
    [HttpGet("historico-cliente")]
    [ProducesResponseType(typeof(IEnumerable<HistoricoAluguelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<HistoricoAluguelDto>>> HistoricoDoCliente(
        [FromQuery] int? clienteId,
        [FromQuery] string? cpf,
        [FromQuery] StatusAluguel? status)
    {
        if (!clienteId.HasValue && string.IsNullOrWhiteSpace(cpf))
        {
            return BadRequest(new ProblemDetails
            {
                Status = 400,
                Title = "Informe o parametro 'clienteId' ou o parametro 'cpf'."
            });
        }

        var cpfLimpo = string.IsNullOrWhiteSpace(cpf) ? null : CpfAttribute.SomenteDigitos(cpf);

        var existeCliente = await _contexto.Clientes.AnyAsync(c =>
            (clienteId != null && c.Id == clienteId) || (cpfLimpo != null && c.Cpf == cpfLimpo));

        if (!existeCliente)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = "Cliente nao encontrado." });
        }

        var consulta =
            from aluguel in _contexto.Alugueis.AsNoTracking()
            join cliente in _contexto.Clientes on aluguel.ClienteId equals cliente.Id        // INNER JOIN
            join veiculo in _contexto.Veiculos on aluguel.VeiculoId equals veiculo.Id        // INNER JOIN
            join fabricante in _contexto.Fabricantes on veiculo.FabricanteId equals fabricante.Id // INNER JOIN
            where (clienteId == null || cliente.Id == clienteId)
               && (cpfLimpo == null || cliente.Cpf == cpfLimpo)
               && (status == null || aluguel.Status == status)
            orderby aluguel.DataRetirada descending
            select new HistoricoAluguelDto
            {
                AluguelId = aluguel.Id,
                Cliente = cliente.Nome,
                Cpf = cliente.Cpf,
                Veiculo = veiculo.Modelo,
                Fabricante = fabricante.Nome,
                Placa = veiculo.Placa,
                DataRetirada = aluguel.DataRetirada,
                DataPrevistaDevolucao = aluguel.DataPrevistaDevolucao,
                DataDevolucao = aluguel.DataDevolucao,
                KmRodados = aluguel.QuilometragemFinal != null
                    ? aluguel.QuilometragemFinal - aluguel.QuilometragemInicial
                    : null,
                ValorTotal = aluguel.ValorTotal,
                Status = aluguel.Status.ToString()
            };

        return Ok(await consulta.ToListAsync());
    }

    // FILTRO 3 - LEFT JOIN entre Veiculo e Aluguel, pegando os que nao tem par

    /// <summary>
    /// Filtro 3: veiculos que nunca foram alugados (frota parada).
    /// </summary>
    /// <remarks>
    /// LEFT JOIN com Aluguel guardando so as linhas que ficaram nulas.
    /// No SQL seria <c>LEFT JOIN Aluguel ... WHERE Aluguel.Id IS NULL</c>.
    /// </remarks>
    /// <response code="200">Consulta executada com sucesso.</response>
    [HttpGet("veiculos-nunca-alugados")]
    [ProducesResponseType(typeof(IEnumerable<VeiculoSemAluguelDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VeiculoSemAluguelDto>>> VeiculosNuncaAlugados()
    {
        var consulta =
            from veiculo in _contexto.Veiculos.AsNoTracking()
            join fabricante in _contexto.Fabricantes on veiculo.FabricanteId equals fabricante.Id // INNER JOIN
            join categoria in _contexto.Categorias on veiculo.CategoriaId equals categoria.Id     // INNER JOIN
            join aluguel in _contexto.Alugueis on veiculo.Id equals aluguel.VeiculoId into alugueisDoVeiculo
            from aluguel in alugueisDoVeiculo.DefaultIfEmpty()                                    // LEFT JOIN
            where aluguel == null                                                                 // nunca foi alugado
            orderby veiculo.Modelo
            select new VeiculoSemAluguelDto
            {
                VeiculoId = veiculo.Id,
                Modelo = veiculo.Modelo,
                Placa = veiculo.Placa,
                Fabricante = fabricante.Nome,
                Categoria = categoria.Nome,
                ValorDiaria = veiculo.ValorDiaria,
                Disponivel = veiculo.Disponivel
            };

        return Ok(await consulta.ToListAsync());
    }

    // FILTRO 4 - LEFT JOIN entre Cliente e Aluguel, com GROUP BY

    /// <summary>
    /// Filtro 4: quanto cada cliente ja gastou na locadora.
    /// </summary>
    /// <remarks>
    /// LEFT JOIN mais GROUP BY por cliente. Quem nunca alugou aparece com total zero.
    /// Com INNER JOIN essas pessoas sumiriam da lista.
    /// </remarks>
    /// <param name="valorMinimo">Mostra apenas clientes que gastaram pelo menos esse valor.</param>
    /// <response code="200">Consulta executada com sucesso.</response>
    [HttpGet("faturamento-por-cliente")]
    [ProducesResponseType(typeof(IEnumerable<FaturamentoClienteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FaturamentoClienteDto>>> FaturamentoPorCliente(
        [FromQuery] decimal? valorMinimo)
    {
        var consulta =
            from cliente in _contexto.Clientes.AsNoTracking()
            join aluguel in _contexto.Alugueis on cliente.Id equals aluguel.ClienteId into alugueisDoCliente
            from aluguel in alugueisDoCliente.DefaultIfEmpty()                                     // LEFT JOIN
            group aluguel by new { cliente.Id, cliente.Nome, cliente.Cpf, cliente.Email }
                into grupo                                                                          // GROUP BY
            select new FaturamentoClienteDto
            {
                ClienteId = grupo.Key.Id,
                Cliente = grupo.Key.Nome,
                Cpf = grupo.Key.Cpf,
                Email = grupo.Key.Email,
                // Precisa do "!= null" porque o LEFT JOIN deixa a linha
                // do aluguel vazia para quem nunca alugou.
                QuantidadeAlugueis = grupo.Sum(a => a != null ? 1 : 0),
                ValorTotalGasto = grupo.Sum(a => a != null ? a.ValorTotal : 0m),
                UltimoAluguel = grupo.Max(a => a != null ? (DateTime?)a.DataRetirada : null)
            };

        if (valorMinimo.HasValue)
        {
            consulta = consulta.Where(x => x.ValorTotalGasto >= valorMinimo.Value);
        }

        return Ok(await consulta.OrderByDescending(x => x.ValorTotalGasto).ToListAsync());
    }

    // FILTRO 5 - INNER JOIN com Cliente/Veiculo + LEFT JOIN com Filial

    /// <summary>
    /// Filtro 5: alugueis em andamento que passaram da data prevista de devolucao.
    /// </summary>
    /// <remarks>
    /// INNER JOIN com Cliente e Veiculo, LEFT JOIN com Filial.
    /// O atraso e contado no banco com DATEDIFF, pelo <c>EF.Functions.DateDiffDay</c>.
    /// </remarks>
    /// <param name="diasMinimosDeAtraso">Mostra apenas atrasos iguais ou maiores que este numero de dias.</param>
    /// <response code="200">Consulta executada com sucesso.</response>
    [HttpGet("alugueis-atrasados")]
    [ProducesResponseType(typeof(IEnumerable<AluguelAtrasadoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AluguelAtrasadoDto>>> AlugueisAtrasados(
        [FromQuery] int diasMinimosDeAtraso = 1)
    {
        var agora = DateTime.Now;

        var consulta =
            from aluguel in _contexto.Alugueis.AsNoTracking()
            join cliente in _contexto.Clientes on aluguel.ClienteId equals cliente.Id   // INNER JOIN
            join veiculo in _contexto.Veiculos on aluguel.VeiculoId equals veiculo.Id   // INNER JOIN
            join filial in _contexto.Filiais on veiculo.FilialId equals (int?)filial.Id into filiais
            from filial in filiais.DefaultIfEmpty()                                     // LEFT JOIN
            where aluguel.Status == StatusAluguel.EmAndamento
               && aluguel.DataPrevistaDevolucao < agora
               && EF.Functions.DateDiffDay(aluguel.DataPrevistaDevolucao, agora) >= diasMinimosDeAtraso
            orderby aluguel.DataPrevistaDevolucao
            select new AluguelAtrasadoDto
            {
                AluguelId = aluguel.Id,
                Cliente = cliente.Nome,
                Telefone = cliente.Telefone,
                Veiculo = veiculo.Modelo,
                Placa = veiculo.Placa,
                Filial = filial != null ? filial.Nome : null,
                DataPrevistaDevolucao = aluguel.DataPrevistaDevolucao,
                DiasDeAtraso = EF.Functions.DateDiffDay(aluguel.DataPrevistaDevolucao, agora),
                ValorDiaria = aluguel.ValorDiaria
            };

        return Ok(await consulta.ToListAsync());
    }

    // FILTRO 6 - INNER JOIN entre Categoria, Veiculo e Aluguel, com GROUP BY

    /// <summary>
    /// Filtro 6: faturamento dos alugueis ja finalizados, agrupado por categoria.
    /// </summary>
    /// <remarks>
    /// INNER JOIN ligando Categoria, Veiculo e Aluguel, com GROUP BY para somar.
    /// A contagem de carros vem de uma segunda consulta.
    /// </remarks>
    /// <response code="200">Consulta executada com sucesso.</response>
    [HttpGet("faturamento-por-categoria")]
    [ProducesResponseType(typeof(IEnumerable<FaturamentoCategoriaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FaturamentoCategoriaDto>>> FaturamentoPorCategoria()
    {
        // Quantos carros tem em cada categoria.
        var veiculosPorCategoria = await _contexto.Veiculos
            .AsNoTracking()
            .GroupBy(v => v.CategoriaId)
            .Select(g => new { CategoriaId = g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.CategoriaId, x => x.Total);

        // Quanto cada categoria faturou, so com aluguel ja finalizado.
        var faturamento = await (
            from categoria in _contexto.Categorias.AsNoTracking()
            join veiculo in _contexto.Veiculos on categoria.Id equals veiculo.CategoriaId // INNER JOIN
            join aluguel in _contexto.Alugueis on veiculo.Id equals aluguel.VeiculoId     // INNER JOIN
            where aluguel.Status == StatusAluguel.Finalizado
            group aluguel by new { categoria.Id, categoria.Nome } into grupo              // GROUP BY
            select new
            {
                CategoriaId = grupo.Key.Id,
                Categoria = grupo.Key.Nome,
                QuantidadeAlugueis = grupo.Count(),
                ValorFaturado = grupo.Sum(a => a.ValorTotal)
            }).ToListAsync();

        var resultado = faturamento
            .Select(f => new FaturamentoCategoriaDto
            {
                CategoriaId = f.CategoriaId,
                Categoria = f.Categoria,
                QuantidadeVeiculos = veiculosPorCategoria.TryGetValue(f.CategoriaId, out var total) ? total : 0,
                QuantidadeAlugueis = f.QuantidadeAlugueis,
                ValorFaturado = f.ValorFaturado,
                TicketMedio = f.QuantidadeAlugueis == 0
                    ? 0m
                    : Math.Round(f.ValorFaturado / f.QuantidadeAlugueis, 2)
            })
            .OrderByDescending(f => f.ValorFaturado)
            .ToList();

        return Ok(resultado);
    }
}
