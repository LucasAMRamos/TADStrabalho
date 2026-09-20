using LocadoraVeiculos.API.Modelos;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.API.Dados;

/// <summary>
/// Cria o banco, se ainda nao existir, e joga alguns registros dentro
/// para as consultas terem o que mostrar.
/// </summary>
public static class DatabaseInitializer
{
    public static async Task InicializarAsync(IServiceProvider servicos)
    {
        using var escopo = servicos.CreateScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<ApplicationContext>();
        var logger = escopo.ServiceProvider.GetRequiredService<ILoggerFactory>()
                                           .CreateLogger(nameof(DatabaseInitializer));

        // Se ja existe migration, usa ela. Se nao, cria o banco pelo modelo mesmo.
        if (contexto.Database.GetMigrations().Any())
        {
            logger.LogInformation("Aplicando migrations pendentes...");
            await contexto.Database.MigrateAsync();
        }
        else
        {
            logger.LogInformation("Nenhuma migration encontrada. Criando o banco a partir do modelo.");
            await contexto.Database.EnsureCreatedAsync();
        }

        // So popula uma vez. Se ja tem fabricante, sai fora.
        if (await contexto.Fabricantes.AnyAsync())
        {
            logger.LogInformation("Banco ja populado. Carga inicial ignorada.");
            return;
        }

        logger.LogInformation("Inserindo carga inicial de dados...");
        await PopularAsync(contexto);
        logger.LogInformation("Carga inicial concluida.");
    }

