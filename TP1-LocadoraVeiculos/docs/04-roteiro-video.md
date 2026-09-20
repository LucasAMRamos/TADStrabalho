# Etapa 4 — Roteiro do Vídeo (Pitch)

**Duração alvo:** 6 a 12 minutos
**Onde publicar:** YouTube (pode ser "não listado") — cole o link no relatório acadêmico

> Grave a tela com o Swagger aberto e o SQL Server Management Studio em outra aba.
> Fale com calma: a banca precisa entender o raciocínio, não só ver telas passando.

---

## Estrutura sugerida

### 1. Abertura — 30 segundos
- Seu nome, disciplina e o tema: sistema de locadora de veículos
- Tecnologias em uma frase: "C# com .NET 8, Entity Framework Core, SQL Server Express e Swagger"

### 2. O problema e o modelo de dados — 2 minutos
Mostre o diagrama do `README.md` e explique:
- As 7 entidades e por que cada uma existe
- **Fale das chaves:** toda PK é `Id` identity; as FKs ligam Veículo→Fabricante,
  Veículo→Categoria, Aluguel→Cliente, Aluguel→Veículo, Pagamento→Aluguel
- **Destaque a decisão de projeto:** três comportamentos de exclusão diferentes
  (Restrict para preservar histórico, SetNull para filial, Cascade para pagamento)

### 3. A `ApplicationContext` — 1 minuto e 30
Abra `Data/ApplicationContext.cs` e mostre um bloco, por exemplo o do `Veiculo`:
- `HasKey` → chave primária
- `HasIndex().IsUnique()` → a placa não se repete
- `HasOne().WithMany().HasForeignKey()` → a chave estrangeira
- `HasCheckConstraint` → quilometragem não pode ser negativa

Depois mostre o banco criado no SSMS, com as tabelas e as constraints — é a prova
de que o modelo virou esquema relacional de verdade.

### 4. Demonstração do CRUD — 2 minutos
No Swagger, faça o ciclo completo com **Cliente**:
1. `POST /api/clientes` → `201`
2. `GET /api/clientes/{id}` → mostre que o CPF foi gravado só com números
3. `PUT` → `204`
4. `DELETE` → `204`

### 5. Validação e tratamento de erros — 1 minuto e 30
**Esta parte impressiona.** Mostre três falhas propositais:
- CPF `111.111.111-11` → `400` com a mensagem do validador
- CPF duplicado → `409`
- `DELETE` em um fabricante que tem veículos → `409` explicando a regra

### 6. Regra de negócio: o ciclo do aluguel — 2 minutos
1. `POST /api/alugueis` → status `Reservado`
2. Tente reservar o **mesmo veículo no mesmo período** → `409`
3. `PUT /{id}/retirada` → `EmAndamento`, e mostre o veículo ficando indisponível
4. `PUT /{id}/devolucao` **com atraso** → mostre a multa calculada e o odômetro
   do veículo sendo atualizado

### 7. As consultas com JOIN — 2 minutos
Escolha as três mais visuais e explique **por que** cada JOIN é aquele:
- `veiculos-nunca-alugados` → "com INNER JOIN esse resultado seria sempre vazio;
  é o LEFT JOIN que traz o carro que nunca foi alugado"
- `faturamento-por-cliente` → "LEFT JOIN com GROUP BY: a Beatriz aparece com
  total zero porque nunca alugou"
- `alugueis-atrasados` → "INNER JOIN com Cliente e Veículo, LEFT JOIN com Filial,
  e o cálculo do atraso acontece no banco, com DATEDIFF"

### 8. Encerramento — 30 segundos
- Recapitule: 7 entidades, CRUD completo, 6 consultas com JOIN, validação e Swagger
- Agradeça

---

## Checklist antes de gravar

- [ ] Banco recriado do zero, com a carga inicial (para os números baterem)
- [ ] Swagger aberto e testado — nenhuma chamada dando erro inesperado
- [ ] SSMS aberto na aba ao lado, já conectado
- [ ] Áudio testado
- [ ] Notificações do sistema desligadas
- [ ] Cronômetro: ensaie uma vez para caber nos 12 minutos

## Checklist depois de gravar

- [ ] Vídeo entre 6 e 12 minutos
- [ ] Publicado no YouTube (público ou "não listado" — **nunca privado**)
- [ ] Link testado em uma aba anônima
- [ ] Link colado no relatório acadêmico
