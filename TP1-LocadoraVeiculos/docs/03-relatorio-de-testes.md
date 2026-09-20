# Etapa 3.3 — Relatório de Testes

> **Como usar este documento:** ele já traz o roteiro de todos os testes na ordem
> correta de execução. Rode cada chamada no Swagger (`https://localhost:7100/`),
> tire o print da tela mostrando **a requisição e a resposta**, e cole a imagem no
> lugar indicado. Ao final, exporte como PDF para entregar no Canvas.

**Aluno:** Lucas A. M. Ramos
**Data da execução:** ___ / ___ / ______
**Ambiente:** .NET 8 · SQL Server Express · Swagger UI

---

## Como tirar os prints

1. Rode `dotnet run --project src/LocadoraVeiculos.API`
2. Abra `https://localhost:7100/`
3. Para cada teste: clique em **Try it out** → preencha os parâmetros → **Execute**
4. Tire o print mostrando o **Request URL**, o **corpo enviado** e o **Response body** com o **Code**

---

## Bloco 1 — Leitura dos dados da carga inicial

| # | Método | Endpoint | Esperado |
|---|--------|----------|----------|
| 1.1 | GET | `/api/fabricantes` | `200` com 5 fabricantes |
| 1.2 | GET | `/api/categorias` | `200` com 5 categorias |
| 1.3 | GET | `/api/filiais` | `200` com 3 filiais |
| 1.4 | GET | `/api/veiculos` | `200` com 10 veículos |
| 1.5 | GET | `/api/clientes` | `200` com 5 clientes |
| 1.6 | GET | `/api/alugueis` | `200` com 7 aluguéis |
| 1.7 | GET | `/api/pagamentos` | `200` com 6 pagamentos |

> _Cole aqui os prints 1.1 a 1.7_

---

## Bloco 2 — CRUD completo (usando Fabricante como exemplo)

| # | Método | Endpoint | Corpo / Parâmetro | Esperado |
|---|--------|----------|-------------------|----------|
| 2.1 | POST | `/api/fabricantes` | `{"nome":"Honda","paisOrigem":"Japao","anoFundacao":1948}` | `201 Created` |
| 2.2 | GET | `/api/fabricantes/{id}` | Id retornado em 2.1 | `200` com os dados |
| 2.3 | PUT | `/api/fabricantes/{id}` | `{"nome":"Honda Motor","paisOrigem":"Japao","anoFundacao":1948}` | `204 No Content` |
| 2.4 | GET | `/api/fabricantes/{id}` | — | `200` com o nome atualizado |
| 2.5 | DELETE | `/api/fabricantes/{id}` | — | `204 No Content` |
| 2.6 | GET | `/api/fabricantes/{id}` | — | `404 Not Found` |

> _Cole aqui os prints 2.1 a 2.6_

---

## Bloco 3 — CRUD de Cliente e Veículo

| # | Método | Endpoint | Corpo | Esperado |
|---|--------|----------|-------|----------|
| 3.1 | POST | `/api/clientes` | `{"nome":"Pedro Almeida","cpf":"390.533.447-05","email":"pedro@email.com","telefone":"(31) 98888-1234"}` | `201` — repare que o CPF é gravado só com dígitos |
| 3.2 | POST | `/api/veiculos` | `{"modelo":"Pulse Drive","placa":"RKL3M45","anoFabricacao":2024,"quilometragem":1200,"cor":"Azul","combustivel":"Flex","numeroPassageiros":5,"valorDiaria":220,"disponivel":true,"fabricanteId":1,"categoriaId":3,"filialId":1}` | `201` |
| 3.3 | GET | `/api/veiculos?disponivel=true&categoriaId=3` | — | `200` com os SUVs disponíveis |
| 3.4 | PUT | `/api/veiculos/{id}` | Mesmo corpo com `"valorDiaria": 240` | `204` |

> _Cole aqui os prints 3.1 a 3.4_

---

## Bloco 4 — Validações e tratamento de erros (requisito 2.4)

**Este bloco é o que comprova que a API valida a entrada.** Todos devem falhar.

