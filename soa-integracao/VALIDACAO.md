# Verificação da integração

## Resultado

- Compilação e empacotamento do serviço Java concluídos com Java 21 e Maven.
- 12 testes automatizados passaram, sem falhas ou testes ignorados.
- Flyway e Hibernate validaram o esquema no banco H2 em modo PostgreSQL durante os testes.
- A aplicação empacotada também foi iniciada com PostgreSQL 16.4 real em um banco temporário de teste. A migração e a validação do esquema concluíram com sucesso.
- Na verificação HTTP com PostgreSQL real, o serviço consultou uma fonte simulando o JSON público de C#, persistiu a análise, retornou 201, encaminhou filtros com acentos e permitiu consultar a análise salva.
- Quando a fonte simulada retornou 503, o SOA retornou 502 e não gravou uma análise adicional. A leitura da análise já persistida continuou funcionando.

## Limites dessa verificação

A fonte HTTP do teste com PostgreSQL era simulada: não era a API C# do computador da equipe. Os dados usados eram exclusivamente exemplos de teste. O contrato foi comparado com o controller, DTO e enums de C# disponíveis no projeto.

Docker Desktop e o Compose completo não foram executados neste ambiente. A configuração YAML foi validada e a aplicação Java foi testada diretamente com PostgreSQL. Execute `Testar-Integracao.ps1` com a API C# real e o Compose no computador da equipe para registrar a evidência final.

O pacote não modifica o código de C#, o banco SQLite ou o Mobile. Não é necessário migrar os relatos existentes. As análises novas são guardadas no PostgreSQL separado deste módulo.

## Cobertura dos testes

| Arquivo | Cenários |
| --- | --- |
| AnaliseDomainTest | Agregação de status e categoria, lista vazia e rejeição de dados duplicados/incompletos. |
| CSharpClientTest | Contrato JSON de C#, filtros acentuados, erro HTTP, falha de conexão e rejeição de contrato incompatível. |
| PersistenciaTest | Migração, gravação/leitura e não gravação quando a fonte falha. |
| AnaliseControllerTest | Resposta 201 com Location, validação de UF e resposta 502. |
