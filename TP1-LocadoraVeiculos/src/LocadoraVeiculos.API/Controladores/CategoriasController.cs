using LocadoraVeiculos.API.DTOs;
using LocadoraVeiculos.API.Dados;
using LocadoraVeiculos.API.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.API.Controladores;

/// <summary>CRUD das categorias (grupos tarifarios) de veiculos.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriasController : ControllerBase
{
    private readonly ApplicationContext _contexto;

    public CategoriasController(ApplicationContext contexto) => _contexto = contexto;

    /// <summary>Lista as categorias cadastradas.</summary>
    /// <response code="200">Lista retornada com sucesso.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoriaResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CategoriaResponseDto>>> Listar()
        => Ok(await _contexto.Categorias
            .AsNoTracking()
            .OrderBy(c => c.Nome)
            .Select(c => new CategoriaResponseDto
            {
                Id = c.Id,
                Nome = c.Nome,
                Descricao = c.Descricao,
                ValorDiariaSugerido = c.ValorDiariaSugerido,
                TotalVeiculos = c.Veiculos.Count
            })
            .ToListAsync());

    /// <summary>Busca uma categoria pelo Id.</summary>
    /// <response code="200">Categoria encontrada.</response>
    /// <response code="404">Categoria nao encontrada.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CategoriaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoriaResponseDto>> ObterPorId(int id)
    {
        var categoria = await _contexto.Categorias
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CategoriaResponseDto
            {
                Id = c.Id,
                Nome = c.Nome,
                Descricao = c.Descricao,
                ValorDiariaSugerido = c.ValorDiariaSugerido,
                TotalVeiculos = c.Veiculos.Count
            })
            .FirstOrDefaultAsync();

        return categoria is null
            ? NotFound(new ProblemDetails { Status = 404, Title = $"Categoria {id} nao encontrada." })
            : Ok(categoria);
    }

    /// <summary>Cadastra uma nova categoria.</summary>
    /// <response code="201">Categoria criada.</response>
    /// <response code="400">Dados invalidos.</response>
    /// <response code="409">Ja existe categoria com esse nome.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CategoriaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoriaResponseDto>> Criar([FromBody] CategoriaRequestDto dto)
    {
        if (await _contexto.Categorias.AnyAsync(c => c.Nome == dto.Nome))
        {
            return Conflict(new ProblemDetails { Status = 409, Title = $"Ja existe a categoria '{dto.Nome}'." });
        }

        var categoria = new Categoria
        {
            Nome = dto.Nome.Trim(),
            Descricao = dto.Descricao?.Trim(),
            ValorDiariaSugerido = dto.ValorDiariaSugerido
        };

        _contexto.Categorias.Add(categoria);
        await _contexto.SaveChangesAsync();

        return CreatedAtAction(nameof(ObterPorId), new { id = categoria.Id }, new CategoriaResponseDto
        {
            Id = categoria.Id,
            Nome = categoria.Nome,
            Descricao = categoria.Descricao,
            ValorDiariaSugerido = categoria.ValorDiariaSugerido,
            TotalVeiculos = 0
        });
    }

    /// <summary>Atualiza uma categoria.</summary>
    /// <response code="204">Atualizada com sucesso.</response>
    /// <response code="404">Categoria nao encontrada.</response>
    /// <response code="409">Outra categoria ja usa esse nome.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] CategoriaRequestDto dto)
    {
        var categoria = await _contexto.Categorias.FindAsync(id);
        if (categoria is null)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = $"Categoria {id} nao encontrada." });
        }

        if (await _contexto.Categorias.AnyAsync(c => c.Nome == dto.Nome && c.Id != id))
        {
            return Conflict(new ProblemDetails { Status = 409, Title = $"Ja existe a categoria '{dto.Nome}'." });
        }

        categoria.Nome = dto.Nome.Trim();
        categoria.Descricao = dto.Descricao?.Trim();
        categoria.ValorDiariaSugerido = dto.ValorDiariaSugerido;

        await _contexto.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Exclui uma categoria.</summary>
    /// <response code="204">Excluida com sucesso.</response>
    /// <response code="404">Categoria nao encontrada.</response>
    /// <response code="409">A categoria possui veiculos vinculados.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Excluir(int id)
    {
        var categoria = await _contexto.Categorias.FindAsync(id);
        if (categoria is null)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = $"Categoria {id} nao encontrada." });
        }

        if (await _contexto.Veiculos.AnyAsync(v => v.CategoriaId == id))
        {
            return Conflict(new ProblemDetails
            {
                Status = 409,
                Title = "Nao e possivel excluir uma categoria que possui veiculos cadastrados."
            });
        }

        _contexto.Categorias.Remove(categoria);
        await _contexto.SaveChangesAsync();
        return NoContent();
    }
}
