using System.Linq.Expressions;
using LocadoraVeiculos.API.DTOs;
using LocadoraVeiculos.API.Data;
using LocadoraVeiculos.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.API.Controllers;

/// <summary>CRUD dos pagamentos vinculados aos alugueis.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PagamentosController : ControllerBase
{
    private readonly ApplicationContext _contexto;

    public PagamentosController(ApplicationContext contexto) => _contexto = contexto;

    private static readonly Expression<Func<Pagamento, PagamentoResponseDto>> ParaDto = p => new PagamentoResponseDto
    {
        Id = p.Id,
        AluguelId = p.AluguelId,
        Valor = p.Valor,
        FormaPagamento = p.FormaPagamento.ToString(),
        Status = p.Status.ToString(),
        DataPagamento = p.DataPagamento,
        CodigoTransacao = p.CodigoTransacao
    };

    /// <summary>Lista os pagamentos, com filtros opcionais.</summary>
    /// <param name="aluguelId">Id do aluguel.</param>
    /// <param name="status">Pendente, Aprovado, Recusado ou Estornado.</param>
    /// <response code="200">Lista retornada com sucesso.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PagamentoResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PagamentoResponseDto>>> Listar(
        [FromQuery] int? aluguelId, [FromQuery] StatusPagamento? status)
    {
        var consulta = _contexto.Pagamentos.AsNoTracking().AsQueryable();

        if (aluguelId.HasValue) consulta = consulta.Where(p => p.AluguelId == aluguelId.Value);
        if (status.HasValue) consulta = consulta.Where(p => p.Status == status.Value);

        return Ok(await consulta
            .OrderByDescending(p => p.DataPagamento)
            .Select(ParaDto)
            .ToListAsync());
    }

    /// <summary>Busca um pagamento pelo Id.</summary>
    /// <response code="200">Pagamento encontrado.</response>
    /// <response code="404">Pagamento nao encontrado.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PagamentoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagamentoResponseDto>> ObterPorId(int id)
    {
        var pagamento = await _contexto.Pagamentos.AsNoTracking()
            .Where(p => p.Id == id).Select(ParaDto).FirstOrDefaultAsync();

        return pagamento is null
            ? NotFound(new ProblemDetails { Status = 404, Title = $"Pagamento {id} nao encontrado." })
            : Ok(pagamento);
    }

    /// <summary>Registra um pagamento para um aluguel.</summary>
    /// <response code="201">Pagamento registrado.</response>
    /// <response code="400">Dados invalidos ou aluguel inexistente.</response>
    [HttpPost]
    [ProducesResponseType(typeof(PagamentoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagamentoResponseDto>> Criar([FromBody] PagamentoRequestDto dto)
    {
        if (!await _contexto.Alugueis.AnyAsync(a => a.Id == dto.AluguelId))
        {
            return BadRequest(new ProblemDetails { Status = 400, Title = $"Aluguel {dto.AluguelId} nao existe." });
        }

        var pagamento = new Pagamento
        {
            AluguelId = dto.AluguelId,
            Valor = dto.Valor,
            FormaPagamento = dto.FormaPagamento,
            Status = dto.Status,
            DataPagamento = DateTime.Now,
            CodigoTransacao = dto.CodigoTransacao?.Trim()
        };

        _contexto.Pagamentos.Add(pagamento);
        await _contexto.SaveChangesAsync();

        return CreatedAtAction(nameof(ObterPorId), new { id = pagamento.Id }, new PagamentoResponseDto
        {
            Id = pagamento.Id,
            AluguelId = pagamento.AluguelId,
            Valor = pagamento.Valor,
            FormaPagamento = pagamento.FormaPagamento.ToString(),
            Status = pagamento.Status.ToString(),
            DataPagamento = pagamento.DataPagamento,
            CodigoTransacao = pagamento.CodigoTransacao
        });
    }

    /// <summary>Atualiza um pagamento (por exemplo, mudando o status para Aprovado).</summary>
    /// <response code="204">Atualizado com sucesso.</response>
    /// <response code="400">Dados invalidos ou aluguel inexistente.</response>
    /// <response code="404">Pagamento nao encontrado.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] PagamentoRequestDto dto)
    {
        var pagamento = await _contexto.Pagamentos.FindAsync(id);
        if (pagamento is null)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = $"Pagamento {id} nao encontrado." });
        }

        if (!await _contexto.Alugueis.AnyAsync(a => a.Id == dto.AluguelId))
        {
            return BadRequest(new ProblemDetails { Status = 400, Title = $"Aluguel {dto.AluguelId} nao existe." });
        }

        pagamento.AluguelId = dto.AluguelId;
        pagamento.Valor = dto.Valor;
        pagamento.FormaPagamento = dto.FormaPagamento;
        pagamento.Status = dto.Status;
        pagamento.CodigoTransacao = dto.CodigoTransacao?.Trim();

        await _contexto.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Exclui um pagamento.</summary>
    /// <response code="204">Excluido com sucesso.</response>
    /// <response code="404">Pagamento nao encontrado.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(int id)
    {
        var pagamento = await _contexto.Pagamentos.FindAsync(id);
        if (pagamento is null)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = $"Pagamento {id} nao encontrado." });
        }

        _contexto.Pagamentos.Remove(pagamento);
        await _contexto.SaveChangesAsync();
        return NoContent();
    }
}
