# Etapa 1 — Modelagem do Banco de Dados

Este documento descreve o modelo conceitual, a tradução dele para o esquema
relacional via Entity Framework Core e as restrições de integridade aplicadas.

---

## 1. Regras de negócio que o modelo precisa atender

O enunciado exige:

| # | Regra | Como o modelo atende |
|---|-------|----------------------|
| 1 | Todo veículo pertence a um fabricante | `Veiculo.FabricanteId` — FK **obrigatória** (`int`, não anulável) |
| 2 | Todo veículo registra modelo, ano de fabricação e quilometragem | Colunas `Modelo`, `AnoFabricacao`, `Quilometragem`, todas `NOT NULL` |
| 3 | Cliente tem no mínimo nome, CPF e e-mail | `Nome`, `Cpf`, `Email` — `NOT NULL`, com CPF e e-mail **únicos** |
| 4 | Aluguel vincula um cliente e um veículo em um período | FKs `ClienteId` e `VeiculoId` + `DataRetirada` e `DataPrevistaDevolucao` |
| 5 | Registrar a devolução | `DataDevolucao` (anulável — nula enquanto o carro não volta) |
| 6 | Quilometragem inicial e final do aluguel | `QuilometragemInicial` (`NOT NULL`) e `QuilometragemFinal` (anulável) |
| 7 | Valor da diária e valor total | `ValorDiaria` e `ValorTotal`, ambos `decimal(10,2)` |
| 8 | Mínimo de 5 entidades | Foram modeladas **7** |

---

## 2. As 7 entidades

### 2.1 Fabricante
A marca do veículo (Fiat, Volkswagen…). Uma marca tem muitos veículos.

| Coluna | Tipo | Restrição |
|--------|------|-----------|
| `Id` | `int` | **PK**, identity |
| `Nome` | `nvarchar(80)` | `NOT NULL`, **UNIQUE** (`UQ_Fabricante_Nome`) |
| `PaisOrigem` | `nvarchar(60)` | `NOT NULL` |
| `AnoFundacao` | `int` | anulável |

### 2.2 Categoria
Grupo tarifário do veículo (Econômico, SUV, Premium…). É a entidade adicional
que o item 1.5 pede, e dá sentido de negócio ao cálculo da diária.

| Coluna | Tipo | Restrição |
|--------|------|-----------|
| `Id` | `int` | **PK**, identity |
| `Nome` | `nvarchar(60)` | `NOT NULL`, **UNIQUE** |
| `Descricao` | `nvarchar(250)` | anulável |
| `ValorDiariaSugerido` | `decimal(10,2)` | `NOT NULL`, `CHECK >= 0` |

### 2.3 Filial
Loja onde o veículo fica estacionado. Permite consultas por cidade e é o lado
opcional de um relacionamento — por isso é ela que motiva o uso de `LEFT JOIN`.

| Coluna | Tipo | Restrição |
|--------|------|-----------|
| `Id` | `int` | **PK**, identity |
| `Nome` | `nvarchar(100)` | `NOT NULL` |
| `Cidade` | `nvarchar(80)` | `NOT NULL` |
| `Estado` | `char(2)` | `NOT NULL` |
| `Endereco`, `Telefone` | `nvarchar` | anuláveis |

### 2.4 Veículo

| Coluna | Tipo | Restrição |
|--------|------|-----------|
| `Id` | `int` | **PK**, identity |
| `Modelo` | `nvarchar(80)` | `NOT NULL` |
| `Placa` | `varchar(8)` | `NOT NULL`, **UNIQUE** (`UQ_Veiculo_Placa`) |
| `AnoFabricacao` | `int` | `NOT NULL`, `CHECK >= 1900` |
| `Quilometragem` | `int` | `NOT NULL`, `CHECK >= 0` |
| `Cor` | `nvarchar(30)` | anulável |
| `Combustivel` | `nvarchar(20)` | `NOT NULL` — enum gravado como texto |
| `NumeroPassageiros` | `int` | `NOT NULL` |
| `ValorDiaria` | `decimal(10,2)` | `NOT NULL`, `CHECK > 0` |
| `Disponivel` | `bit` | `NOT NULL` |
| `FabricanteId` | `int` | **FK** → `Fabricante.Id`, `NOT NULL`, `ON DELETE NO ACTION` |
| `CategoriaId` | `int` | **FK** → `Categoria.Id`, `NOT NULL`, `ON DELETE NO ACTION` |
| `FilialId` | `int` | **FK** → `Filial.Id`, **anulável**, `ON DELETE SET NULL` |