    private static async Task PopularAsync(ApplicationContext contexto)
    {
        // ------------------------- Fabricantes -------------------------
        var fiat = new Fabricante { Nome = "Fiat", PaisOrigem = "Italia", AnoFundacao = 1899 };
        var volks = new Fabricante { Nome = "Volkswagen", PaisOrigem = "Alemanha", AnoFundacao = 1937 };
        var toyota = new Fabricante { Nome = "Toyota", PaisOrigem = "Japao", AnoFundacao = 1937 };
        var chevrolet = new Fabricante { Nome = "Chevrolet", PaisOrigem = "Estados Unidos", AnoFundacao = 1911 };
        var jeep = new Fabricante { Nome = "Jeep", PaisOrigem = "Estados Unidos", AnoFundacao = 1943 };
        contexto.Fabricantes.AddRange(fiat, volks, toyota, chevrolet, jeep);

        // ------------------------- Categorias --------------------------
        var economico = new Categoria { Nome = "Economico", Descricao = "Hatch 1.0, ideal para cidade", ValorDiariaSugerido = 120m };
        var intermediario = new Categoria { Nome = "Intermediario", Descricao = "Sedan compacto", ValorDiariaSugerido = 180m };
        var suv = new Categoria { Nome = "SUV", Descricao = "Utilitario esportivo", ValorDiariaSugerido = 260m };
        var premium = new Categoria { Nome = "Premium", Descricao = "Veiculos de luxo", ValorDiariaSugerido = 420m };
        var utilitario = new Categoria { Nome = "Utilitario", Descricao = "Carga leve", ValorDiariaSugerido = 200m };
        contexto.Categorias.AddRange(economico, intermediario, suv, premium, utilitario);

        // --------------------------- Filiais ---------------------------
        var filialBh = new Filial { Nome = "Filial Centro - BH", Cidade = "Belo Horizonte", Estado = "MG", Endereco = "Av. Afonso Pena, 1000", Telefone = "(31) 3333-1000" };
        var filialSp = new Filial { Nome = "Filial Paulista - SP", Cidade = "Sao Paulo", Estado = "SP", Endereco = "Av. Paulista, 2200", Telefone = "(11) 3333-2000" };
        var filialConf = new Filial { Nome = "Filial Aeroporto Confins", Cidade = "Confins", Estado = "MG", Endereco = "Rod. LMG-800, s/n", Telefone = "(31) 3333-3000" };
        contexto.Filiais.AddRange(filialBh, filialSp, filialConf);

        // --------------------------- Veiculos --------------------------
        var veiculos = new List<Veiculo>
        {
            new() { Modelo = "Argo 1.0 Drive",  Placa = "RKA1B23", AnoFabricacao = 2023, Quilometragem = 15400, Cor = "Branco",  Combustivel = TipoCombustivel.Flex,     NumeroPassageiros = 5, ValorDiaria = 130m, Fabricante = fiat,       Categoria = economico,     Filial = filialBh },
            new() { Modelo = "Mobi Like",       Placa = "RKB2C34", AnoFabricacao = 2022, Quilometragem = 32100, Cor = "Prata",   Combustivel = TipoCombustivel.Flex,     NumeroPassageiros = 5, ValorDiaria = 110m, Fabricante = fiat,       Categoria = economico,     Filial = filialBh },
            new() { Modelo = "Polo Track",      Placa = "RKC3D45", AnoFabricacao = 2024, Quilometragem = 8200,  Cor = "Cinza",   Combustivel = TipoCombustivel.Flex,     NumeroPassageiros = 5, ValorDiaria = 150m, Fabricante = volks,      Categoria = economico,     Filial = filialSp },
            new() { Modelo = "Virtus Highline", Placa = "RKD4E56", AnoFabricacao = 2023, Quilometragem = 21800, Cor = "Preto",   Combustivel = TipoCombustivel.Flex,     NumeroPassageiros = 5, ValorDiaria = 195m, Fabricante = volks,      Categoria = intermediario, Filial = filialSp },
            new() { Modelo = "Corolla XEi",     Placa = "RKE5F67", AnoFabricacao = 2024, Quilometragem = 6400,  Cor = "Prata",   Combustivel = TipoCombustivel.Flex,     NumeroPassageiros = 5, ValorDiaria = 280m, Fabricante = toyota,     Categoria = intermediario, Filial = filialConf },
            new() { Modelo = "Corolla Cross",   Placa = "RKF6G78", AnoFabricacao = 2024, Quilometragem = 4300,  Cor = "Branco",  Combustivel = TipoCombustivel.Hibrido,  NumeroPassageiros = 5, ValorDiaria = 340m, Fabricante = toyota,     Categoria = suv,           Filial = filialConf },
            new() { Modelo = "Tracker Premier", Placa = "RKG7H89", AnoFabricacao = 2023, Quilometragem = 19500, Cor = "Vermelho",Combustivel = TipoCombustivel.Gasolina, NumeroPassageiros = 5, ValorDiaria = 300m, Fabricante = chevrolet,  Categoria = suv,           Filial = filialBh },
            new() { Modelo = "Montana LTZ",     Placa = "RKH8I90", AnoFabricacao = 2022, Quilometragem = 41200, Cor = "Branco",  Combustivel = TipoCombustivel.Flex,     NumeroPassageiros = 2, ValorDiaria = 210m, Fabricante = chevrolet,  Categoria = utilitario,    Filial = filialSp },
            new() { Modelo = "Compass Limited", Placa = "RKI9J01", AnoFabricacao = 2024, Quilometragem = 3100,  Cor = "Cinza",   Combustivel = TipoCombustivel.Diesel,   NumeroPassageiros = 5, ValorDiaria = 450m, Fabricante = jeep,       Categoria = premium,       Filial = filialConf },
            // Esse nao tem filial e nunca foi alugado, serve para testar o LEFT JOIN.
            new() { Modelo = "Renegade Sport",  Placa = "RKJ0K12", AnoFabricacao = 2021, Quilometragem = 58700, Cor = "Verde",   Combustivel = TipoCombustivel.Flex,     NumeroPassageiros = 5, ValorDiaria = 240m, Fabricante = jeep,       Categoria = suv,           Filial = null }
        };
        contexto.Veiculos.AddRange(veiculos);

        // --------------------------- Clientes --------------------------
        var maria   = new Cliente { Nome = "Maria Oliveira",   Cpf = "52998224725", Email = "maria.oliveira@email.com",  Telefone = "(31) 99999-0001", DataNascimento = new DateTime(1990, 4, 12), NumeroCnh = "01234567890" };
        var joao    = new Cliente { Nome = "Joao Pereira",     Cpf = "11144477735", Email = "joao.pereira@email.com",    Telefone = "(31) 99999-0002", DataNascimento = new DateTime(1985, 9, 3),  NumeroCnh = "01234567891" };
        var ana     = new Cliente { Nome = "Ana Souza",        Cpf = "12345678909", Email = "ana.souza@email.com",       Telefone = "(11) 98888-0003", DataNascimento = new DateTime(1998, 1, 25), NumeroCnh = "01234567892" };
        var carlos  = new Cliente { Nome = "Carlos Mendes",    Cpf = "39053344705", Email = "carlos.mendes@email.com",   Telefone = "(11) 98888-0004", DataNascimento = new DateTime(1979, 7, 30), NumeroCnh = "01234567893" };
        // Cliente que nunca alugou, aparece zerado no LEFT JOIN.
        var beatriz = new Cliente { Nome = "Beatriz Lima",     Cpf = "98765432100", Email = "beatriz.lima@email.com",    Telefone = "(31) 97777-0005", DataNascimento = new DateTime(2000, 11, 8) };
        contexto.Clientes.AddRange(maria, joao, ana, carlos, beatriz);

        // --------------------------- Alugueis --------------------------
        var hoje = DateTime.Today;
        var alugueis = new List<Aluguel>
        {
            // 1) Finalizado sem atraso
            CriarFinalizado(maria, veiculos[0], hoje.AddDays(-60), 5, kmRodados: 640, atrasoEmDias: 0),
            // 2) Finalizado com 2 dias de atraso, entao tem multa
            CriarFinalizado(joao, veiculos[3], hoje.AddDays(-45), 3, kmRodados: 980, atrasoEmDias: 2),
            // 3) Finalizado
            CriarFinalizado(ana, veiculos[4], hoje.AddDays(-30), 7, kmRodados: 1520, atrasoEmDias: 0),
            // 4) Finalizado
            CriarFinalizado(maria, veiculos[6], hoje.AddDays(-20), 2, kmRodados: 310, atrasoEmDias: 0),
            // 5) Rolando, ainda no prazo
            CriarEmAndamento(carlos, veiculos[8], hoje.AddDays(-2), 6),
            // 6) Rolando e atrasado, devia ter voltado ha 3 dias
            CriarEmAndamento(joao, veiculos[5], hoje.AddDays(-10), 7),
            // 7) Reserva futura
            CriarReservado(ana, veiculos[2], hoje.AddDays(5), 4)
        };
        contexto.Alugueis.AddRange(alugueis);

        // -------------------------- Pagamentos -------------------------
        var pagamentos = new List<Pagamento>
        {
            new() { Aluguel = alugueis[0], Valor = alugueis[0].ValorTotal, FormaPagamento = FormaPagamento.Pix,           Status = StatusPagamento.Aprovado, DataPagamento = alugueis[0].DataRetirada, CodigoTransacao = "PIX-000001" },
            new() { Aluguel = alugueis[1], Valor = alugueis[1].ValorTotal, FormaPagamento = FormaPagamento.CartaoCredito, Status = StatusPagamento.Aprovado, DataPagamento = alugueis[1].DataRetirada, CodigoTransacao = "CC-000002" },
            new() { Aluguel = alugueis[2], Valor = alugueis[2].ValorTotal, FormaPagamento = FormaPagamento.CartaoDebito,  Status = StatusPagamento.Aprovado, DataPagamento = alugueis[2].DataRetirada, CodigoTransacao = "CD-000003" },
            new() { Aluguel = alugueis[3], Valor = alugueis[3].ValorTotal, FormaPagamento = FormaPagamento.Dinheiro,      Status = StatusPagamento.Aprovado, DataPagamento = alugueis[3].DataRetirada },
            new() { Aluguel = alugueis[4], Valor = alugueis[4].ValorTotal, FormaPagamento = FormaPagamento.Boleto,        Status = StatusPagamento.Pendente, DataPagamento = alugueis[4].DataRetirada, CodigoTransacao = "BOL-000005" },
            new() { Aluguel = alugueis[5], Valor = alugueis[5].ValorTotal, FormaPagamento = FormaPagamento.Pix,           Status = StatusPagamento.Aprovado, DataPagamento = alugueis[5].DataRetirada, CodigoTransacao = "PIX-000006" }
        };
        contexto.Pagamentos.AddRange(pagamentos);

        await contexto.SaveChangesAsync();
    }

