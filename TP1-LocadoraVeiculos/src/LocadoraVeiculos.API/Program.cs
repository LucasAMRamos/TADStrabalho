using System.Reflection;
using System.Text.Json.Serialization;
using LocadoraVeiculos.API.Dados;
using LocadoraVeiculos.API.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1) Banco de dados: EF Core com SQL Server Express
var stringDeConexao = builder.Configuration.GetConnectionString("SqlServer")
    ?? throw new InvalidOperationException(
        "A connection string 'SqlServer' nao foi encontrada no appsettings.json.");

builder.Services.AddDbContext<ApplicationContext>(opcoes =>
    opcoes.UseSqlServer(stringDeConexao, sql => sql.EnableRetryOnFailure(3)));

// 2) Controllers e o jeito de montar o JSON
builder.Services
    .AddControllers()
    .AddJsonOptions(opcoes =>
    {
        // Enum sai como texto ("Flex") e nao como numero (3).
        opcoes.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        // Evita o loop infinito Veiculo -> Aluguel -> Veiculo.
        opcoes.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// 3) Deixa o erro de validacao sempre no mesmo formato:
//    se o ModelState nao passa, devolve 400 com a lista do que deu errado.
builder.Services.Configure<ApiBehaviorOptions>(opcoes =>
{
    opcoes.InvalidModelStateResponseFactory = contexto =>
    {
        var problema = new ValidationProblemDetails(contexto.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Um ou mais campos enviados sao invalidos.",
            Instance = contexto.HttpContext.Request.Path
        };

        return new BadRequestObjectResult(problema)
        {
            ContentTypes = { "application/problem+json" }
        };
    };
});

// 4) Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opcoes =>
{
    opcoes.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "API - Locadora de Veiculos",
        Description = """
            API REST para gestao de uma locadora de veiculos.

            Permite o CRUD completo de Fabricantes, Categorias, Filiais, Veiculos,
            Clientes, Alugueis e Pagamentos, alem de 6 consultas com JOIN entre tabelas
            (INNER JOIN, LEFT JOIN e agrupamentos) disponiveis em /api/consultas.

            Trabalho Pratico - Banco de Dados / Backend.
            """,
        Contact = new OpenApiContact { Name = "Lucas A. M. Ramos" }
    });

    // Puxa para o Swagger os comentarios escritos no codigo.
    var arquivoXml = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var caminhoXml = Path.Combine(AppContext.BaseDirectory, arquivoXml);
    if (File.Exists(caminhoXml))
    {
        opcoes.IncludeXmlComments(caminhoXml);
    }
});

var app = builder.Build();

// 5) Ordem de execucao

// O tratamento de erro vem primeiro, para pegar tudo que vier depois.
app.UseTratamentoDeErros();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opcoes =>
    {
        opcoes.SwaggerEndpoint("/swagger/v1/swagger.json", "Locadora de Veiculos v1");
        // Abre o Swagger na raiz, em https://localhost:7100/.
        opcoes.RoutePrefix = string.Empty;
        opcoes.DocumentTitle = "API - Locadora de Veiculos";
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// 6) Cria o banco e joga os dados de exemplo
await DatabaseInitializer.InicializarAsync(app.Services);

app.Run();
