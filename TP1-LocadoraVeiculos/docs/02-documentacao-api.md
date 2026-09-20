# Etapa 3.2 — Documentação dos Endpoints

Base da URL em desenvolvimento: `https://localhost:7100`
Swagger UI: `https://localhost:7100/` (abre na raiz)

Todos os endpoints consomem e produzem `application/json`.

---

## Códigos de resposta usados no projeto

| Código | Quando acontece |
|--------|-----------------|
| `200 OK` | Consulta ou atualização de estado bem-sucedida, com corpo na resposta |
| `201 Created` | Recurso criado. O cabeçalho `Location` aponta para o novo recurso |
| `204 No Content` | `PUT` ou `DELETE` bem-sucedido, sem corpo na resposta |
| `400 Bad Request` | Falha de validação ou FK apontando para um registro inexistente |
| `404 Not Found` | O `{id}` informado não existe |
| `409 Conflict` | Violação de regra de negócio (duplicidade, período ocupado, exclusão bloqueada) |
| `500 Internal Server Error` | Erro inesperado — capturado pelo middleware global |

Erros sempre voltam no formato **ProblemDetails** (RFC 7807):

```json
{
  "title": "Um ou mais campos enviados sao invalidos.",
  "status": 400,
  "instance": "/api/clientes",
  "errors": {
    "Cpf": ["O campo Cpf nao contem um CPF valido."],
    "Email": ["E-mail em formato invalido."]
  }
}
```

---

## 1. Fabricantes — `/api/fabricantes`

| Método | Rota | Descrição | Respostas |
|--------|------|-----------|-----------|
| `GET` | `/api/fabricantes?nome=` | Lista fabricantes (filtro opcional por parte do nome) | `200` |
| `GET` | `/api/fabricantes/{id}` | Busca por Id | `200`, `404` |
| `POST` | `/api/fabricantes` | Cadastra | `201`, `400`, `409` |
| `PUT` | `/api/fabricantes/{id}` | Atualiza | `204`, `400`, `404`, `409` |
| `DELETE` | `/api/fabricantes/{id}` | Exclui | `204`, `404`, `409` |

**Corpo (POST/PUT):**
```json
{ "nome": "Fiat", "paisOrigem": "Italia", "anoFundacao": 1899 }
```
**Validações:** `nome` 2–80 caracteres, obrigatório e único; `paisOrigem` obrigatório (máx. 60);
`anoFundacao` entre 1800 e 2100.
**`409`** quando o nome já existe ou quando a exclusão é bloqueada por existirem veículos da marca.

---

## 2. Categorias — `/api/categorias`

| Método | Rota | Descrição | Respostas |
|--------|------|-----------|-----------|
| `GET` | `/api/categorias` | Lista todas | `200` |
| `GET` | `/api/categorias/{id}` | Busca por Id | `200`, `404` |
| `POST` | `/api/categorias` | Cadastra | `201`, `400`, `409` |
| `PUT` | `/api/categorias/{id}` | Atualiza | `204`, `404`, `409` |
| `DELETE` | `/api/categorias/{id}` | Exclui | `204`, `404`, `409` |

```json
{ "nome": "SUV", "descricao": "Utilitario esportivo", "valorDiariaSugerido": 260.00 }
```

---

## 3. Filiais — `/api/filiais`

| Método | Rota | Descrição | Respostas |
|--------|------|-----------|-----------|
| `GET` | `/api/filiais?cidade=&estado=` | Lista com filtros opcionais | `200` |
| `GET` | `/api/filiais/{id}` | Busca por Id | `200`, `404` |
| `POST` | `/api/filiais` | Cadastra | `201`, `400` |
| `PUT` | `/api/filiais/{id}` | Atualiza | `204`, `404` |
| `DELETE` | `/api/filiais/{id}` | Exclui (veículos ficam sem filial) | `204`, `404` |

```json
{
  "nome": "Filial Savassi - BH",
  "cidade": "Belo Horizonte",
  "estado": "MG",
  "endereco": "Rua Pernambuco, 500",
  "telefone": "(31) 3333-4000"
}
```
**Validações:** `estado` deve ter exatamente 2 letras (gravado em maiúsculas).