### 2.5 Cliente

| Coluna | Tipo | Restrição |
|--------|------|-----------|
| `Id` | `int` | **PK**, identity |
| `Nome` | `nvarchar(120)` | `NOT NULL` |
| `Cpf` | `char(11)` | `NOT NULL`, **UNIQUE**, `CHECK LEN(Cpf) = 11` |
| `Email` | `nvarchar(150)` | `NOT NULL`, **UNIQUE** |
| `Telefone` | `nvarchar(20)` | anulável |
| `DataNascimento` | `date` | anulável |
| `NumeroCnh` | `varchar(11)` | anulável |
| `DataCadastro` | `datetime2` | `NOT NULL`, `DEFAULT GETDATE()` |

> O CPF é gravado **somente com dígitos**. A API remove pontos e traços antes de
> salvar e valida os dígitos verificadores (`Validacoes/CpfAttribute.cs`).

### 2.6 Aluguel

| Coluna | Tipo | Restrição |
|--------|------|-----------|
| `Id` | `int` | **PK**, identity |
| `ClienteId` | `int` | **FK** → `Cliente.Id`, `NOT NULL`, `ON DELETE NO ACTION` |
| `VeiculoId` | `int` | **FK** → `Veiculo.Id`, `NOT NULL`, `ON DELETE NO ACTION` |
| `DataRetirada` | `datetime2` | `NOT NULL` |
| `DataPrevistaDevolucao` | `datetime2` | `NOT NULL` |
| `DataDevolucao` | `datetime2` | anulável |
| `QuilometragemInicial` | `int` | `NOT NULL` |
| `QuilometragemFinal` | `int` | anulável |
| `ValorDiaria` | `decimal(10,2)` | `NOT NULL`, `CHECK > 0` |
| `QuantidadeDiarias` | `int` | `NOT NULL` |
| `ValorMulta` | `decimal(10,2)` | `NOT NULL` |
| `ValorTotal` | `decimal(10,2)` | `NOT NULL`, `CHECK >= 0` |
| `Status` | `nvarchar(20)` | `NOT NULL` — Reservado / EmAndamento / Finalizado / Cancelado |
| `Observacoes` | `nvarchar(500)` | anulável |

**CHECK constraints da tabela:**
- `CK_Aluguel_Periodo`: `DataPrevistaDevolucao >= DataRetirada`
- `CK_Aluguel_Quilometragem`: `QuilometragemFinal IS NULL OR QuilometragemFinal >= QuilometragemInicial`

**Índices**: `ClienteId`, `VeiculoId` e `DataRetirada` — aceleram as consultas da Etapa 2.

### 2.7 Pagamento

| Coluna | Tipo | Restrição |
|--------|------|-----------|
| `Id` | `int` | **PK**, identity |
| `AluguelId` | `int` | **FK** → `Aluguel.Id`, `NOT NULL`, **`ON DELETE CASCADE`** |
| `Valor` | `decimal(10,2)` | `NOT NULL`, `CHECK > 0` |
| `FormaPagamento` | `nvarchar(20)` | `NOT NULL` |
| `Status` | `nvarchar(20)` | `NOT NULL` |
| `DataPagamento` | `datetime2` | `NOT NULL`, `DEFAULT GETDATE()` |
| `CodigoTransacao` | `nvarchar(50)` | anulável |

---

## 3. Resumo das chaves

### Chaves primárias (item 1.3)
Todas as 7 tabelas usam `Id` `int` **identity**, nomeadas explicitamente
(`PK_Fabricante`, `PK_Categoria`, `PK_Filial`, `PK_Veiculo`, `PK_Cliente`,
`PK_Aluguel`, `PK_Pagamento`).

### Chaves estrangeiras (item 1.3)

