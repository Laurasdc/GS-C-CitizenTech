# Bairro em Dia — integração SOA e C#

Serviço Java/Spring Boot que consulta as ocorrências da API ASP.NET Core por HTTP e persiste análises agregadas em PostgreSQL. Permite acompanhar a distribuição das ocorrências por situação e categoria em diferentes momentos.

## Integrantes

| Nome | RM |
| --- | --- |
| Laura Souza | 556320 |
| Vinicius Henrique | 556908 |
| Gustavo Pierre | 558928 |
| Gabriel Belo | 551669 |

## Responsabilidades

| Componente | Responsabilidade |
| --- | --- |
| Mobile | Consulta e participação dos moradores, consumindo C#. |
| API C# | Contas, autenticação, ocorrências, confirmações, histórico e regras do aplicativo. Continua usando seu banco SQLite. |
| API Java de SOA | Consulta HTTP da listagem pública do C#, cálculo e armazenamento de análises agregadas. |
| PostgreSQL do SOA | Guarda apenas análises; não copia usuários, senhas nem o cadastro das ocorrências. |

Ao gerar uma análise: **SOA → GET da API C# → cálculo → PostgreSQL → resposta**. A consulta a uma análise já salva usa somente o PostgreSQL.

A comunicação é por HTTP/JSON. O Java não acessa o SQLite e o C# não acessa o PostgreSQL. A API C# continua sendo a fonte das ocorrências. Nenhum endpoint do Mobile, ID ou status precisa ser alterado. A indisponibilidade do SOA não impede usar o aplicativo.

## Funcionalidades

- Gerar uma análise de todas as ocorrências ou filtrar por UF, cidade e bairro.
- Contabilizar ocorrências abertas e encerradas e a distribuição por situação e categoria.
- Persistir a data e os filtros de cada análise para consulta posterior.
- Consultar as 20 análises mais recentes ou uma análise pelo ID.
- Documentar automaticamente os endpoints com OpenAPI e Swagger UI.
- Aplicar migrações do banco com Flyway e validar o esquema com Hibernate.
- Retornar HTTP 502 se C# não responder ou enviar um contrato inválido, sem gravar uma análise falsa com zeros.

As análises são fotografias dos dados no momento da geração. Não se atualizam sozinhas e não comprovam solução de problemas reais. Um total zero só é válido quando C# respondeu com uma lista vazia.

## Instalação

Copie esta pasta `soa-integracao` para a raiz do backend, ao lado de `BairroEmDia.Api`, `BairroEmDia.Domain` e `BairroEmDia.Infrastructure`. Não substitua as pastas C#, o banco SQLite, o README principal ou arquivos de ambiente do Mobile.

Este módulo é a adaptação da parte SOA recebida pela equipe para a divisão de responsabilidades acima. A estrutura Spring Boot, Java 21, JPA, PostgreSQL, Flyway e Swagger foi aproveitada. O segundo CRUD de ocorrências foi substituído pelo domínio de análises para manter uma única fonte de dados. ML, notebooks, dados de treino e entregas das demais matérias não são necessários e não estão neste pacote.

## Executar com Docker Desktop

### 1. Iniciar C#

Na raiz do backend, com o banco já configurado conforme seu README:

```powershell
dotnet run --project BairroEmDia.Api --launch-profile http -- --urls http://0.0.0.0:5239
```

Deixe esse terminal aberto. `0.0.0.0` permite que o container Java consulte a API no computador.

### 2. Iniciar SOA e PostgreSQL

Em outro terminal, na raiz do backend:

```powershell
cd soa-integracao
docker compose -p bairroemdia-soa-integracao up --build -d
docker compose -p bairroemdia-soa-integracao logs -f soa
```

Requer Docker Desktop instalado e iniciado, com containers Linux. Na primeira execução, Docker baixa as imagens e Maven baixa as dependências.

| Serviço | Endereço |
| --- | --- |
| API C# no computador | `http://localhost:5239` |
| API Java | `http://localhost:8080` |
| Swagger do SOA | `http://localhost:8080/swagger-ui/index.html` |
| OpenAPI JSON | `http://localhost:8080/v3/api-docs` |
| PostgreSQL no computador | `localhost:5433` |

O banco deste módulo é `bairroemdia_soa_integracao`, usuário `postgres`, senha `postgres`, com volume próprio. Essas credenciais são para desenvolvimento local. As portas do SOA e PostgreSQL são publicadas apenas em localhost. Não execute o Compose original que inclui ML para este teste.

No container, `CSHARP_API_URL=http://host.docker.internal:5239`. Essa variável é a URL da raiz do backend, **sem `/api` no final**. Em Linux, o Compose inclui a resolução `host-gateway`. Se o acesso falhar, confira a API C#, o endereço e o firewall local.

O banco e o volume têm nomes próprios para não reutilizar as migrações do antigo CRUD Java. Não apague o banco C# nem execute `docker compose down -v` para aplicar esta integração.