---

## 4. Veículos — `/api/veiculos`

| Método | Rota | Descrição | Respostas |
|--------|------|-----------|-----------|
| `GET` | `/api/veiculos?modelo=&fabricanteId=&categoriaId=&filialId=&disponivel=` | Lista com filtros | `200` |
| `GET` | `/api/veiculos/{id}` | Busca por Id | `200`, `404` |
| `POST` | `/api/veiculos` | Cadastra | `201`, `400`, `409` |
| `PUT` | `/api/veiculos/{id}` | Atualiza | `204`, `400`, `404`, `409` |
| `DELETE` | `/api/veiculos/{id}` | Exclui | `204`, `404`, `409` |

```json
{
  "modelo": "Pulse Drive 1.3",
  "placa": "RKL3M45",
  "anoFabricacao": 2024,
  "quilometragem": 1200,
  "cor": "Azul",
  "combustivel": "Flex",
  "numeroPassageiros": 5,
  "valorDiaria": 220.00,
  "disponivel": true,
  "fabricanteId": 1,
  "categoriaId": 3,
  "filialId": 1
}
```

**Validações:**
- `placa` no padrão antigo (`ABC-1234`) ou Mercosul (`ABC1D23`); é normalizada para maiúsculas e sem traço
- `anoFabricacao` entre 1900 e 2100; `quilometragem` ≥ 0; `valorDiaria` > 0
- `combustivel`: `Gasolina`, `Etanol`, `Flex`, `Diesel`, `Eletrico` ou `Hibrido`
- `fabricanteId` e `categoriaId` precisam existir → senão `400`
- `filialId` é opcional (pode ser `null`)

**`409`:** placa já cadastrada, ou exclusão de veículo que possui aluguéis.

---

## 5. Clientes — `/api/clientes`

| Método | Rota | Descrição | Respostas |
|--------|------|-----------|-----------|
| `GET` | `/api/clientes?nome=&cpf=` | Lista com filtros | `200` |
| `GET` | `/api/clientes/{id}` | Busca por Id | `200`, `404` |
| `POST` | `/api/clientes` | Cadastra | `201`, `400`, `409` |
| `PUT` | `/api/clientes/{id}` | Atualiza | `204`, `400`, `404`, `409` |
| `DELETE` | `/api/clientes/{id}` | Exclui | `204`, `404`, `409` |

```json
{
  "nome": "Pedro Almeida",
  "cpf": "390.533.447-05",
  "email": "pedro.almeida@email.com",
  "telefone": "(31) 98888-1234",
  "dataNascimento": "1992-03-18",
  "numeroCnh": "09876543210"
}
```

**Validações:** CPF validado pelos **dígitos verificadores** (aceita com ou sem
pontuação, grava só os números); e-mail em formato válido; nome de 3 a 120 caracteres;
CNH com 11 dígitos quando informada.
**`409`:** CPF ou e-mail já cadastrados; exclusão de cliente com aluguéis.

---

## 6. Aluguéis — `/api/alugueis`

| Método | Rota | Descrição | Respostas |
|--------|------|-----------|-----------|
| `GET` | `/api/alugueis?clienteId=&veiculoId=&status=&dataInicio=&dataFim=` | Lista com filtros | `200` |
| `GET` | `/api/alugueis/{id}` | Busca por Id | `200`, `404` |
| `POST` | `/api/alugueis` | Abre a reserva | `201`, `400`, `409` |
| `PUT` | `/api/alugueis/{id}` | Altera período/observações | `204`, `400`, `404`, `409` |
| `PUT` | `/api/alugueis/{id}/retirada` | Confirma a retirada | `200`, `404`, `409` |
| `PUT` | `/api/alugueis/{id}/devolucao` | Registra a devolução | `200`, `400`, `404`, `409` |
| `PUT` | `/api/alugueis/{id}/cancelamento` | Cancela | `200`, `404`, `409` |
| `DELETE` | `/api/alugueis/{id}` | Exclui (pagamentos vão junto) | `204`, `404`, `409` |

### Ciclo de vida do aluguel

