using System.Linq.Expressions;
using LocadoraVeiculos.API.DTOs;
using LocadoraVeiculos.API.Data;
using LocadoraVeiculos.API.Models;
using LocadoraVeiculos.API.Validacoes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.API.Controllers;

/// <summary>CRUD dos clientes da locadora.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ClientesController : ControllerBase
{
    private readonly ApplicationContext _contexto;

    public ClientesController(ApplicationContext contexto) => _contexto = contexto;

    private static readonly Expression<Func<Cliente, ClienteResponseDto>> ParaDto = c => new ClienteResponseDto
    {
        Id = c.Id,
        Nome = c.Nome,
        Cpf = c.Cpf,
        Email = c.Email,
        Telefone = c.Telefone,
        DataNascimento = c.DataNascimento,
        NumeroCnh = c.NumeroCnh,
        DataCadastro = c.DataCadastro,
        TotalAlugueis = c.Alugueis.Count
    };

    /// <summary>Lista os clientes, com filtros opcionais por nome ou CPF.</summary>
    /// <param name="nome">Parte do nome do cliente.</param>
    /// <param name="cpf">CPF com ou sem pontuacao.</param>
    /// <response code="200">Lista retornada com sucesso.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClienteResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ClienteResponseDto>>> Listar(
        [FromQuery] string? nome, [FromQuery] string? cpf)
    {
        var consulta = _contexto.Clientes.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(nome))
        {
            consulta = consulta.Where(c => c.Nome.Contains(nome));
        }

        if (!string.IsNullOrWhiteSpace(cpf))
        {
            var somenteDigitos = CpfAttribute.SomenteDigitos(cpf);
            consulta = consulta.Where(c => c.Cpf == somenteDigitos);
        }

        return Ok(await consulta.OrderBy(c => c.Nome).Select(ParaDto).ToListAsync());
    }

    /// <summary>Busca um cliente pelo Id.</summary>
    /// <response code="200">Cliente encontrado.</response>
    /// <response code="404">Cliente nao encontrado.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteResponseDto>> ObterPorId(int id)
    {
        var cliente = await _contexto.Clientes.AsNoTracking()
            .Where(c => c.Id == id).Select(ParaDto).FirstOrDefaultAsync();

        return cliente is null
            ? NotFound(new ProblemDetails { Status = 404, Title = $"Cliente {id} nao encontrado." })
            : Ok(cliente);
    }

    /// <summary>Cadastra um novo cliente.</summary>
    /// <remarks>O CPF passa pela conta dos digitos verificadores e e gravado so com numeros.</remarks>
    /// <response code="201">Cliente criado.</response>
    /// <response code="400">Dados invalidos (CPF ou e-mail fora do padrao).</response>
    /// <response code="409">CPF ou e-mail ja cadastrados.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClienteResponseDto>> Criar([FromBody] ClienteRequestDto dto)
    {
        var cpf = CpfAttribute.SomenteDigitos(dto.Cpf);
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _contexto.Clientes.AnyAsync(c => c.Cpf == cpf))
        {
            return Conflict(new ProblemDetails { Status = 409, Title = "Ja existe um cliente com esse CPF." });
        }

        if (await _contexto.Clientes.AnyAsync(c => c.Email == email))
        {
            return Conflict(new ProblemDetails { Status = 409, Title = "Ja existe um cliente com esse e-mail." });
        }

        var cliente = new Cliente
        {
            Nome = dto.Nome.Trim(),
            Cpf = cpf,
            Email = email,
            Telefone = dto.Telefone?.Trim(),
            DataNascimento = dto.DataNascimento,
            NumeroCnh = dto.NumeroCnh?.Trim(),
            DataCadastro = DateTime.Now
        };

        _contexto.Clientes.Add(cliente);
        await _contexto.SaveChangesAsync();

        return CreatedAtAction(nameof(ObterPorId), new { id = cliente.Id }, new ClienteResponseDto
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            Cpf = cliente.Cpf,
            Email = cliente.Email,
            Telefone = cliente.Telefone,
            DataNascimento = cliente.DataNascimento,
            NumeroCnh = cliente.NumeroCnh,
            DataCadastro = cliente.DataCadastro,
            TotalAlugueis = 0
        });
    }

    /// <summary>Atualiza os dados de um cliente.</summary>
    /// <response code="204">Atualizado com sucesso.</response>
    /// <response code="400">Dados invalidos.</response>
    /// <response code="404">Cliente nao encontrado.</response>
    /// <response code="409">CPF ou e-mail ja usados por outro cliente.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] ClienteRequestDto dto)
    {
        var cliente = await _contexto.Clientes.FindAsync(id);
        if (cliente is null)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = $"Cliente {id} nao encontrado." });
        }

        var cpf = CpfAttribute.SomenteDigitos(dto.Cpf);
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _contexto.Clientes.AnyAsync(c => c.Cpf == cpf && c.Id != id))
        {
            return Conflict(new ProblemDetails { Status = 409, Title = "Esse CPF ja pertence a outro cliente." });
        }

        if (await _contexto.Clientes.AnyAsync(c => c.Email == email && c.Id != id))
        {
            return Conflict(new ProblemDetails { Status = 409, Title = "Esse e-mail ja pertence a outro cliente." });
        }

        cliente.Nome = dto.Nome.Trim();
        cliente.Cpf = cpf;
        cliente.Email = email;
        cliente.Telefone = dto.Telefone?.Trim();
        cliente.DataNascimento = dto.DataNascimento;
        cliente.NumeroCnh = dto.NumeroCnh?.Trim();

        await _contexto.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Exclui um cliente.</summary>
    /// <response code="204">Excluido com sucesso.</response>
    /// <response code="404">Cliente nao encontrado.</response>
    /// <response code="409">O cliente possui alugueis registrados.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Excluir(int id)
    {
        var cliente = await _contexto.Clientes.FindAsync(id);
        if (cliente is null)
        {
            return NotFound(new ProblemDetails { Status = 404, Title = $"Cliente {id} nao encontrado." });
        }

        if (await _contexto.Alugueis.AnyAsync(a => a.ClienteId == id))
        {
            return Conflict(new ProblemDetails
            {
                Status = 409,
                Title = "Nao e possivel excluir um cliente que possui alugueis registrados."
            });
        }

        _contexto.Clientes.Remove(cliente);
        await _contexto.SaveChangesAsync();
        return NoContent();
    }
}
