# Bairro em Dia

API para registrar, consultar e acompanhar problemas nos espaços públicos de um bairro, como iluminação apagada, lixo acumulado, buracos e calçadas danificadas.

Desenvolvido em C# para a Global Solution CitizenTech — Engenharia de Software, FIAP, 2026.

## Integrantes

| Nome              | RM     |
| ----------------- | ------ |
| Laura Souza       | 556320 |
| Vinicius Henrique | 556908 |
| Gustavo Pierre    | 558928 |
| Gabriel Belo      | 551669 |

## A proposta

O Bairro em Dia organiza relatos de moradores em um histórico compartilhado de acompanhamento. O cidadão registra um problema, outros moradores podem confirmar que também são afetados e uma associação de moradores representa o responsável pelo acompanhamento das ocorrências no cenário do protótipo.

A API permite registrar ocorrências em bairros de qualquer cidade do Brasil, identificando UF, cidade, bairro e localização. Chácara Klabin, em São Paulo/SP, é apenas um exemplo de demonstração. A UF é validada; nomes de cidades e bairros são textuais, sem validação de existência em um catálogo oficial nesta versão.

| Ficha da CitizenTech    | Descrição                                                                                                                             |
| ----------------------- | ------------------------------------------------------------------------------------------------------------------------------------- |
| Cidadão beneficiado     | Moradores que desejam registrar e acompanhar problemas nos espaços públicos do bairro.                                                |
| Problema                | Necessidade de organizar relatos locais e tornar visível seu acompanhamento coletivo.                                                 |
| Solução                 | Registro de ocorrências, confirmações de outros moradores, histórico de status e indicadores.                                         |
| Ecossistema proposto    | Moradores e associações de moradores; órgãos e prestadores responsáveis pela manutenção integram o contexto de encaminhamento futuro. |
| Tecnologias             | C#, ASP.NET Core Web API, Entity Framework Core, LINQ e SQLite.                                                                       |
| Indicadores disponíveis | Totais de ocorrências, abertas, encerradas, em status Registrada e distribuição por categoria e status.                               |
| Evolução prevista       | Aplicativo Expo com React Native e TypeScript consumindo esta API.                                                                    |
| ODS                     | ODS 11 — Cidades e comunidades sustentáveis.                                                                                          |

O projeto é um protótipo acadêmico com usuários e ocorrências fictícios. Os registros não são enviados à Prefeitura e não representam solicitações oficiais de serviços. Os status representam acompanhamento dentro do protótipo, sem comprovação externa de execução de manutenção.

## Funcionalidades

- Cadastro de contas por nome, e-mail e senha; login e logout com sessão revogável.
- Gestão de usuários restrita ao perfil Gestor.
- Cadastro e consulta detalhada de ocorrências.
- Listagem de ocorrências com filtros opcionais por UF, cidade, bairro, categoria e status.
- Edição dos dados de ocorrências abertas.
- Alteração de status com justificativa e histórico cronológico.
- Encerramento e reabertura de ocorrências.
- Confirmação por outros moradores, com bloqueio de confirmação própria e duplicada.
- Indicadores gerais ou filtrados por UF, cidade e bairro.
- Persistência em SQLite com migrations do EF Core.
- Documentação OpenAPI e interface interativa Scalar.
- Tratamento centralizado de exceções e testes automatizados.

## Categorias e status

| Valor | Categoria no JSON   |
| ----: | ------------------- |
|     1 | `Iluminacao`        |
|     2 | `LixoAcumulado`     |
|     3 | `Buraco`            |
|     4 | `CalcadaDanificada` |

| Valor | Status no JSON     |
| ----: | ------------------ |
|     1 | `Registrada`       |
|     2 | `EmAnalise`        |
|     3 | `EmAcompanhamento` |
|     4 | `Encerrada`        |

Nos corpos JSON, utilize os nomes dos enums. Nos filtros apresentados pelo Scalar, utilize os valores numéricos.

## Regras de negócio

