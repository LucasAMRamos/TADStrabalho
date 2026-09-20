using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.API.Middlewares;

/// <summary>
/// Pega qualquer erro que estoure nos controllers, grava no log e devolve
/// um JSON padronizado, em vez da tela feia de erro do servidor.
/// </summary>
public class TratamentoDeErrosMiddleware
{
    private readonly RequestDelegate _proximo;
    private readonly ILogger<TratamentoDeErrosMiddleware> _logger;
    private readonly IHostEnvironment _ambiente;

    public TratamentoDeErrosMiddleware(
        RequestDelegate proximo,
        ILogger<TratamentoDeErrosMiddleware> logger,
        IHostEnvironment ambiente)
    {
        _proximo = proximo;
        _logger = logger;
        _ambiente = ambiente;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _proximo(contexto);
        }
        catch (Exception excecao)
        {
            await TratarAsync(contexto, excecao);
        }
    }

    private async Task TratarAsync(HttpContext contexto, Exception excecao)
    {
        _logger.LogError(excecao, "Erro nao tratado em {Metodo} {Caminho}",
            contexto.Request.Method, contexto.Request.Path);

        // Se a resposta ja comecou a ser enviada nao da para trocar,
        // entao deixa o erro subir.
        if (contexto.Response.HasStarted)
        {
            throw excecao;
        }

        var (status, titulo) = excecao switch
        {
            // Bateu em alguma regra do banco: FK, UNIQUE, CHECK...
            DbUpdateException => (HttpStatusCode.Conflict,
                "A operacao viola uma regra de integridade do banco de dados."),

            ArgumentException or InvalidOperationException => (HttpStatusCode.BadRequest,
                "Requisicao invalida."),

            KeyNotFoundException => (HttpStatusCode.NotFound,
                "Recurso nao encontrado."),

            _ => (HttpStatusCode.InternalServerError,
                "Ocorreu um erro inesperado no servidor.")
        };

        var problema = new ProblemDetails
        {
            Status = (int)status,
            Title = titulo,
            Instance = contexto.Request.Path,
            // O detalhe tecnico so aparece quando esta em desenvolvimento.
            Detail = _ambiente.IsDevelopment() ? excecao.ToString() : null
        };

        contexto.Response.Clear();
        contexto.Response.StatusCode = (int)status;
        contexto.Response.ContentType = "application/problem+json";

        await contexto.Response.WriteAsync(JsonSerializer.Serialize(problema,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}

/// <summary>Atalho para chamar o middleware la no Program.cs.</summary>
public static class TratamentoDeErrosMiddlewareExtensions
{
    public static IApplicationBuilder UseTratamentoDeErros(this IApplicationBuilder app)
        => app.UseMiddleware<TratamentoDeErrosMiddleware>();
}
