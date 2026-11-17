# Verificação da integração

## Resultado

- Compilação e empacotamento do serviço Java concluídos com Java 21 e Maven.
- 12 testes automatizados passaram, sem falhas ou testes ignorados.
- Flyway e Hibernate validaram o esquema no banco H2 em modo PostgreSQL durante os testes.
- A aplicação empacotada também foi iniciada com PostgreSQL 16.4 real em um banco temporário de teste. A migração e a validação do esquema concluíram com sucesso.
- Na verificação HTTP com PostgreSQL real, o serviço consultou uma fonte simulando o JSON público de C#, persistiu a análise, retornou 201, encaminhou filtros com acentos e permitiu consultar a análise salva.
- Quando a fonte simulada retornou 503, o SOA retornou 502 e não gravou uma análise adicional. A leitura da análise já persistida continuou funcionando.

## Conferência no computador da equipe

Em 17/11/2026, a equipe iniciou SOA e PostgreSQL com Docker Compose e executou `Testar-Integracao.ps1` com a API C# real. A conferência concluiu com sucesso, comparando o total, a distribuição por status e categoria e a consulta da análise persistida.

O resultado registrado foi de 5 ocorrências abertas: 3 em análise e 2 registradas, distribuídas em 1 buraco e 4 ocorrências de iluminação. O histórico Flyway confirmou a migração `1 — criar analises`, com `success = t`.

Os [prints da execução estão nas evidências](../docs/EVIDENCIAS.md). A verificação de indisponibilidade descrita na seção anterior foi feita com fonte HTTP simulada; não há registro desse cenário executado no computador da equipe.

As análises são guardadas no PostgreSQL do SOA. O script consulta a API C# por HTTP e não modifica as ocorrências do SQLite.

## Cobertura dos testes

| Arquivo | Cenários |
| --- | --- |
| AnaliseDomainTest | Agregação de status e categoria, lista vazia e rejeição de dados duplicados/incompletos. |
| CSharpClientTest | Contrato JSON de C#, filtros acentuados, erro HTTP, falha de conexão e rejeição de contrato incompatível. |
| PersistenciaTest | Migração, gravação/leitura e não gravação quando a fonte falha. |
| AnaliseControllerTest | Resposta 201 com Location, validação de UF e resposta 502. |