### 3. Conferir a integração

No Swagger, execute **POST `/api/analises`** com o corpo:

```json
{}
```

Isso consulta as ocorrências existentes no C# e grava uma análise. Para filtrar:

```json
{
  "uf": "SP",
  "cidade": "São Paulo",
  "bairro": "Chácara Klabin"
}
```

Os filtros são opcionais. A consulta por cidade e bairro segue as regras e a escrita dos nomes cadastrados no C#.

Também há um script de conferência, executado na pasta `soa-integracao`:

```powershell
.\Testar-Integracao.ps1
```

Ele compara as contagens retornadas pelo SOA com a listagem real do C#, grava uma análise e a consulta pelo ID. Não cria, edita ou exclui ocorrências. Evite alterar ocorrências enquanto o script executa para não comparar dois momentos diferentes.

Se a política do PowerShell bloquear o script, use esta execução pontual:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Testar-Integracao.ps1
```

## Endpoints

| Método | Rota | Resultado |
| --- | --- | --- |
| GET | `/api/health` | Verifica se o serviço Java está respondendo. Não atesta a conexão com C#. |
| POST | `/api/analises` | Consulta C#, calcula e grava uma análise; retorna 201 e `Location`. |
| GET | `/api/analises` | Lista as 20 análises mais recentes salvas. |
| GET | `/api/analises/{id}` | Consulta uma análise salva; retorna 404 se ela não existir. |

Erros de filtro retornam 400. Falhas da fonte C# retornam 502. Uma análise salva continua acessível se C# ficar indisponível. O POST consome apenas a rota pública `GET /api/ocorrencias` e não recebe tokens, senhas ou identificadores de contas. Os endpoints de análises são locais e não expõem operações administrativas do C#.

### Contrato de integração

O cliente Java lê `id` como UUID, `status` e `categoria` da resposta JSON de C#; os outros campos são ignorados. Os filtros enviados são `uf`, `cidade` e `bairro`.

| Situações aceitas | Categorias aceitas |
| --- | --- |
| `Registrada` | `Iluminacao` |
| `EmAnalise` | `LixoAcumulado` |
| `EmAcompanhamento` | `Buraco` |
| `Encerrada` | `CalcadaDanificada` |

Uma ocorrência é aberta quando seu status não é `Encerrada`. Os status são os mesmos do C#, sem tradução para `ABERTA` ou `RESOLVIDA`. Cada UUID é contado apenas uma vez; uma resposta com IDs duplicados ou campos obrigatórios ausentes é rejeitada.

## Executar sem Docker

Requer Java 21, Maven 3.9+ e PostgreSQL disponível. Crie um banco vazio `bairroemdia_soa_integracao` e configure na sessão do PowerShell:

```powershell
$env:DB_URL="jdbc:postgresql://localhost:5433/bairroemdia_soa_integracao"
$env:DB_USER="postgres"
$env:DB_PASSWORD="postgres"
$env:CSHARP_API_URL="http://localhost:5239"
cd soa
mvn spring-boot:run
```

Flyway aplica o esquema automaticamente no início. Hibernate usa `validate`, sem recriar as tabelas.

## Testes automatizados

Na pasta `soa`:

```powershell
mvn test
```

Ou usando a imagem Maven do Docker, a partir de `soa-integracao`:

```powershell
docker run --rm -v "${PWD}/soa:/workspace" -w /workspace maven:3.9.9-eclipse-temurin-21 mvn test
```

Os testes cobrem o contrato HTTP com C#, filtros acentuados, falhas de conexão, contagens, validação dos endpoints, migração Flyway e gravação/leitura no banco de testes H2 em modo PostgreSQL. A conferência com a API C# real e PostgreSQL no computador da equipe está registrada nas [evidências](../docs/EVIDENCIAS.md).

## Evidências da integração

[Veja os prints da integração e da migração aplicadas pela equipe](../docs/EVIDENCIAS.md). Os comandos abaixo permitem reproduzir a conferência.

1. Com as duas APIs ativas, execute o script e salve a saída com as contagens conferidas.
2. Capture o POST e o GET da análise no Swagger, mostrando seu ID e as contagens.
3. Registre as migrações aplicadas:

```powershell
docker compose -p bairroemdia-soa-integracao exec postgres-soa psql -U postgres -d bairroemdia_soa_integracao -c "SELECT version, description, success FROM flyway_schema_history;"
docker compose -p bairroemdia-soa-integracao exec postgres-soa psql -U postgres -d bairroemdia_soa_integracao -c "SELECT id, gerada_em, uf, cidade, bairro, total FROM analises_ocorrencias ORDER BY gerada_em DESC LIMIT 5;"
```

4. Pare somente C# e tente gerar outra análise: deve retornar 502. Consulte o ID já salvo: deve continuar acessível. Reinicie C# para voltar a usar o aplicativo.

Para parar os containers preservando as análises:

```powershell
docker compose -p bairroemdia-soa-integracao down
```