```
POST /api/alugueis          →  Reservado
PUT  /{id}/retirada         →  EmAndamento   (veículo fica indisponível)
PUT  /{id}/devolucao        →  Finalizado    (veículo volta, odômetro atualizado)
PUT  /{id}/cancelamento     →  Cancelado     (só antes de finalizar)
```

**POST — corpo:**
```json
{
  "clienteId": 1,
  "veiculoId": 3,
  "dataRetirada": "2026-10-01T09:00:00",
  "dataPrevistaDevolucao": "2026-10-05T09:00:00",
  "valorDiaria": 150.00,
  "observacoes": "Cliente solicitou cadeirinha infantil"
}
```
- `valorDiaria` é **opcional**: se omitido, usa o valor cadastrado no veículo
- `quilometragemInicial` **não é enviada** — é copiada do odômetro do veículo
- `quantidadeDiarias` = dias entre as datas, arredondado para cima, mínimo 1
- `valorTotal` = `valorDiaria × quantidadeDiarias`

**Regras que geram `409`:**
- O veículo já tem aluguel `Reservado` ou `EmAndamento` com período sobreposto
- Tentar alterar/cancelar um aluguel já `Finalizado` ou `Cancelado`
- Registrar retirada de um aluguel que não está `Reservado`
- Excluir um aluguel `EmAndamento`

**PUT `/devolucao` — corpo:**
```json
{
  "dataDevolucao": "2026-10-06T18:30:00",
  "quilometragemFinal": 15900,
  "observacoes": "Devolvido com arranhão no para-choque"
}
```

Efeitos da devolução:
1. `quilometragemFinal` é gravada e o **odômetro do veículo é atualizado**
2. O veículo volta a ficar `disponivel = true`
3. Se houve atraso, cobra-se **por dia de atraso: a diária + 20% de multa**
   `valorMulta = diasDeAtraso × valorDiaria × 1,20`
4. `valorTotal = (valorDiaria × quantidadeDiarias) + valorMulta`
5. Status vira `Finalizado`

**`400`:** `quilometragemFinal` menor que a inicial, ou `dataDevolucao` anterior à retirada.

---

## 7. Pagamentos — `/api/pagamentos`

| Método | Rota | Descrição | Respostas |
|--------|------|-----------|-----------|
| `GET` | `/api/pagamentos?aluguelId=&status=` | Lista com filtros | `200` |
| `GET` | `/api/pagamentos/{id}` | Busca por Id | `200`, `404` |
| `POST` | `/api/pagamentos` | Registra | `201`, `400` |
| `PUT` | `/api/pagamentos/{id}` | Atualiza (ex.: aprovar) | `204`, `400`, `404` |
| `DELETE` | `/api/pagamentos/{id}` | Exclui | `204`, `404` |

```json
{
  "aluguelId": 1,
  "valor": 600.00,
  "formaPagamento": "Pix",
  "status": "Aprovado",
  "codigoTransacao": "PIX-000123"
}
```
- `formaPagamento`: `Dinheiro`, `Pix`, `CartaoDebito`, `CartaoCredito`, `Boleto`
- `status`: `Pendente`, `Aprovado`, `Recusado`, `Estornado`

---

## 8. Consultas com JOIN — `/api/consultas`

> Requisito 2.5: 5 filtros usando pelo menos **dois tipos de join** diferentes.
> Foram implementados **6 filtros**, com **INNER JOIN**, **LEFT JOIN** e **GROUP BY**.

### 8.1 `GET /api/consultas/veiculos-disponiveis`
**JOINs:** `INNER JOIN` com Fabricante e Categoria + `LEFT JOIN` com Filial.
Como a filial é opcional, o `LEFT JOIN` garante que veículos sem filial também apareçam.

| Parâmetro | Tipo | Descrição |
|-----------|------|-----------|
| `fabricanteId` | int? | Filtra por marca |
| `categoriaId` | int? | Filtra por categoria |
| `cidade` | string? | Parte do nome da cidade da filial |
| `valorDiariaMaximo` | decimal? | Teto de valor da diária |

