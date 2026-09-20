using LocadoraVeiculos.API.DTOs;
using LocadoraVeiculos.API.Data;
using LocadoraVeiculos.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.API.Controllers;

/// <summary>CRUD de fabricantes (marcas) de veiculos.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FabricantesController : ControllerBase
{
    private readonly ApplicationContext _contexto;

    public FabricantesController(ApplicationContext contexto) => _contexto = contexto;

    /// <summary>Lista todos os fabricantes cadastrados.</summary>
    /// <param name="nome">Filtro opcional por parte do nome.</param>
    /// <response code="200">Lista retornada com sucesso.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FabricanteResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FabricanteResponseDto>>> Listar([FromQuery] string? nome)
    {
        var consulta = _contexto.Fabricantes.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(nome))
        {
            consulta = consulta.Where(f => f.Nome.Contains(nome));
        }

        var resultado = await consulta
            .OrderBy(f => f.Nome)
            .Select(f => new FabricanteResponseDto
            {
                Id = f.Id,
                Nome = f.Nome,
                PaisOrigem = f.PaisOrigem,
                AnoFundacao = f.AnoFundacao,
                TotalVeiculos = f.Veiculos.Count
            })
            .ToListAsync();

        return Ok(resultado);
    }

    /// <summary>Busca um fabricante pelo seu identificador.</summary>
    /// <response code="200">Fabricante encontrado.</response>
    /// <response code="404">Nao existe fabricante com o Id informado.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(FabricanteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FabricanteResponseDto>> ObterPorId(int id)
    {
        var fabricante = await _contexto.Fabricantes
            .AsNoTracking()
            .Where(f => f.Id == id)
            .Select(f => new FabricanteResponseDto
            {
                Id = f.Id,
                Nome = f.Nome,
                PaisOrigem = f.PaisOrigem,
                AnoFundacao = f.AnoFundacao,
                TotalVeiculos = f.Veiculos.Count
            })
            .FirstOrDefaultAsync();

        return fabricante is null
            ? NotFound(new ProblemDetails { Status = 404, Title = $"Fabricante {id} nao encontrado." })
            : Ok(fabricante);
    }

    /// <summary>Cadastra um novo fabricante.</summary>
    /// <response code="201">Fabricante criado.</response>
    /// <response code="400">Dados invalidos.</response>
    /// <response code="409">Ja existe um fabricante com esse nome.</response>
    [HttpPost]
    [ProducesResponseType(typeof(FabricanteResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FabricanteResponseDto>> Criar([FromBody] FabricanteRequestDto dto)
    {
        if (await _contexto.Fabricantes.AnyAsync(f => f.Nome == dto.Nome))
        {
            return Conflict(new ProblemDetails
            {
                Status = 409,
                Title = $"Ja existe um fabricante chamado '{dto.Nome}'."
            });
        }

        var fabricante = new Fabricante
        {
            Nome = dto.Nome.Trim(),
            PaisOrigem = dto.PaisOrigem.Trim(),
            AnoFundacao = dto.AnoFundacao
        };

        _contexto.Fabricantes.Add(fabricante);
        await _contexto.SaveChangesAsync();

        var resposta = new FabricanteResponseDto
        {
            Id = fabricante.Id,
            Nome = fabricante.Nome,
            PaisOrigem = fabricante.PaisOrigem,
            AnoFundacao = fabricante.AnoFundacao,
            TotalVeiculos = 0
        };

        return CreatedAtAction(nameof(ObterPorId), new { id = fabricante.Id }, resposta);
    }

    /// <summary>Atualiza um fabricante existente.</summary>
    /// <response code="204">Atualizado com sucesso.</response>
    /// <response code="404">Fabricante nao encontrado.</response>
    /// <response code="409">Outro fabricante ja usa esse nome.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] FabricanteRequestDto dto)
    {
        var fabricante = await _contexto.Fabricantes.FindAsync(id);
        if (fabricante is null)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = $"Fabricante {id} nao encontrado." });
        }

        if (await _contexto.Fabricantes.AnyAsync(f => f.Nome == dto.Nome && f.Id != id))
        {
            return Conflict(new ProblemDetails { Status = 409, Title = $"Ja existe um fabricante chamado '{dto.Nome}'." });
        }

        fabricante.Nome = dto.Nome.Trim();
        fabricante.PaisOrigem = dto.PaisOrigem.Trim();
        fabricante.AnoFundacao = dto.AnoFundacao;

        await _contexto.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Exclui um fabricante.</summary>
    /// <response code="204">Excluido com sucesso.</response>
    /// <response code="404">Fabricante nao encontrado.</response>
    /// <response code="409">O fabricante possui veiculos vinculados.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Excluir(int id)
    {
        var fabricante = await _contexto.Fabricantes.FindAsync(id);
        if (fabricante is null)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = $"Fabricante {id} nao encontrado." });
        }

        if (await _contexto.Veiculos.AnyAsync(v => v.FabricanteId == id))
        {
            return Conflict(new ProblemDetails
            {
                Status = 409,
                Title = "Nao e possivel excluir um fabricante que possui veiculos cadastrados."
            });
        }

        _contexto.Fabricantes.Remove(fabricante);
        await _contexto.SaveChangesAsync();
        return NoContent();
    }
}
