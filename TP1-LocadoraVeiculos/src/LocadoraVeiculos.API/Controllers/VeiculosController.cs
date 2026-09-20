using System.Linq.Expressions;
using LocadoraVeiculos.API.DTOs;
using LocadoraVeiculos.API.Data;
using LocadoraVeiculos.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.API.Controllers;

/// <summary>CRUD dos veiculos da frota.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class VeiculosController : ControllerBase
{
    private readonly ApplicationContext _contexto;

    public VeiculosController(ApplicationContext contexto) => _contexto = contexto;

    /// <summary>
    /// Converte a entidade em DTO direto no banco. O EF transforma isso
    /// nas colunas do SELECT, sem trazer o objeto todo.
    /// </summary>
    private static readonly Expression<Func<Veiculo, VeiculoResponseDto>> ParaDto = v => new VeiculoResponseDto
    {
        Id = v.Id,
        Modelo = v.Modelo,
        Placa = v.Placa,
        AnoFabricacao = v.AnoFabricacao,
        Quilometragem = v.Quilometragem,
        Cor = v.Cor,
        Combustivel = v.Combustivel.ToString(),
        NumeroPassageiros = v.NumeroPassageiros,
        ValorDiaria = v.ValorDiaria,
        Disponivel = v.Disponivel,
        FabricanteId = v.FabricanteId,
        FabricanteNome = v.Fabricante!.Nome,
        CategoriaId = v.CategoriaId,
        CategoriaNome = v.Categoria!.Nome,
        FilialId = v.FilialId,
        FilialNome = v.Filial != null ? v.Filial.Nome : null
    };

    /// <summary>Lista os veiculos, com filtros opcionais por query string.</summary>
    /// <param name="modelo">Parte do modelo do veiculo.</param>
    /// <param name="fabricanteId">Id do fabricante.</param>
    /// <param name="categoriaId">Id da categoria.</param>
    /// <param name="filialId">Id da filial.</param>
    /// <param name="disponivel">true para apenas disponiveis, false para apenas indisponiveis.</param>
    /// <response code="200">Lista retornada com sucesso.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VeiculoResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VeiculoResponseDto>>> Listar(
        [FromQuery] string? modelo,
        [FromQuery] int? fabricanteId,
        [FromQuery] int? categoriaId,
        [FromQuery] int? filialId,
        [FromQuery] bool? disponivel)
    {
        var consulta = _contexto.Veiculos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(modelo)) consulta = consulta.Where(v => v.Modelo.Contains(modelo));
        if (fabricanteId.HasValue) consulta = consulta.Where(v => v.FabricanteId == fabricanteId.Value);
        if (categoriaId.HasValue) consulta = consulta.Where(v => v.CategoriaId == categoriaId.Value);
        if (filialId.HasValue) consulta = consulta.Where(v => v.FilialId == filialId.Value);
        if (disponivel.HasValue) consulta = consulta.Where(v => v.Disponivel == disponivel.Value);

        return Ok(await consulta.OrderBy(v => v.Modelo).Select(ParaDto).ToListAsync());
    }

    /// <summary>Busca um veiculo pelo Id.</summary>
    /// <response code="200">Veiculo encontrado.</response>
    /// <response code="404">Veiculo nao encontrado.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(VeiculoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VeiculoResponseDto>> ObterPorId(int id)
    {
        var veiculo = await _contexto.Veiculos
            .AsNoTracking()
            .Where(v => v.Id == id)
            .Select(ParaDto)
            .FirstOrDefaultAsync();

        return veiculo is null
            ? NotFound(new ProblemDetails { Status = 404, Title = $"Veiculo {id} nao encontrado." })
            : Ok(veiculo);
    }

    /// <summary>Cadastra um novo veiculo.</summary>
    /// <response code="201">Veiculo criado.</response>
    /// <response code="400">Dados invalidos ou fabricante/categoria/filial inexistente.</response>
    /// <response code="409">Ja existe um veiculo com essa placa.</response>
    [HttpPost]
    [ProducesResponseType(typeof(VeiculoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VeiculoResponseDto>> Criar([FromBody] VeiculoRequestDto dto)
    {
        var placa = NormalizarPlaca(dto.Placa);

        if (await _contexto.Veiculos.AnyAsync(v => v.Placa == placa))
        {
            return Conflict(new ProblemDetails { Status = 409, Title = $"Ja existe um veiculo com a placa {placa}." });
        }

        var erro = await ValidarRelacionamentosAsync(dto);
        if (erro is not null) return BadRequest(erro);

        var veiculo = new Veiculo
        {
            Modelo = dto.Modelo.Trim(),
            Placa = placa,
            AnoFabricacao = dto.AnoFabricacao,
            Quilometragem = dto.Quilometragem,
            Cor = dto.Cor?.Trim(),
            Combustivel = dto.Combustivel,
            NumeroPassageiros = dto.NumeroPassageiros,
            ValorDiaria = dto.ValorDiaria,
            Disponivel = dto.Disponivel,
            FabricanteId = dto.FabricanteId,
            CategoriaId = dto.CategoriaId,
            FilialId = dto.FilialId
        };

        _contexto.Veiculos.Add(veiculo);
        await _contexto.SaveChangesAsync();

        var resposta = await _contexto.Veiculos.AsNoTracking()
            .Where(v => v.Id == veiculo.Id).Select(ParaDto).FirstAsync();

        return CreatedAtAction(nameof(ObterPorId), new { id = veiculo.Id }, resposta);
    }

    /// <summary>Atualiza os dados de um veiculo.</summary>
    /// <response code="204">Atualizado com sucesso.</response>
    /// <response code="400">Dados invalidos ou relacionamento inexistente.</response>
    /// <response code="404">Veiculo nao encontrado.</response>
    /// <response code="409">A placa informada ja pertence a outro veiculo.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] VeiculoRequestDto dto)
    {
        var veiculo = await _contexto.Veiculos.FindAsync(id);
        if (veiculo is null)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = $"Veiculo {id} nao encontrado." });
        }

        var placa = NormalizarPlaca(dto.Placa);
        if (await _contexto.Veiculos.AnyAsync(v => v.Placa == placa && v.Id != id))
        {
            return Conflict(new ProblemDetails { Status = 409, Title = $"A placa {placa} ja pertence a outro veiculo." });
        }

        var erro = await ValidarRelacionamentosAsync(dto);
        if (erro is not null) return BadRequest(erro);

        veiculo.Modelo = dto.Modelo.Trim();
        veiculo.Placa = placa;
        veiculo.AnoFabricacao = dto.AnoFabricacao;
        veiculo.Quilometragem = dto.Quilometragem;
        veiculo.Cor = dto.Cor?.Trim();
        veiculo.Combustivel = dto.Combustivel;
        veiculo.NumeroPassageiros = dto.NumeroPassageiros;
        veiculo.ValorDiaria = dto.ValorDiaria;
        veiculo.Disponivel = dto.Disponivel;
        veiculo.FabricanteId = dto.FabricanteId;
        veiculo.CategoriaId = dto.CategoriaId;
        veiculo.FilialId = dto.FilialId;

        await _contexto.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Exclui um veiculo da frota.</summary>
    /// <response code="204">Excluido com sucesso.</response>
    /// <response code="404">Veiculo nao encontrado.</response>
    /// <response code="409">O veiculo possui alugueis registrados.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Excluir(int id)
    {
        var veiculo = await _contexto.Veiculos.FindAsync(id);
        if (veiculo is null)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = $"Veiculo {id} nao encontrado." });
        }

        if (await _contexto.Alugueis.AnyAsync(a => a.VeiculoId == id))
        {
            return Conflict(new ProblemDetails
            {
                Status = 409,
                Title = "Nao e possivel excluir um veiculo que possui alugueis registrados."
            });
        }

        _contexto.Veiculos.Remove(veiculo);
        await _contexto.SaveChangesAsync();
        return NoContent();
    }

    // Metodos de apoio

    /// <summary>Deixa a placa em maiuscula e tira traco e espaco.</summary>
    private static string NormalizarPlaca(string placa)
        => placa.Trim().Replace("-", string.Empty).ToUpperInvariant();

    /// <summary>
    /// Ve se os Ids de marca, categoria e filial existem mesmo.
    /// Volta null quando esta tudo certo.
    /// </summary>
    private async Task<ProblemDetails?> ValidarRelacionamentosAsync(VeiculoRequestDto dto)
    {
        if (!await _contexto.Fabricantes.AnyAsync(f => f.Id == dto.FabricanteId))
        {
            return new ProblemDetails { Status = 400, Title = $"Fabricante {dto.FabricanteId} nao existe." };
        }

        if (!await _contexto.Categorias.AnyAsync(c => c.Id == dto.CategoriaId))
        {
            return new ProblemDetails { Status = 400, Title = $"Categoria {dto.CategoriaId} nao existe." };
        }

        if (dto.FilialId.HasValue && !await _contexto.Filiais.AnyAsync(f => f.Id == dto.FilialId.Value))
        {
            return new ProblemDetails { Status = 400, Title = $"Filial {dto.FilialId} nao existe." };
        }

        return null;
    }
}