    // Metodos que montam os alugueis com datas, km e valores batendo

    private static Aluguel CriarReservado(Cliente cliente, Veiculo veiculo, DateTime retirada, int diarias)
        => new()
        {
            Cliente = cliente,
            Veiculo = veiculo,
            DataRetirada = retirada,
            DataPrevistaDevolucao = retirada.AddDays(diarias),
            QuilometragemInicial = veiculo.Quilometragem,
            ValorDiaria = veiculo.ValorDiaria,
            QuantidadeDiarias = diarias,
            ValorMulta = 0m,
            ValorTotal = veiculo.ValorDiaria * diarias,
            Status = StatusAluguel.Reservado
        };

    private static Aluguel CriarEmAndamento(Cliente cliente, Veiculo veiculo, DateTime retirada, int diarias)
    {
        var aluguel = CriarReservado(cliente, veiculo, retirada, diarias);
        aluguel.Status = StatusAluguel.EmAndamento;

        // Com o aluguel rolando, o carro sai da lista de disponiveis.
        veiculo.Disponivel = false;

        return aluguel;
    }

    private static Aluguel CriarFinalizado(
        Cliente cliente, Veiculo veiculo, DateTime retirada, int diarias, int kmRodados, int atrasoEmDias)
    {
        var aluguel = CriarReservado(cliente, veiculo, retirada, diarias);

        aluguel.Status = StatusAluguel.Finalizado;
        aluguel.DataDevolucao = aluguel.DataPrevistaDevolucao.AddDays(atrasoEmDias);
        aluguel.QuilometragemFinal = aluguel.QuilometragemInicial + kmRodados;

        // Cada dia de atraso custa a diaria mais 20%.
        aluguel.ValorMulta = atrasoEmDias * aluguel.ValorDiaria * 1.20m;
        aluguel.ValorTotal = (aluguel.ValorDiaria * diarias) + aluguel.ValorMulta;

        // O km do carro e atualizado na devolucao.
        veiculo.Quilometragem = aluguel.QuilometragemFinal.Value;

        return aluguel;
    }
}