Exemplo: `/api/consultas/veiculos-disponiveis?cidade=Belo&valorDiariaMaximo=200`

```json
[
  {
    "veiculoId": 2, "modelo": "Mobi Like", "placa": "RKB2C34",
    "anoFabricacao": 2022, "quilometragem": 32100, "valorDiaria": 110.00,
    "fabricante": "Fiat", "categoria": "Economico",
    "filial": "Filial Centro - BH", "cidade": "Belo Horizonte"
  }
]
```
**Respostas:** `200`

---

### 8.2 `GET /api/consultas/historico-cliente`
**JOIN:** `INNER JOIN` encadeado entre 4 tabelas — Aluguel → Cliente → Veículo → Fabricante.

| Parâmetro | Tipo | Descrição |
|-----------|------|-----------|
| `clienteId` | int? | Id do cliente |
| `cpf` | string? | CPF com ou sem pontuação |
| `status` | enum? | `Reservado`, `EmAndamento`, `Finalizado`, `Cancelado` |

Informe `clienteId` **ou** `cpf`.
Exemplo: `/api/consultas/historico-cliente?cpf=529.982.247-25`

**Respostas:** `200`, `400` (nenhum identificador informado), `404` (cliente não existe)

---

### 8.3 `GET /api/consultas/veiculos-nunca-alugados`
**JOIN:** `LEFT JOIN` entre Veículo e Aluguel, mantendo só as linhas em que o lado
direito ficou nulo — equivale a `LEFT JOIN Aluguel ... WHERE Aluguel.Id IS NULL`.
É a consulta que **só funciona com LEFT JOIN**: com `INNER JOIN` o resultado seria sempre vazio.

Sem parâmetros. **Respostas:** `200`

---

### 8.4 `GET /api/consultas/faturamento-por-cliente`
**JOIN:** `LEFT JOIN` entre Cliente e Aluguel + `GROUP BY` por cliente.
Clientes que nunca alugaram aparecem com total zerado.

| Parâmetro | Tipo | Descrição |
|-----------|------|-----------|
| `valorMinimo` | decimal? | Só clientes que gastaram pelo menos esse valor |

```json
[
  {
    "clienteId": 1, "cliente": "Maria Oliveira", "cpf": "52998224725",
    "email": "maria.oliveira@email.com",
    "quantidadeAlugueis": 2, "valorTotalGasto": 1250.00,
    "ultimoAluguel": "2026-08-31T00:00:00"
  }
]
```
**Respostas:** `200`

---

### 8.5 `GET /api/consultas/alugueis-atrasados`
**JOINs:** `INNER JOIN` com Cliente e Veículo + `LEFT JOIN` com Filial.
O atraso é calculado **no banco**, com `DATEDIFF` (via `EF.Functions.DateDiffDay`).

| Parâmetro | Tipo | Padrão | Descrição |
|-----------|------|--------|-----------|
| `diasMinimosDeAtraso` | int | `1` | Filtra atrasos a partir de N dias |

**Respostas:** `200`

---

### 8.6 `GET /api/consultas/faturamento-por-categoria`
**JOIN:** `INNER JOIN` encadeado Categoria → Veículo → Aluguel + `GROUP BY` por categoria,
considerando apenas aluguéis `Finalizado`.

```json
[
  {
    "categoriaId": 3, "categoria": "SUV",
    "quantidadeVeiculos": 3, "quantidadeAlugueis": 1,
    "valorFaturado": 600.00, "ticketMedio": 600.00
  }
]
```
**Respostas:** `200`

---

## Resumo dos tipos de JOIN por consulta

| Consulta | INNER JOIN | LEFT JOIN | GROUP BY |
|----------|:----------:|:---------:|:--------:|
| 8.1 veículos-disponiveis | ✅ | ✅ | — |
| 8.2 historico-cliente | ✅ | — | — |
| 8.3 veiculos-nunca-alugados | ✅ | ✅ | — |
| 8.4 faturamento-por-cliente | — | ✅ | ✅ |
| 8.5 alugueis-atrasados | ✅ | ✅ | — |
| 8.6 faturamento-por-categoria | ✅ | — | ✅ |
