using LocadoraVeiculos.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.API.Data;

/// <summary>
/// Contexto do EF Core: liga as classes C# as tabelas do SQL Server.
/// Cada DbSet vira uma tabela e o OnModelCreating descreve as colunas,
/// as chaves e as restricoes.
/// </summary>
public class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
    }

    // Cada DbSet abaixo e uma tabela do banco.
    public DbSet<Fabricante> Fabricantes => Set<Fabricante>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Filial> Filiais => Set<Filial>();
    public DbSet<Veiculo> Veiculos => Set<Veiculo>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Aluguel> Alugueis => Set<Aluguel>();
    public DbSet<Pagamento> Pagamentos => Set<Pagamento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigurarFabricante(modelBuilder);
        ConfigurarCategoria(modelBuilder);
        ConfigurarFilial(modelBuilder);
        ConfigurarVeiculo(modelBuilder);
        ConfigurarCliente(modelBuilder);
        ConfigurarAluguel(modelBuilder);
        ConfigurarPagamento(modelBuilder);
    }

    // ==================================================================
    // FABRICANTE
    // ==================================================================
    private static void ConfigurarFabricante(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Fabricante>(entidade =>
        {
            entidade.ToTable("Fabricante");

            // ---- Chave primaria ----
            entidade.HasKey(f => f.Id)
                    .HasName("PK_Fabricante");
            entidade.Property(f => f.Id).ValueGeneratedOnAdd();

            // ---- Colunas ----
            entidade.Property(f => f.Nome)
                    .HasColumnName("Nome")
                    .HasMaxLength(80)
                    .IsRequired();

            entidade.Property(f => f.PaisOrigem)
                    .HasColumnName("PaisOrigem")
                    .HasMaxLength(60)
                    .IsRequired();

            entidade.Property(f => f.AnoFundacao)
                    .HasColumnName("AnoFundacao");

            // Nao pode ter dois fabricantes com o mesmo nome.
            entidade.HasIndex(f => f.Nome)
                    .IsUnique()
                    .HasDatabaseName("UQ_Fabricante_Nome");
        });
    }

    // ==================================================================
    // CATEGORIA
    // ==================================================================
    private static void ConfigurarCategoria(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(entidade =>
        {
            entidade.ToTable("Categoria", tabela =>
                tabela.HasCheckConstraint(
                    "CK_Categoria_ValorDiariaSugerido",
                    "[ValorDiariaSugerido] >= 0"));

            entidade.HasKey(c => c.Id).HasName("PK_Categoria");
            entidade.Property(c => c.Id).ValueGeneratedOnAdd();

            entidade.Property(c => c.Nome)
                    .HasMaxLength(60)
                    .IsRequired();

            entidade.Property(c => c.Descricao)
                    .HasMaxLength(250);

            // decimal(10,2) para nao perder centavos.
            entidade.Property(c => c.ValorDiariaSugerido)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

            entidade.HasIndex(c => c.Nome)
                    .IsUnique()
                    .HasDatabaseName("UQ_Categoria_Nome");
        });
    }

    // ==================================================================
    // FILIAL
    // ==================================================================
    private static void ConfigurarFilial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Filial>(entidade =>
        {
            entidade.ToTable("Filial");

            entidade.HasKey(f => f.Id).HasName("PK_Filial");
            entidade.Property(f => f.Id).ValueGeneratedOnAdd();

            entidade.Property(f => f.Nome)
                    .HasMaxLength(100)
                    .IsRequired();

            entidade.Property(f => f.Cidade)
                    .HasMaxLength(80)
                    .IsRequired();

            // Sigla do estado, sempre 2 letras.
            entidade.Property(f => f.Estado)
                    .HasColumnType("char(2)")
                    .IsRequired();

            entidade.Property(f => f.Endereco).HasMaxLength(200);
            entidade.Property(f => f.Telefone).HasMaxLength(20);

            entidade.HasIndex(f => new { f.Cidade, f.Nome })
                    .HasDatabaseName("IX_Filial_Cidade_Nome");
        });
    }

    // ==================================================================
    // VEICULO
    // ==================================================================
    private static void ConfigurarVeiculo(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Veiculo>(entidade =>
        {
            entidade.ToTable("Veiculo", tabela =>
            {
                tabela.HasCheckConstraint(
                    "CK_Veiculo_AnoFabricacao", "[AnoFabricacao] >= 1900");
                tabela.HasCheckConstraint(
                    "CK_Veiculo_Quilometragem", "[Quilometragem] >= 0");
                tabela.HasCheckConstraint(
                    "CK_Veiculo_ValorDiaria", "[ValorDiaria] > 0");
            });

            entidade.HasKey(v => v.Id).HasName("PK_Veiculo");
            entidade.Property(v => v.Id).ValueGeneratedOnAdd();

            entidade.Property(v => v.Modelo)
                    .HasMaxLength(80)
                    .IsRequired();

            entidade.Property(v => v.Placa)
                    .HasColumnType("varchar(8)")   // aceita "ABC1D23" e "ABC-1234"
                    .IsRequired();

            entidade.Property(v => v.AnoFabricacao).IsRequired();
            entidade.Property(v => v.Quilometragem).IsRequired();
            entidade.Property(v => v.Cor).HasMaxLength(30);
            entidade.Property(v => v.NumeroPassageiros).IsRequired();

            entidade.Property(v => v.ValorDiaria)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

            // Sem HasDefaultValue aqui: com bool o EF acaba ignorando o "false"
            // no INSERT e o banco grava "true" errado.
            entidade.Property(v => v.Disponivel).IsRequired();

            // Grava o enum como texto ("Flex"), fica mais facil de ler.
            entidade.Property(v => v.Combustivel)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();

            entidade.HasIndex(v => v.Placa)
                    .IsUnique()
                    .HasDatabaseName("UQ_Veiculo_Placa");

            // Veiculo (N) -> Fabricante (1).
            // Restrict: nao deixa apagar marca que ainda tem carro.
            entidade.HasOne(v => v.Fabricante)
                    .WithMany(f => f.Veiculos)
                    .HasForeignKey(v => v.FabricanteId)
                    .HasConstraintName("FK_Veiculo_Fabricante")
                    .OnDelete(DeleteBehavior.Restrict);

            // Veiculo (N) -> Categoria (1).
            entidade.HasOne(v => v.Categoria)
                    .WithMany(c => c.Veiculos)
                    .HasForeignKey(v => v.CategoriaId)
                    .HasConstraintName("FK_Veiculo_Categoria")
                    .OnDelete(DeleteBehavior.Restrict);

            // Veiculo (N) -> Filial (0..1). FK opcional.
            // Se a filial fechar, o carro fica sem filial em vez de sumir.
            entidade.HasOne(v => v.Filial)
                    .WithMany(f => f.Veiculos)
                    .HasForeignKey(v => v.FilialId)
                    .HasConstraintName("FK_Veiculo_Filial")
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);
        });
    }

    // ==================================================================
    // CLIENTE
    // ==================================================================
    private static void ConfigurarCliente(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entidade =>
        {
            entidade.ToTable("Cliente", tabela =>
                tabela.HasCheckConstraint(
                    "CK_Cliente_Cpf_11Digitos", "LEN([Cpf]) = 11"));

            entidade.HasKey(c => c.Id).HasName("PK_Cliente");
            entidade.Property(c => c.Id).ValueGeneratedOnAdd();

            entidade.Property(c => c.Nome)
                    .HasMaxLength(120)
                    .IsRequired();

            // CPF so com numeros.
            entidade.Property(c => c.Cpf)
                    .HasColumnType("char(11)")
                    .IsRequired();

            entidade.Property(c => c.Email)
                    .HasMaxLength(150)
                    .IsRequired();

            entidade.Property(c => c.Telefone).HasMaxLength(20);
            entidade.Property(c => c.NumeroCnh).HasColumnType("varchar(11)");

            entidade.Property(c => c.DataNascimento).HasColumnType("date");

            entidade.Property(c => c.DataCadastro)
                    .HasColumnType("datetime2")
                    .HasDefaultValueSql("GETDATE()")
                    .IsRequired();

            entidade.HasIndex(c => c.Cpf)
                    .IsUnique()
                    .HasDatabaseName("UQ_Cliente_Cpf");

            entidade.HasIndex(c => c.Email)
                    .IsUnique()
                    .HasDatabaseName("UQ_Cliente_Email");
        });
    }

    // ==================================================================
    // ALUGUEL
    // ==================================================================
    private static void ConfigurarAluguel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Aluguel>(entidade =>
        {
            entidade.ToTable("Aluguel", tabela =>
            {
                // A devolucao nao pode ser antes da retirada.
                tabela.HasCheckConstraint(
                    "CK_Aluguel_Periodo",
                    "[DataPrevistaDevolucao] >= [DataRetirada]");

                // Km final nunca menor que o inicial.
                tabela.HasCheckConstraint(
                    "CK_Aluguel_Quilometragem",
                    "[QuilometragemFinal] IS NULL OR [QuilometragemFinal] >= [QuilometragemInicial]");

                tabela.HasCheckConstraint(
                    "CK_Aluguel_ValorDiaria", "[ValorDiaria] > 0");

                tabela.HasCheckConstraint(
                    "CK_Aluguel_ValorTotal", "[ValorTotal] >= 0");
            });

            entidade.HasKey(a => a.Id).HasName("PK_Aluguel");
            entidade.Property(a => a.Id).ValueGeneratedOnAdd();

            // ---- Periodo ----
            entidade.Property(a => a.DataRetirada)
                    .HasColumnType("datetime2")
                    .IsRequired();

            entidade.Property(a => a.DataPrevistaDevolucao)
                    .HasColumnType("datetime2")
                    .IsRequired();

            // Fica nula ate o carro voltar.
            entidade.Property(a => a.DataDevolucao)
                    .HasColumnType("datetime2");

            // ---- Quilometragem ----
            entidade.Property(a => a.QuilometragemInicial).IsRequired();
            entidade.Property(a => a.QuilometragemFinal);

            // ---- Valores ----
            entidade.Property(a => a.ValorDiaria)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

            entidade.Property(a => a.QuantidadeDiarias).IsRequired();

            entidade.Property(a => a.ValorTotal)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

            entidade.Property(a => a.ValorMulta)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

            entidade.Property(a => a.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();

            entidade.Property(a => a.Observacoes).HasMaxLength(500);

            // Conta calculada em C#, nao vira coluna.
            entidade.Ignore(a => a.KmRodados);

            // Aluguel (N) -> Cliente (1).
            // Restrict: cliente com historico de aluguel nao pode ser apagado.
            entidade.HasOne(a => a.Cliente)
                    .WithMany(c => c.Alugueis)
                    .HasForeignKey(a => a.ClienteId)
                    .HasConstraintName("FK_Aluguel_Cliente")
                    .OnDelete(DeleteBehavior.Restrict);

            // Aluguel (N) -> Veiculo (1).
            entidade.HasOne(a => a.Veiculo)
                    .WithMany(v => v.Alugueis)
                    .HasForeignKey(a => a.VeiculoId)
                    .HasConstraintName("FK_Aluguel_Veiculo")
                    .OnDelete(DeleteBehavior.Restrict);

            // Indices para as consultas ficarem mais rapidas.
            entidade.HasIndex(a => a.ClienteId).HasDatabaseName("IX_Aluguel_ClienteId");
            entidade.HasIndex(a => a.VeiculoId).HasDatabaseName("IX_Aluguel_VeiculoId");
            entidade.HasIndex(a => a.DataRetirada).HasDatabaseName("IX_Aluguel_DataRetirada");
        });
    }

    // ==================================================================
    // PAGAMENTO
    // ==================================================================
    private static void ConfigurarPagamento(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pagamento>(entidade =>
        {
            entidade.ToTable("Pagamento", tabela =>
                tabela.HasCheckConstraint("CK_Pagamento_Valor", "[Valor] > 0"));

            entidade.HasKey(p => p.Id).HasName("PK_Pagamento");
            entidade.Property(p => p.Id).ValueGeneratedOnAdd();

            entidade.Property(p => p.Valor)
                    .HasColumnType("decimal(10,2)")
                    .IsRequired();

            entidade.Property(p => p.FormaPagamento)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();

            entidade.Property(p => p.Status)
                    .HasConversion<string>()
                    .HasMaxLength(20)
                    .IsRequired();

            entidade.Property(p => p.DataPagamento)
                    .HasColumnType("datetime2")
                    .HasDefaultValueSql("GETDATE()")
                    .IsRequired();

            entidade.Property(p => p.CodigoTransacao).HasMaxLength(50);

            // Pagamento (N) -> Aluguel (1).
            // Cascade: apagou o aluguel, os pagamentos vao junto.
            entidade.HasOne(p => p.Aluguel)
                    .WithMany(a => a.Pagamentos)
                    .HasForeignKey(p => p.AluguelId)
                    .HasConstraintName("FK_Pagamento_Aluguel")
                    .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
