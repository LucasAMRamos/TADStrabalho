using LocadoraVeiculos.API.DTOs;
using LocadoraVeiculos.API.Dados;
using LocadoraVeiculos.API.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.API.Controladores;

/// <summary>CRUD das filiais (lojas) da locadora.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FiliaisController : ControllerBase
{
    private readonly ApplicationContext _contexto;

    public FiliaisController(ApplicationContext contexto) => _contexto = contexto;

    /// <summary>Lista as filiais, opcionalmente filtrando por cidade ou estado.</summary>
    /// <param name="cidade">Parte do nome da cidade.</param>
    /// <param name="estado">Sigla da UF (ex.: MG).</param>
    /// <response code="200">Lista retornada com sucesso.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FilialResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FilialResponseDto>>> Listar(
        [FromQuery] string? cidade, [FromQuery] string? estado)
    {
        var consulta = _contexto.Filiais.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(cidade))
        {
            consulta = consulta.Where(f => f.Cidade.Contains(cidade));
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            consulta = consulta.Where(f => f.Estado == estado);
        }

        return Ok(await consulta
            .OrderBy(f => f.Cidade).ThenBy(f => f.Nome)
            .Select(f => new FilialResponseDto
            {
                Id = f.Id,
                Nome = f.Nome,
                Cidade = f.Cidade,
                Estado = f.Estado,
                Endereco = f.Endereco,
                Telefone = f.Telefone,
                TotalVeiculos = f.Veiculos.Count
            })
            .ToListAsync());
    }

    /// <summary>Busca uma filial pelo Id.</summary>
    /// <response code="200">Filial encontrada.</response>
    /// <response code="404">Filial nao encontrada.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(FilialResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FilialResponseDto>> ObterPorId(int id)
    {
        var filial = await _contexto.Filiais
            .AsNoTracking()
            .Where(f => f.Id == id)
            .Select(f => new FilialResponseDto
            {
                Id = f.Id,
                Nome = f.Nome,
                Cidade = f.Cidade,
                Estado = f.Estado,
                Endereco = f.Endereco,
                Telefone = f.Telefone,
                TotalVeiculos = f.Veiculos.Count
            })
            .FirstOrDefaultAsync();

        return filial is null
            ? NotFound(new ProblemDetails { Status = 404, Title = $"Filial {id} nao encontrada." })
            : Ok(filial);
    }

    /// <summary>Cadastra uma nova filial.</summary>
    /// <response code="201">Filial criada.</response>
    /// <response code="400">Dados invalidos.</response>
    [HttpPost]
    [ProducesResponseType(typeof(FilialResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FilialResponseDto>> Criar([FromBody] FilialRequestDto dto)
    {
        var filial = new Filial
        {
            Nome = dto.Nome.Trim(),
            Cidade = dto.Cidade.Trim(),
            Estado = dto.Estado.ToUpperInvariant(),
            Endereco = dto.Endereco?.Trim(),
            Telefone = dto.Telefone?.Trim()
        };

        _contexto.Filiais.Add(filial);
        await _contexto.SaveChangesAsync();

        return CreatedAtAction(nameof(ObterPorId), new { id = filial.Id }, new FilialResponseDto
        {
            Id = filial.Id,
            Nome = filial.Nome,
            Cidade = filial.Cidade,
            Estado = filial.Estado,
            Endereco = filial.Endereco,
            Telefone = filial.Telefone,
            TotalVeiculos = 0
        });
    }

    /// <summary>Atualiza uma filial.</summary>
    /// <response code="204">Atualizada com sucesso.</response>
    /// <response code="404">Filial nao encontrada.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] FilialRequestDto dto)
    {
        var filial = await _contexto.Filiais.FindAsync(id);
        if (filial is null)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = $"Filial {id} nao encontrada." });
        }

        filial.Nome = dto.Nome.Trim();
        filial.Cidade = dto.Cidade.Trim();
        filial.Estado = dto.Estado.ToUpperInvariant();
        filial.Endereco = dto.Endereco?.Trim();
        filial.Telefone = dto.Telefone?.Trim();

        await _contexto.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Exclui uma filial. Os veiculos vinculados NAO sao apagados:
    /// a coluna FilialId deles passa a ficar nula (regra SetNull da FK).
    /// </summary>
    /// <response code="204">Excluida com sucesso.</response>
    /// <response code="404">Filial nao encontrada.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(int id)
    {
        var filial = await _contexto.Filiais
            .Include(f => f.Veiculos)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (filial is null)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = $"Filial {id} nao encontrada." });
        }

        foreach (var veiculo in filial.Veiculos)
        {
            veiculo.FilialId = null;
        }

        _contexto.Filiais.Remove(filial);
        await _contexto.SaveChangesAsync();
        return NoContent();
    }
}