- Toda ocorrência começa como `Registrada` e deve estar vinculada a um usuário existente.
- Título, descrição, cidade, UF, bairro, localização e categoria devem ser válidos. A UF é normalizada para letras maiúsculas e deve pertencer às 27 unidades federativas brasileiras.
- Limites: título de 120 caracteres, descrição de 2.000, cidade de 100, UF de 2, bairro de 100 e localização de 250.
- Toda mudança de status exige justificativa de até 1.000 caracteres e gera um histórico com status anterior, novo status e data.
- Não é permitido alterar para o mesmo status atual.
- Uma ocorrência encerrada não pode ter seus dados editados.
- Para reabrir uma ocorrência encerrada, o novo status deve ser `EmAnalise`.
- O autor não pode confirmar a própria ocorrência.
- Cada usuário pode confirmar uma ocorrência apenas uma vez. O banco também possui um índice único para o par ocorrência/usuário.

### Leitura dos indicadores

`ocorrenciasAbertas` reúne todas as ocorrências cujo status não é `Encerrada`. `semMudancaDeStatus` conta os registros atualmente no status `Registrada`; não analisa o histórico para determinar se nunca houve uma mudança anterior. Confirmações e edições textuais não alteram esse indicador.

Os indicadores representam os dados cadastrados no protótipo. Não constituem uma medição comprovada de impacto urbano ou de resolução de problemas reais.

## Organização

| Projeto                      | Responsabilidade                                                                 |
| ---------------------------- | -------------------------------------------------------------------------------- |
| `BairroEmDia.Api`            | Controllers, DTOs, serviços, tratamento de exceções e configuração da aplicação. |
| `BairroEmDia.Domain`         | Entidades, enums, interfaces de repositórios e regras de negócio.                |
| `BairroEmDia.Infrastructure` | Contexto EF Core, mapeamentos, migrations e repositórios.                        |
| `BairroEmDia.Tests`          | Testes de domínio e de serviços com xUnit e Moq.                                 |

A solução utiliza encapsulamento, interfaces, injeção de dependência, consultas LINQ e operações assíncronas com `async/await` e `CancellationToken`.

## Como executar

### Pré-requisitos

- SDK do .NET 10.
- Git.
- VS Code ou outra IDE de sua preferência.

Não é necessário instalar um servidor de banco de dados. O SQLite é utilizado pelo projeto.

### 1. Obter o projeto

```powershell
git clone https://github.com/Laurasdc/GS-C-CitizenTech.git
cd GS-C-CitizenTech
```

Execute os próximos comandos nessa pasta, que contém `BairroEmDia.slnx` e `dotnet-tools.json`.

### 2. Restaurar dependências e ferramentas

```powershell
dotnet restore
dotnet tool restore
```

### 3. Criar o banco e aplicar as migrations existentes

```powershell
dotnet ef database update --project BairroEmDia.Infrastructure --startup-project BairroEmDia.Api
```

O repositório inclui as migrations inicial, `AdicionaCidadeEUf` e `AdicionaAutenticacao`; não é necessário gerá-las novamente. A migration de localidades preserva os registros antigos como São Paulo/SP. A de autenticação adiciona as tabelas de contas e sessões sem apagar usuários ou ocorrências existentes. O comando cria o banco e aplica o esquema. Um banco novo começa sem usuários ou ocorrências.

A conexão está em `BairroEmDia.Api/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=bairroemdia.db"
}
```

O caminho do SQLite é relativo ao diretório de execução da aplicação. Com os comandos documentados, o arquivo é criado no projeto da API. Mantenha a mesma forma de execução para utilizar o mesmo banco. O banco local e seus arquivos auxiliares são ignorados pelo Git.

### 4. Iniciar a API

```powershell
dotnet run --project BairroEmDia.Api --launch-profile http
```

Com o perfil atual:

