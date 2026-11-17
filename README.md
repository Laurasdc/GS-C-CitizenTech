# Bairro em Dia — API C# e integração SOA

Backend para registrar, consultar e acompanhar problemas nos espaços públicos: iluminação apagada, lixo acumulado, buracos e calçadas danificadas. O aplicativo Mobile consome a API ASP.NET Core; o serviço Java de SOA consulta essa API para gerar análises agregadas.

## Ficha da CitizenTech

| Campo | Descrição |
| --- | --- |
| Nome da CitizenTech | Bairro em Dia. |
| Cidade e bairro | Cadastro disponível para cidades e bairros de todo o Brasil. O cenário de demonstração inclui Chácara Klabin, São Paulo/SP. |
| Cidadão beneficiado | Moradores que precisam registrar problemas nos espaços públicos e acompanhar seu andamento, incluindo pessoas afetadas por calçadas danificadas e falta de iluminação. |
| Problema, em uma frase | Relatos dispersos e falta de visibilidade sobre o acompanhamento de problemas de manutenção dos espaços públicos. |
| Como funciona | O morador consulta ou registra uma ocorrência, outras pessoas confirmam que também são afetadas e o gestor atualiza a situação com justificativas disponíveis no histórico. |
| O que cada parte faz | Mobile: interface dos moradores e gestores; C#: autenticação, ocorrências, confirmações e histórico em SQLite; SOA em Java: consulta C# por HTTP e armazena análises agregadas em PostgreSQL. |
| Participantes do ecossistema | Moradores e gestores da comunidade. Órgãos e prestadores de manutenção são possíveis destinatários de encaminhamentos futuros. |
| Repositórios | [Mobile](https://github.com/Laurasdc/GS-Mobile-CitizenTech) e [API C# e integração SOA](https://github.com/Laurasdc/GS-C-CitizenTech). |
| Indicadores de acompanhamento e impacto | Quantidade de relatos, distribuição por categoria e situação, confirmações de moradores e proporção de relatos encerrados. A consulta por região e as análises salvas permitem acompanhar diferentes localidades e momentos. |
| ODS conectados | [ODS 11 — Cidades e comunidades sustentáveis](https://brasil.un.org/pt-br/sdgs/11): contribuição proposta às metas 11.3 e 11.7 por meio da participação dos moradores e da identificação de problemas que prejudicam o uso seguro e acessível dos espaços públicos. |

O projeto é um protótipo acadêmico. Os indicadores refletem registros do sistema; o encerramento de um relato não comprova a execução de manutenção real. Não há envio automático de solicitações à Prefeitura.

## Integrantes

| Nome | RM |
| --- | --- |
| Laura Souza | 556320 |
| Vinicius Henrique | 556908 |
| Gustavo Pierre | 558928 |
| Gabriel Belo | 551669 |

## Funcionalidades

- Cadastro de contas, login, consulta da conta e logout.
- Cadastro, consulta e edição de ocorrências abertas.
- Listagem pública com filtros por UF, cidade, bairro, categoria e situação.
- Consulta dos relatos da própria conta autenticada.
- Confirmação de que outro morador também é afetado, sem duplicação ou confirmação do próprio relato.
- Atualização de situação por gestores com justificativa e histórico.
- Encerramento e reabertura de ocorrências para análise.
- Indicadores de ocorrências abertas, encerradas e distribuição por situação e categoria.
- Persistência em SQLite com migrations do Entity Framework Core.
- Documentação automática OpenAPI e interface Scalar.
- Integração HTTP com SOA para gerar e consultar análises persistidas em PostgreSQL.

## Tecnologias e organização

| Componente | Tecnologias e responsabilidade |
| --- | --- |
| `BairroEmDia.Domain` | C#: entidades, enums, interfaces e regras de domínio. |
| `BairroEmDia.Infrastructure` | Entity Framework Core, repositórios, LINQ, SQLite e migrations. |
| `BairroEmDia.Api` | .NET 10 e ASP.NET Core: controllers, DTOs, serviços com async/await, autenticação, injeção de dependência, OpenAPI e Scalar. |
| `BairroEmDia.Tests` | xUnit, Moq, WebApplicationFactory e SQLite em memória. |
| `soa-integracao` | Java 21, Spring Boot, JPA, PostgreSQL, Flyway, Swagger, JUnit e Mockito; execução com Docker Compose ou ambiente local. |

## Executar a API C#

Requer SDK .NET 10. Comece na raiz do repositório:

```powershell
dotnet restore
dotnet tool restore --tool-manifest dotnet-tools.json
```

A conexão está em `BairroEmDia.Api/appsettings.json`, na chave `ConnectionStrings:DefaultConnection`. O padrão é `Data Source=bairroemdia.db`. Aplique as migrações a partir da pasta da API:

```powershell
cd BairroEmDia.Api
dotnet ef database update --project ../BairroEmDia.Infrastructure --startup-project .
cd ..
```

Inicie o backend:

```powershell
dotnet run --project BairroEmDia.Api --launch-profile http -- --urls http://0.0.0.0:5239
```

- API: `http://localhost:5239/api`.
- Scalar: [http://localhost:5239/scalar/v1](http://localhost:5239/scalar/v1).
- OpenAPI: [http://localhost:5239/openapi/v1.json](http://localhost:5239/openapi/v1.json).

Scalar e OpenAPI ficam disponíveis no perfil de desenvolvimento. O endereço `0.0.0.0` permite que o celular e o container SOA alcancem o backend.

## Contas e perfis

Crie uma conta pelo aplicativo ou por `POST /api/auth/registrar`, informando `nome`, `email` e `senha`. O cadastro público concede o perfil **Morador**.

Para autenticação HTTP, faça login em `POST /api/auth/entrar` e envie o token retornado no cabeçalho `Authorization: Bearer SEU_TOKEN`. O aplicativo web utiliza a opção de sessão por cookie HttpOnly. As sessões expiram em 8 horas e são revogadas no logout.

Para habilitar **Gestor** em uma conta existente, pare a API e execute na raiz do repositório:

```powershell
dotnet run --project BairroEmDia.Api --launch-profile http -- --definir-gestor "seu-email@example.com"
```

Depois, inicie a API novamente e faça um novo login. O gestor pode atualizar situações e acessar a gestão de usuários.

Para redefinir localmente a senha de uma conta existente:

```powershell
dotnet run --project BairroEmDia.Api --launch-profile http -- --redefinir-senha "seu-email@example.com"
```

O comando solicita a nova senha duas vezes, sem exibir os caracteres, e encerra as sessões anteriores da conta. Os dois comandos administrativos são locais e terminam sem iniciar um servidor HTTP.

## Endpoints principais

| Método | Rota | Acesso / função |
| --- | --- | --- |
| POST | `/api/auth/registrar` | Público: criar conta. |
| POST | `/api/auth/entrar` | Público: entrar. |
| GET | `/api/auth/me` | Autenticado: consultar a conta. |
| POST | `/api/auth/sair` | Autenticado: sair. |
| GET | `/api/ocorrencias` | Público: listar e filtrar relatos. |
| GET | `/api/ocorrencias/{id}` | Público: consultar detalhes e histórico. |
| POST | `/api/ocorrencias` | Autenticado: registrar um problema. |
| GET | `/api/ocorrencias/minhas` | Autenticado: consultar os próprios relatos. |
| PUT | `/api/ocorrencias/{id}` | Autor ou gestor: editar uma ocorrência aberta. |
| PATCH | `/api/ocorrencias/{id}/status` | Gestor: atualizar situação e justificativa. |
| POST | `/api/ocorrencias/{id}/confirmacoes` | Autenticado: confirmar que também é afetado. |
| GET | `/api/ocorrencias/{id}/minha-confirmacao` | Autenticado: consultar a própria confirmação. |
| GET | `/api/indicadores` | Público: consultar indicadores gerais ou por região. |
| GET / POST | `/api/usuarios` | Gestor: consultar ou cadastrar usuários. |
| GET | `/api/usuarios/{id}` | Gestor: consultar um usuário. |

Os campos e códigos de resposta estão no Scalar. O cadastro de usuário em `/api/usuarios` não substitui o cadastro de conta em `/api/auth/registrar`.

## Categorias, situações e regras

| Valor | Categoria no JSON | Situação no JSON |
| --- | --- | --- |
| 1 | `Iluminacao` | `Registrada` |
| 2 | `LixoAcumulado` | `EmAnalise` |
| 3 | `Buraco` | `EmAcompanhamento` |
| 4 | `CalcadaDanificada` | `Encerrada` |

Use os nomes dos enums nos corpos JSON. Consulte os filtros pelo contrato gerado no Scalar.

- Novas ocorrências começam em `Registrada` e são vinculadas ao autor autenticado.
- Toda mudança de situação exige justificativa e gera um histórico.
- Não é permitido atualizar para a mesma situação atual.
- Ocorrências encerradas não podem ser editadas; a reabertura deve ser para `EmAnalise`.
- O autor não pode confirmar o próprio relato; outra pessoa só pode confirmá-lo uma vez.
- A UF é validada; cidade e bairro são informados como texto.
- Título: até 120 caracteres; descrição: até 2.000; cidade e bairro: até 100; localização: até 250; justificativa: até 1.000.

## Integração com Mobile e SOA

O [Mobile](https://github.com/Laurasdc/GS-Mobile-CitizenTech) usa C# para as operações de moradores e gestores. O SOA consulta `GET /api/ocorrencias` por HTTP e salva análises no PostgreSQL, com data e filtros. C# continua sendo a fonte das ocorrências. Cada serviço acessa apenas seu próprio banco.

Com C# rodando, execute em outro terminal, a partir da raiz:

```powershell
cd soa-integracao
docker compose -p bairroemdia-soa-integracao up --build -d
powershell -NoProfile -ExecutionPolicy Bypass -File .\Testar-Integracao.ps1
```

Requer Docker Desktop iniciado com suporte a containers Linux. A execução sem Docker e os demais detalhes estão no [README do SOA](soa-integracao/README.md).

| Configuração | Endereço |
| --- | --- |
| Swagger SOA | [http://localhost:8080/swagger-ui/index.html](http://localhost:8080/swagger-ui/index.html) |
| PostgreSQL SOA | `localhost:5433`, banco `bairroemdia_soa_integracao` |
| C# visto pelo container | `CSHARP_API_URL=http://host.docker.internal:5239` — sem `/api` no final |
| C# visto pelo Mobile | `EXPO_PUBLIC_API_URL=http://IP_DO_COMPUTADOR:5239/api` — com `/api` no final |

No Swagger SOA, `POST /api/analises` com `{}` gera uma análise geral. `GET /api/analises` lista as mais recentes e `GET /api/analises/{id}` consulta uma análise salva. Os filtros opcionais são `uf`, `cidade` e `bairro`.

## Testes

Na raiz do repositório:

```powershell
dotnet test BairroEmDia.Tests/BairroEmDia.Tests.csproj
```

Os testes C# cobrem regras das ocorrências, indicadores, localidades, autenticação, autorização e redefinição local de senha.

Para executar os testes Java, na pasta `soa-integracao/soa`, com Java 21 e Maven:

```powershell
mvn test
```

Os testes Java cobrem contrato HTTP, domínio, endpoints, migração e persistência. A conferência das duas APIs no ambiente da equipe está registrada nas evidências.

## Demonstração e evidências

- [Vídeo do aplicativo consumindo a API C#](https://youtube.com/shorts/rhYkBh2buLQ).
- [Evidências da aplicação, integração e migrations](docs/EVIDENCIAS.md).
- [Validação detalhada do SOA](soa-integracao/VALIDACAO.md).