| FK | De → Para | Obrigatória? | Ao excluir o "pai" |
|----|-----------|--------------|--------------------|
| `FK_Veiculo_Fabricante` | `Veiculo.FabricanteId` → `Fabricante.Id` | sim | **Restrict** — bloqueia |
| `FK_Veiculo_Categoria` | `Veiculo.CategoriaId` → `Categoria.Id` | sim | **Restrict** — bloqueia |
| `FK_Veiculo_Filial` | `Veiculo.FilialId` → `Filial.Id` | não | **SetNull** — o veículo fica sem filial |
| `FK_Aluguel_Cliente` | `Aluguel.ClienteId` → `Cliente.Id` | sim | **Restrict** — preserva o histórico |
| `FK_Aluguel_Veiculo` | `Aluguel.VeiculoId` → `Veiculo.Id` | sim | **Restrict** — preserva o histórico |
| `FK_Pagamento_Aluguel` | `Pagamento.AluguelId` → `Aluguel.Id` | sim | **Cascade** — pagamento não existe sozinho |

**Por que três comportamentos diferentes?**
- **Restrict** onde o registro filho é histórico contábil: apagar um cliente que
  já alugou destruiria o registro da locação.
- **SetNull** onde o vínculo é circunstancial: fechar uma filial não deve apagar
  os carros dela.
- **Cascade** onde o filho é dependente: um pagamento só faz sentido
  dentro de um aluguel.

---

## 4. Configuração da `ApplicationContext` (item 1.2 / 1.4)

Arquivo: `src/LocadoraVeiculos.API/Data/ApplicationContext.cs`

A classe herda de `DbContext` e faz três coisas:

**1) Declara os `DbSet`** — cada um vira uma tabela:

```csharp
public DbSet<Fabricante> Fabricantes => Set<Fabricante>();
public DbSet<Veiculo>    Veiculos    => Set<Veiculo>();
// ... e assim por diante para as 7 entidades
```

**2) Configura o mapeamento no `OnModelCreating`**, usando **Fluent API**
(um método `Configurar<Entidade>` por tabela, para o arquivo ficar legível):

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    ConfigurarFabricante(modelBuilder);
    ConfigurarCategoria(modelBuilder);
    // ...
}
```

**3) Descreve cada tabela.** Exemplo comentado, com a entidade `Veiculo`:

```csharp
entidade.ToTable("Veiculo", tabela =>
    tabela.HasCheckConstraint("CK_Veiculo_Quilometragem", "[Quilometragem] >= 0"));

entidade.HasKey(v => v.Id).HasName("PK_Veiculo");     // chave primária

entidade.Property(v => v.Placa)
        .HasColumnType("varchar(8)")
        .IsRequired();                                 // NOT NULL

entidade.HasIndex(v => v.Placa)
        .IsUnique()
        .HasDatabaseName("UQ_Veiculo_Placa");          // restrição de unicidade

entidade.HasOne(v => v.Fabricante)                     // chave estrangeira
        .WithMany(f => f.Veiculos)                     // 1 fabricante → N veículos
        .HasForeignKey(v => v.FabricanteId)
        .HasConstraintName("FK_Veiculo_Fabricante")
        .OnDelete(DeleteBehavior.Restrict);
```

### Por que Fluent API e não Data Annotations?

| Critério | Data Annotations (`[Required]` na entidade) | Fluent API (no `ApplicationContext`) |
|----------|-------------------------------------------|--------------------------------------|
| Onde fica | espalhado pelas classes de modelo | centralizado em um arquivo |
| O que consegue configurar | básico | tudo (CHECK, nomes de constraint, `ON DELETE`, precisão decimal) |
| Entidades | ficam "sujas" de atributos de banco | ficam limpas, só com os dados |

Neste projeto as **Data Annotations ficam nos DTOs** (validação do que o usuário
envia pela API) e o **Fluent API fica no `ApplicationContext`** (regras do banco).
São duas camadas de validação com responsabilidades diferentes.

### Enums gravados como texto

```csharp
entidade.Property(v => v.Combustivel)
        .HasConversion<string>()
        .HasMaxLength(20);
```

Sem isso, o SQL Server guardaria `3` em vez de `Flex`. Ler a tabela direto no
SSMS fica muito mais fácil com o texto.

---

## 5. Gerando o banco físico

```bash
dotnet ef migrations add CriacaoInicial --project src/LocadoraVeiculos.API
dotnet ef database update --project src/LocadoraVeiculos.API
```

O primeiro comando cria a pasta `Migrations/` com o C# que gera todo o esquema
(este é o artefato que comprova a tradução do modelo conceitual para o relacional).
O segundo executa esse script no SQL Server Express.

Para ver o SQL puro que será executado, sem aplicá-lo:

```bash
dotnet ef migrations script --project src/LocadoraVeiculos.API
```