| # | Método | Endpoint | Corpo inválido | Esperado |
|---|--------|----------|----------------|----------|
| 4.1 | POST | `/api/clientes` | `{"nome":"Teste","cpf":"111.111.111-11","email":"teste@email.com"}` | `400` — CPF com dígitos verificadores inválidos |
| 4.2 | POST | `/api/clientes` | `{"nome":"Teste","cpf":"529.982.247-25","email":"nao-e-email"}` | `400` — e-mail fora do formato |
| 4.3 | POST | `/api/clientes` | CPF `529.982.247-25` (já existe na carga inicial) | `409` — CPF duplicado |
| 4.4 | POST | `/api/veiculos` | `{"modelo":"X","placa":"12345","anoFabricacao":2024,"quilometragem":0,"valorDiaria":100,"fabricanteId":1,"categoriaId":1}` | `400` — placa fora do padrão |
| 4.5 | POST | `/api/veiculos` | Corpo válido, mas `"fabricanteId": 999` | `400` — FK inexistente |
| 4.6 | POST | `/api/veiculos` | Placa `RKA1B23` (já cadastrada) | `409` — placa duplicada |
| 4.7 | GET | `/api/clientes/9999` | — | `404` |
| 4.8 | DELETE | `/api/fabricantes/1` | — | `409` — a Fiat tem veículos vinculados |
| 4.9 | POST | `/api/alugueis` | `dataPrevistaDevolucao` **anterior** à `dataRetirada` | `400` — validação cruzada entre campos |

> _Cole aqui os prints 4.1 a 4.9_

---

## Bloco 5 — Ciclo de vida completo de um aluguel

Execute na ordem. Anote o `id` retornado em 5.1 e use nos passos seguintes.

| # | Método | Endpoint | Corpo | Esperado |
|---|--------|----------|-------|----------|
| 5.1 | POST | `/api/alugueis` | `{"clienteId":1,"veiculoId":1,"dataRetirada":"2026-12-01T09:00:00","dataPrevistaDevolucao":"2026-12-05T09:00:00"}` | `201` — status `Reservado`, `valorTotal` = diária × 4 |
| 5.2 | POST | `/api/alugueis` | Mesmo veículo e período sobreposto | `409` — veículo já comprometido |
| 5.3 | PUT | `/api/alugueis/{id}/retirada` | — | `200` — status vira `EmAndamento` |
| 5.4 | GET | `/api/veiculos/1` | — | `200` — `disponivel: false` |
| 5.5 | PUT | `/api/alugueis/{id}/devolucao` | `{"dataDevolucao":"2026-12-07T09:00:00","quilometragemFinal":16500}` | `200` — status `Finalizado`, com **multa de 2 dias de atraso** |
| 5.6 | GET | `/api/veiculos/1` | — | `200` — `disponivel: true` e **quilometragem atualizada para 16500** |
| 5.7 | PUT | `/api/alugueis/{id}/devolucao` | Mesmo corpo | `409` — já finalizado |
| 5.8 | POST | `/api/pagamentos` | `{"aluguelId":{id},"valor":600,"formaPagamento":"Pix","status":"Aprovado"}` | `201` |

**Confira o cálculo em 5.5:**
diária = R$ 130,00 · 4 diárias = R$ 520,00 · atraso de 2 dias
→ multa = 2 × 130,00 × 1,20 = **R$ 312,00** → total = **R$ 832,00**

> _Cole aqui os prints 5.1 a 5.8_

---

## Bloco 6 — Os 6 filtros com JOIN (requisito 2.5)

| # | Método | Endpoint | Tipo de JOIN | Esperado |
|---|--------|----------|--------------|----------|
| 6.1 | GET | `/api/consultas/veiculos-disponiveis` | INNER + LEFT | `200` com marca, categoria e filial |
| 6.2 | GET | `/api/consultas/veiculos-disponiveis?cidade=Belo&valorDiariaMaximo=200` | INNER + LEFT | `200` filtrado |
| 6.3 | GET | `/api/consultas/historico-cliente?cpf=529.982.247-25` | INNER encadeado (4 tabelas) | `200` com o histórico da Maria |
| 6.4 | GET | `/api/consultas/historico-cliente` (sem parâmetro) | — | `400` |
| 6.5 | GET | `/api/consultas/veiculos-nunca-alugados` | LEFT + `IS NULL` | `200` — o Renegade Sport aparece |
| 6.6 | GET | `/api/consultas/faturamento-por-cliente` | LEFT + GROUP BY | `200` — a Beatriz aparece com total `0` |
| 6.7 | GET | `/api/consultas/faturamento-por-cliente?valorMinimo=500` | LEFT + GROUP BY | `200` filtrado |
| 6.8 | GET | `/api/consultas/alugueis-atrasados` | INNER + LEFT + DATEDIFF | `200` — o aluguel do João aparece atrasado |
| 6.9 | GET | `/api/consultas/faturamento-por-categoria` | INNER + GROUP BY | `200` com ticket médio |

> _Cole aqui os prints 6.1 a 6.9_

---

## Conclusão

Total de casos executados: **____**
Casos aprovados: **____**
Casos com divergência: **____**

**Observações:**

_______________________________________________________________________

_______________________________________________________________________