- API: [http://localhost:5239](http://localhost:5239)
- Scalar: [http://localhost:5239/scalar](http://localhost:5239/scalar)
- OpenAPI: [http://localhost:5239/openapi/v1.json](http://localhost:5239/openapi/v1.json)

A raiz da API não possui página própria. A documentação está em `/scalar` e fica habilitada no ambiente `Development`. Se a porta for alterada, utilize o endereço exibido em `Now listening on:`. Para encerrar a execução, pressione `Ctrl+C`.

## Autenticação e permissões

A consulta de ocorrências, detalhes e indicadores é pública. Para participar, crie uma conta em `POST /api/auth/registrar` e entre em `POST /api/auth/entrar`.

- As senhas usam o `PasswordHasher` do ASP.NET Core Identity; a senha original não é salva.
- Cada sessão possui um token aleatório de 256 bits. Somente o hash SHA-256 do token fica no SQLite.
- A sessão expira em 8 horas, sem renovação automática. O logout revoga a sessão atual no servidor.
- O navegador usa `POST /api/auth/entrar?usarCookie=true`: recebe cookie HttpOnly, com `SameSite=Lax`; o token não é devolvido no JSON. O app nativo usa `Authorization: Bearer <token>` e SecureStore.
- Login e cadastro compartilham o limite de 10 solicitações por IP por minuto (429).
- Requisições de escrita com cookie exigem uma origem autorizada; o CORS permite credenciais somente para origens explícitas.
- O autor da ocorrência e da confirmação vem da sessão. Enviar outro `usuarioId` não altera a identidade.
- Cadastros públicos recebem o perfil `Morador`. Edição é permitida ao autor ou Gestor. Alteração de status e gestão de usuários exigem `Gestor`.
- O app não tem promoção pública de perfil, recuperação de senha, verificação de e-mail ou login social nesta etapa.

Usuários antigos de demonstração continuam vinculados aos seus relatos, mas não ganham senhas automaticamente. Crie uma conta nova pelo app. Não existe senha padrão.

### Endpoints

| Método | Rota | Acesso / operação |
| --- | --- | --- |
| POST | `/api/auth/registrar` | Público: criar conta com nome, e-mail e senha. |
| POST | `/api/auth/entrar` | Público: iniciar sessão. |
| GET | `/api/auth/me` | Autenticado: consultar sua própria conta. |
| POST | `/api/auth/sair` | Autenticado: revogar a sessão atual. |
| GET | `/api/ocorrencias` | Público: listar e filtrar. |
| GET | `/api/ocorrencias/{id}` | Público: detalhes e histórico. |
| POST | `/api/ocorrencias` | Autenticado: registrar em seu próprio nome. |
| PUT | `/api/ocorrencias/{id}` | Autor ou Gestor: editar. |
| PATCH | `/api/ocorrencias/{id}/status` | Gestor: alterar status com justificativa. |
| POST | `/api/ocorrencias/{id}/confirmacoes` | Autenticado: confirmar, sem corpo e sem `usuarioId`. |
| GET | `/api/ocorrencias/{id}/minha-confirmacao` | Autenticado: verificar sua confirmação. |
| GET | `/api/indicadores` | Público: indicadores e filtros. |
| POST / GET | `/api/usuarios` | Gestor: operações de usuários de demonstração. |
| GET | `/api/usuarios/{id}` | Gestor: consultar usuário. |

Respostas: 400 (validação/regra de negócio), 401 (sessão ausente, inválida ou expirada), 403 (permissão/origem recusada), 404 (não encontrado), 409 (duplicidade) e 429 (limite de tentativas).

### Exemplo para cliente nativo ou ferramenta HTTP

Em `POST /api/auth/registrar`:

```json
{ "nome": "Moradora de Teste", "email": "moradora@example.com", "senha": "UmaSenhaDeTeste123!" }
```

Entre em `POST /api/auth/entrar` com e-mail e senha. Use o `token` retornado no cabeçalho `Authorization: Bearer <token>` nas chamadas autenticadas. O token é opaco, não é um JWT.

Em `POST /api/ocorrencias`:

```json
{
  "titulo": "Poste apagado próximo ao ponto de ônibus",
  "descricao": "A iluminação está apagada durante a noite.",
  "cidade": "São Paulo", "uf": "SP", "bairro": "Chácara Klabin",
  "localizacao": "Rua de demonstração, próximo ao número 100", "categoria": "Iluminacao"
}
```

Entre com uma segunda conta para confirmar em `POST /api/ocorrencias/{id}/confirmacoes`, sem corpo. A própria ocorrência e uma confirmação repetida são recusadas.

No modo web com cookie, ferramentas HTTP também precisam enviar o cabeçalho `Origin` do frontend autorizado para escrever. No ambiente local, use `http://localhost:8081` ou `http://127.0.0.1:8081`. Não coloque tokens ou senhas no repositório.

### Configuração fora do ambiente local

Use HTTPS e configure as origens exatas em `Frontend:Origins` (por exemplo, `Frontend__Origins__0` nas variáveis de ambiente). O cookie é `Secure` fora de Development. O frontend web e a API devem compartilhar o mesmo site para a política `SameSite=Lax`; um domínio de API completamente diferente exige rever essa configuração antes da publicação. O perfil Gestor precisa de provisionamento administrativo fora do cadastro público; não existe conta administrativa padrão.

## Testes e evidências de validação

Execute:

```powershell
dotnet test
```

Na validação da autenticação, os **42 testes passaram, sem falhas**, incluindo validação de UF/cidade, filtros em SQLite real e preservação dos registros antigos pela migration. A suíte cobre criação e validação de ocorrências, mudanças de status, histórico, encerramento, reabertura, confirmações e decisões de consulta/persistência dos serviços. Os testes de indicadores conferem os totais, o cenário vazio e o repasse do filtro de bairro.

Os testes de serviços utilizam Moq. Os testes de integração de autenticação usam a API com WebApplicationFactory e SQLite em memória, cobrindo sessão, cookies, logout, expiração, autorização, confirmação repetida, identidade e limite de tentativas.

Também foram executados manualmente pelo Scalar:

| Cenário                                           | Resultado observado                              |
| ------------------------------------------------- | ------------------------------------------------ |
| Cadastro de usuário e ocorrência                  | `201 Created`.                                   |
| Alteração de status e consulta em nova requisição | `200 OK`, com histórico persistido.              |
| Confirmação pelo autor                            | `400 Bad Request`.                               |
| Confirmação por outro usuário                     | `200 OK`, com total de confirmações atualizado.  |
| Confirmação repetida                              | `400 Bad Request`.                               |
| Filtro com e sem correspondência                  | `200 OK`, com lista preenchida ou vazia.         |
| Edição de ocorrência aberta                       | `200 OK`.                                        |
| Edição de ocorrência encerrada                    | `400 Bad Request`.                               |
| Reabertura para análise                           | `200 OK`.                                        |
| Indicadores do bairro                             | Totais conferidos com a listagem de ocorrências. |

Esses resultados documentam a validação do protótipo com dados de demonstração.

## Fundamentação e ODS

A proposta foi contextualizada por pesquisa documental em fontes públicas. Não foram realizadas entrevistas próprias nem pesquisas de campo com moradores.

- A [Prefeitura de São Paulo, nas perguntas frequentes do SP156](https://prefeitura.sp.gov.br/web/inovacao/w/acesso_a_informacao/350229), descreve canais para solicitar serviços e acompanhar protocolos. Essa referência orienta a importância do registro e do acompanhamento, sem significar que o Bairro em Dia esteja integrado ao SP156.
- O [Manual da Sociedade dos Amigos, Moradores e Empreendedores do Alto de Pinheiros — SAAP](https://www.saap.org.br/wp-content/uploads/2023/03/Manual-da-SAAP-final.pdf), na seção sobre problemas de zeladoria, orienta moradores a registrar problemas de iluminação, buracos e lixo e compartilhar protocolos para acompanhamento pela associação. Trata-se de uma referência de organização comunitária em outro bairro, e não de um diagnóstico específico da Chácara Klabin.
- O [ODS 11, apresentado pelas Nações Unidas no Brasil](https://brasil.un.org/pt-br/sdgs/11), trata de cidades inclusivas e sustentáveis. A proposta se relaciona à participação na gestão urbana (meta 11.3) e à atenção aos espaços públicos seguros e acessíveis (meta 11.7), ao organizar relatos e seu acompanhamento comunitário.

Fontes consultadas em 08/11/2026. Elas fundamentam o contexto e a proposta; não comprovam demanda local ou impacto efetivamente obtido pelo aplicativo.

## Localidades e filtros

Os endpoints `GET /api/ocorrencias` e `GET /api/indicadores` aceitam `uf`, `cidade` e `bairro` opcionais. Omitir um campo amplia a consulta: apenas `uf=SP` consulta todo o estado, enquanto `uf=SP&cidade=Campinas` consulta a cidade inteira. Para consultar um bairro específico sem misturar nomes iguais, informe os três campos.

Cidade e bairro são comparados pelo nome completo. A comparação SQLite `NOCASE` cobre diferenças de caixa em ASCII; não remove acentos nem unifica variantes de escrita. Use os mesmos acentos do cadastro. UF inválida retorna `400`. Novos cadastros e edições exigem `cidade` e `uf` no JSON. A atualização do seletor de localidades no Mobile será feita na etapa seguinte.

## Integração Mobile

O app Expo/React Native consome as consultas públicas e a autenticação desta API, com login, cadastro, logout e confirmação de ocorrência. A preferência de tema fica separada da sessão. O formulário de criação de ocorrência no Mobile será a próxima etapa.
