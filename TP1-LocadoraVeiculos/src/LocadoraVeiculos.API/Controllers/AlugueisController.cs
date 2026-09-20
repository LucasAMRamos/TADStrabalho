using System.Linq.Expressions;
using LocadoraVeiculos.API.DTOs;
using LocadoraVeiculos.API.Data;
using LocadoraVeiculos.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.API.Controllers;

/// <summary>
/// Cuida dos alugueis: reserva, retirada, devolucao e cancelamento.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AlugueisController : ControllerBase
{
    /// <summary>Multa de 20% em cima da diaria quando devolve atrasado.</summary>
    private const decimal PercentualMultaAtraso = 0.20m;

    private readonly ApplicationContext _contexto;

    public AlugueisController(ApplicationContext contexto) => _contexto = contexto;

    private static readonly Expression<Func<Aluguel, AluguelResponseDto>> ParaDto = a => new AluguelResponseDto
    {
        Id = a.Id,
        ClienteId = a.ClienteId,
        ClienteNome = a.Cliente!.Nome,
        ClienteCpf = a.Cliente.Cpf,
        VeiculoId = a.VeiculoId,
        VeiculoModelo = a.Veiculo!.Modelo,
        VeiculoPlaca = a.Veiculo.Placa,
        FabricanteNome = a.Veiculo.Fabricante!.Nome,
        DataRetirada = a.DataRetirada,
        DataPrevistaDevolucao = a.DataPrevistaDevolucao,
        DataDevolucao = a.DataDevolucao,
        QuilometragemInicial = a.QuilometragemInicial,
        QuilometragemFinal = a.QuilometragemFinal,
        KmRodados = a.QuilometragemFinal != null
            ? a.QuilometragemFinal - a.QuilometragemInicial
            : null,
        ValorDiaria = a.ValorDiaria,
        QuantidadeDiarias = a.QuantidadeDiarias,
        ValorMulta = a.ValorMulta,
        ValorTotal = a.ValorTotal,
        Status = a.Status.ToString(),
        Observacoes = a.Observacoes
    };

    /// <summary>Lista os alugueis, com filtros opcionais.</summary>
    /// <param name="clienteId">Id do cliente.</param>
    /// <param name="veiculoId">Id do veiculo.</param>
    /// <param name="status">Reservado, EmAndamento, Finalizado ou Cancelado.</param>
    /// <param name="dataInicio">Considera alugueis com retirada a partir desta data.</param>
    /// <param name="dataFim">Considera alugueis com retirada ate esta data.</param>
    /// <response code="200">Lista retornada com sucesso.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AluguelResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AluguelResponseDto>>> Listar(
        [FromQuery] int? clienteId,
        [FromQuery] int? veiculoId,
        [FromQuery] StatusAluguel? status,
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim)
    {
        var consulta = _contexto.Alugueis.AsNoTracking().AsQueryable();

        if (clienteId.HasValue) consulta = consulta.Where(a => a.ClienteId == clienteId.Value);
        if (veiculoId.HasValue) consulta = consulta.Where(a => a.VeiculoId == veiculoId.Value);
        if (status.HasValue) consulta = consulta.Where(a => a.Status == status.Value);
        if (dataInicio.HasValue) consulta = consulta.Where(a => a.DataRetirada >= dataInicio.Value);
        if (dataFim.HasValue) consulta = consulta.Where(a => a.DataRetirada <= dataFim.Value);

        return Ok(await consulta
            .OrderByDescending(a => a.DataRetirada)
            .Select(ParaDto)
            .ToListAsync());
    }

    /// <summary>Busca um aluguel pelo Id.</summary>
    /// <response code="200">Aluguel encontrado.</response>
    /// <response code="404">Aluguel nao encontrado.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AluguelResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AluguelResponseDto>> ObterPorId(int id)
    {
        var aluguel = await _contexto.Alugueis.AsNoTracking()
            .Where(a => a.Id == id).Select(ParaDto).FirstOrDefaultAsync();

        return aluguel is null
            ? NotFound(new ProblemDetails { Status = 404, Title = $"Aluguel {id} nao encontrado." })
            : Ok(aluguel);
    }

    /// <summary>Abre um novo aluguel (status inicial: Reservado).</summary>
    /// <remarks>
    /// O cliente e o carro precisam existir e o carro nao pode estar preso
    /// em outro aluguel no mesmo periodo. O km inicial vem do proprio carro
    /// e o total e a diaria vezes o numero de dias.
    /// </remarks>
    /// <response code="201">Aluguel criado.</response>
    /// <response code="400">Dados invalidos ou cliente/veiculo inexistente.</response>
    /// <response code="409">O veiculo ja esta comprometido nesse periodo.</response>
    [HttpPost]
    [ProducesResponseType(typeof(AluguelResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AluguelResponseDto>> Criar([FromBody] AluguelRequestDto dto)
    {
        if (!await _contexto.Clientes.AnyAsync(c => c.Id == dto.ClienteId))
        {
            return BadRequest(new ProblemDetails { Status = 400, Title = $"Cliente {dto.ClienteId} nao existe." });
        }

        var veiculo = await _contexto.Veiculos.FindAsync(dto.VeiculoId);
        if (veiculo is null)
        {
            return BadRequest(new ProblemDetails { Status = 400, Title = $"Veiculo {dto.VeiculoId} nao existe." });
        }

        if (await ExisteConflitoDePeriodoAsync(dto.VeiculoId, dto.DataRetirada, dto.DataPrevistaDevolucao))
        {
            return Conflict(new ProblemDetails
            {
                Status = 409,
                Title = "O veiculo ja possui um aluguel reservado ou em andamento nesse periodo."
            });
        }

        var diarias = CalcularDiarias(dto.DataRetirada, dto.DataPrevistaDevolucao);
        var valorDiaria = dto.ValorDiaria ?? veiculo.ValorDiaria;

        var aluguel = new Aluguel
        {
            ClienteId = dto.ClienteId,
            VeiculoId = dto.VeiculoId,
            DataRetirada = dto.DataRetirada,
            DataPrevistaDevolucao = dto.DataPrevistaDevolucao,
            QuilometragemInicial = veiculo.Quilometragem,
            ValorDiaria = valorDiaria,
            QuantidadeDiarias = diarias,
            ValorMulta = 0m,
            ValorTotal = valorDiaria * diarias,
            Status = StatusAluguel.Reservado,
            Observacoes = dto.Observacoes?.Trim()
        };

        _contexto.Alugueis.Add(aluguel);
        await _contexto.SaveChangesAsync();

        var resposta = await _contexto.Alugueis.AsNoTracking()
            .Where(a => a.Id == aluguel.Id).Select(ParaDto).FirstAsync();

        return CreatedAtAction(nameof(ObterPorId), new { id = aluguel.Id }, resposta);
    }

    /// <summary>Altera o periodo ou as observacoes de um aluguel ainda nao finalizado.</summary>
    /// <response code="204">Atualizado com sucesso.</response>
    /// <response code="400">Dados invalidos.</response>
    /// <response code="404">Aluguel nao encontrado.</response>
    /// <response code="409">Aluguel ja finalizado/cancelado ou conflito de periodo.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AluguelRequestDto dto)
    {
        var aluguel = await _contexto.Alugueis.FindAsync(id);
        if (aluguel is null)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = $"Aluguel {id} nao encontrado." });
        }

        if (aluguel.Status is StatusAluguel.Finalizado or StatusAluguel.Cancelado)
        {
            return Conflict(new ProblemDetails
            {
                Status = 409,
                Title = $"Um aluguel com status {aluguel.Status} nao pode mais ser alterado."
            });
        }

        if (await ExisteConflitoDePeriodoAsync(dto.VeiculoId, dto.DataRetirada, dto.DataPrevistaDevolucao, id))
        {
            return Conflict(new ProblemDetails
            {
                Status = 409,
                Title = "O veiculo ja possui outro aluguel nesse periodo."
            });
        }

        var diarias = CalcularDiarias(dto.DataRetirada, dto.DataPrevistaDevolucao);

        aluguel.DataRetirada = dto.DataRetirada;
        aluguel.DataPrevistaDevolucao = dto.DataPrevistaDevolucao;
        aluguel.QuantidadeDiarias = diarias;
        aluguel.ValorDiaria = dto.ValorDiaria ?? aluguel.ValorDiaria;
        aluguel.ValorTotal = (aluguel.ValorDiaria * diarias) + aluguel.ValorMulta;
        aluguel.Observacoes = dto.Observacoes?.Trim();

        await _contexto.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Confirma a retirada do veiculo (Reservado -> EmAndamento).</summary>
    /// <response code="200">Retirada registrada.</response>
    /// <response code="404">Aluguel nao encontrado.</response>
    /// <response code="409">O aluguel nao esta no status Reservado.</response>
    [HttpPut("{id:int}/retirada")]
    [ProducesResponseType(typeof(AluguelResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AluguelResponseDto>> RegistrarRetirada(int id)
    {
        var aluguel = await _contexto.Alugueis
            .Include(a => a.Veiculo)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (aluguel is null)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = $"Aluguel {id} nao encontrado." });
        }

        if (aluguel.Status != StatusAluguel.Reservado)
        {
            return Conflict(new ProblemDetails
            {
                Status = 409,
                Title = $"So e possivel registrar a retirada de um aluguel Reservado (atual: {aluguel.Status})."
            });
        }

        aluguel.Status = StatusAluguel.EmAndamento;
        aluguel.QuilometragemInicial = aluguel.Veiculo!.Quilometragem;
        aluguel.Veiculo.Disponivel = false;

        await _contexto.SaveChangesAsync();

        return Ok(await _contexto.Alugueis.AsNoTracking()
            .Where(a => a.Id == id).Select(ParaDto).FirstAsync());
    }

    /// <summary>Registra a devolucao do veiculo e fecha a conta do aluguel.</summary>
    /// <remarks>
    /// Atualiza o km do carro, libera ele para alugar de novo e,
    /// se voltou atrasado, cobra a diaria mais 20% por dia de atraso.
    /// </remarks>
    /// <response code="200">Devolucao registrada, com o valor total recalculado.</response>
    /// <response code="400">Quilometragem final menor que a inicial ou data anterior a retirada.</response>
    /// <response code="404">Aluguel nao encontrado.</response>
    /// <response code="409">O aluguel ja foi finalizado ou cancelado.</response>
    [HttpPut("{id:int}/devolucao")]
    [ProducesResponseType(typeof(AluguelResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AluguelResponseDto>> RegistrarDevolucao(
        int id, [FromBody] DevolucaoRequestDto dto)
    {
        var aluguel = await _contexto.Alugueis
            .Include(a => a.Veiculo)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (aluguel is null)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = $"Aluguel {id} nao encontrado." });
        }

        if (aluguel.Status is StatusAluguel.Finalizado or StatusAluguel.Cancelado)
        {
            return Conflict(new ProblemDetails
            {
                Status = 409,
                Title = $"Este aluguel ja esta com status {aluguel.Status}."
            });
        }

        if (dto.DataDevolucao < aluguel.DataRetirada)
        {
            return BadRequest(new ProblemDetails
            {
                Status = 400,
                Title = "A data de devolucao nao pode ser anterior a data de retirada."
            });
        }

        if (dto.QuilometragemFinal < aluguel.QuilometragemInicial)
        {
            return BadRequest(new ProblemDetails
            {
                Status = 400,
                Title = $"A quilometragem final ({dto.QuilometragemFinal}) nao pode ser menor "
                      + $"que a inicial ({aluguel.QuilometragemInicial})."
            });
        }

        // Multa por atraso
        var diasDeAtraso = CalcularDiasDeAtraso(aluguel.DataPrevistaDevolucao, dto.DataDevolucao);
        aluguel.ValorMulta = diasDeAtraso * aluguel.ValorDiaria * (1 + PercentualMultaAtraso);

        aluguel.DataDevolucao = dto.DataDevolucao;
        aluguel.QuilometragemFinal = dto.QuilometragemFinal;
        aluguel.ValorTotal = (aluguel.ValorDiaria * aluguel.QuantidadeDiarias) + aluguel.ValorMulta;
        aluguel.Status = StatusAluguel.Finalizado;

        if (!string.IsNullOrWhiteSpace(dto.Observacoes))
        {
            aluguel.Observacoes = dto.Observacoes.Trim();
        }

        // O carro volta para a frota com o km atualizado.
        aluguel.Veiculo!.Quilometragem = dto.QuilometragemFinal;
        aluguel.Veiculo.Disponivel = true;

        await _contexto.SaveChangesAsync();

        return Ok(await _contexto.Alugueis.AsNoTracking()
            .Where(a => a.Id == id).Select(ParaDto).FirstAsync());
    }

    /// <summary>Cancela um aluguel que ainda nao foi finalizado.</summary>
    /// <response code="200">Aluguel cancelado.</response>
    /// <response code="404">Aluguel nao encontrado.</response>
    /// <response code="409">O aluguel ja foi finalizado ou cancelado.</response>
    [HttpPut("{id:int}/cancelamento")]
    [ProducesResponseType(typeof(AluguelResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AluguelResponseDto>> Cancelar(int id)
    {
        var aluguel = await _contexto.Alugueis
            .Include(a => a.Veiculo)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (aluguel is null)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = $"Aluguel {id} nao encontrado." });
        }

        if (aluguel.Status is StatusAluguel.Finalizado or StatusAluguel.Cancelado)
        {
            return Conflict(new ProblemDetails
            {
                Status = 409,
                Title = $"Um aluguel com status {aluguel.Status} nao pode ser cancelado."
            });
        }

        aluguel.Status = StatusAluguel.Cancelado;
        aluguel.Veiculo!.Disponivel = true;

        await _contexto.SaveChangesAsync();

        return Ok(await _contexto.Alugueis.AsNoTracking()
            .Where(a => a.Id == id).Select(ParaDto).FirstAsync());
    }

    /// <summary>Exclui um aluguel (e seus pagamentos, por cascata).</summary>
    /// <response code="204">Excluido com sucesso.</response>
    /// <response code="404">Aluguel nao encontrado.</response>
    /// <response code="409">Alugueis em andamento nao podem ser excluidos.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Excluir(int id)
    {
        var aluguel = await _contexto.Alugueis.FindAsync(id);
        if (aluguel is null)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = $"Aluguel {id} nao encontrado." });
        }

        if (aluguel.Status == StatusAluguel.EmAndamento)
        {
            return Conflict(new ProblemDetails
            {
                Status = 409,
                Title = "Registre a devolucao ou cancele o aluguel antes de exclui-lo."
            });
        }

        _contexto.Alugueis.Remove(aluguel);
        await _contexto.SaveChangesAsync();
        return NoContent();
    }

    // Contas usadas pelas regras acima

    /// <summary>
    /// Conta as diarias. Sobrou hora, cobra o dia inteiro, e o minimo e 1 diaria.
    /// </summary>
    private static int CalcularDiarias(DateTime retirada, DateTime devolucaoPrevista)
    {
        var dias = (int)Math.Ceiling((devolucaoPrevista - retirada).TotalDays);
        return dias < 1 ? 1 : dias;
    }

    /// <summary>Quantos dias passaram do prazo, sempre arredondando para cima.</summary>
    private static int CalcularDiasDeAtraso(DateTime previsto, DateTime realizado)
    {
        if (realizado <= previsto)
        {
            return 0;
        }

        return (int)Math.Ceiling((realizado - previsto).TotalDays);
    }

    /// <summary>
    /// Olha se o carro ja esta preso em outro aluguel nesse periodo.
    /// Dois periodos batem quando A.inicio &lt; B.fim e B.inicio &lt; A.fim.
    /// </summary>
    private Task<bool> ExisteConflitoDePeriodoAsync(
        int veiculoId, DateTime inicio, DateTime fim, int? ignorarAluguelId = null)
        => _contexto.Alugueis.AnyAsync(a =>
            a.VeiculoId == veiculoId
            && (ignorarAluguelId == null || a.Id != ignorarAluguelId)
            && (a.Status == StatusAluguel.Reservado || a.Status == StatusAluguel.EmAndamento)
            && inicio < (a.DataDevolucao ?? a.DataPrevistaDevolucao)
            && a.DataRetirada < fim);
}
