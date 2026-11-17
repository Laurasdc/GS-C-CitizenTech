# Bairro em Dia

Aplicativo para registrar problemas nos espaços públicos e acompanhar os relatos da comunidade em cidades de todo o Brasil. A proposta é aproximar moradores e gestão, reunindo informações sobre problemas do bairro e tornando seu acompanhamento mais claro.

## Vídeo de demonstração

[Assista ao Bairro em Dia em funcionamento](https://youtube.com/shorts/rhYkBh2buLQ)

## Integrantes

| Nome | RM |
| --- | --- |
| Laura Souza | 556320 |
| Vinicius Henrique | 556908 |
| Gustavo Pierre | 558928 |
| Gabriel Belo | 551669 |

## Tecnologias utilizadas

| Tecnologia | Utilização |
| --- | --- |
| Expo e React Native | Desenvolvimento do aplicativo para celular e navegador. |
| TypeScript | Tipagem do código. |
| Expo Router | Navegação entre telas. |
| React Native Web | Execução da interface no navegador. |
| Expo SecureStore | Armazenamento da sessão no aplicativo nativo. |
| AsyncStorage | Persistência da preferência de aparência. |
| API REST em ASP.NET Core | Autenticação e operações de usuários e ocorrências. |
| Entity Framework Core e SQLite | Persistência dos dados no backend. |

## Funcionalidades

### Consulta e participação

- Consultar ocorrências da comunidade sem precisar entrar em uma conta.
- Filtrar relatos por estado, cidade, bairro, categoria e situação.
- Visualizar a descrição, a localização, a situação atual e o histórico de atualizações de uma ocorrência.
- Criar uma conta, entrar, consultar os dados do perfil e sair.
- Registrar problemas de iluminação, lixo acumulado, buracos e calçadas danificadas.
- Confirmar que também é afetado por uma ocorrência de outro morador.
- Consultar os próprios registros em **Meus relatos** e editar suas informações enquanto estiverem abertos.

### Gestão de ocorrências

Contas com perfil **Gestor** têm acesso à área de gerenciamento para:

- Buscar ocorrências por título ou região e filtrar por situação.
- Atualizar a situação para **Registrada**, **Em análise**, **Em acompanhamento** ou **Encerrada**, conforme as transições permitidas.
- Informar uma justificativa que fica disponível no histórico para os moradores.
- Reabrir uma ocorrência encerrada para análise quando necessário.

### Interface

- Layout adaptado para celular e navegador.
- Temas claro, escuro e do dispositivo, com preferência salva.

## Telas do aplicativo

Prints do aplicativo no iPhone, no tema escuro.

| Início | Busca de ocorrências | Registro de problema |
| --- | --- | --- |
| <img src="docs/images/inicio.png" alt="Tela inicial do Bairro em Dia" width="260"> | <img src="docs/images/busca.png" alt="Busca de ocorrências por estado, cidade e bairro" width="260"> | <img src="docs/images/registro.png" alt="Formulário para registrar um problema" width="260"> |

## Como executar

### Pré-requisitos

- Node.js compatível com o Expo SDK 58 e npm.
- API do Bairro em Dia configurada, com o banco de dados atualizado e em execução.
- Expo Go compatível com o projeto para testar no celular, ou um navegador para a versão web.

O backend está no repositório [GS-C-CitizenTech](https://github.com/Laurasdc/GS-C-CitizenTech). Siga as instruções dele para configurar e iniciar a API.

### 1. Obter o projeto e instalar as dependências

```powershell
git clone https://github.com/Laurasdc/GS-Mobile-CitizenTech.git
cd GS-Mobile-CitizenTech
```

Entre na pasta que contém o arquivo `package.json` do aplicativo e execute:

```powershell
npm install
```

### 2. Configurar a conexão com a API

Na mesma pasta do `package.json`, crie o arquivo `.env.local` com o endereço da API:

```dotenv
EXPO_PUBLIC_API_URL=http://localhost:5239/api
```

Esse endereço serve para usar o navegador no próprio computador. Para acessar pelo celular, substitua `localhost` pelo IPv4 do computador, por exemplo:

```dotenv
EXPO_PUBLIC_API_URL=http://192.168.1.10:5239/api
```

No Windows, consulte o IPv4 da conexão ativa com:

```powershell
ipconfig
```

Para o teste no celular, conecte os dois dispositivos à mesma rede Wi-Fi e deixe a API acessível pela rede local. Na pasta do backend, você pode iniciá-la com:

```powershell
dotnet run --project BairroEmDia.Api --launch-profile http -- --urls http://0.0.0.0:5239
```

O endereço no `.env.local` deve terminar em `/api`. Reinicie o Expo sempre que alterar essa configuração.

### 3. Iniciar o aplicativo

```powershell
npx expo start --go --clear
```

- **Celular:** abra o projeto no Expo Go usando o QR Code exibido no terminal.
- **Navegador:** pressione `w` no terminal. O endereço padrão é `http://localhost:8081`.

Mantenha a API em execução durante o uso. Se utilizar outro endereço ou porta na versão web, ajuste também as origens permitidas no backend.

## Como usar o aplicativo

1. Na tela **Início**, escolha **Buscar ocorrências** para consultar os relatos da comunidade.
2. Preencha os filtros desejados ou consulte todas as localidades.
3. Abra uma ocorrência para ver os detalhes e o histórico de acompanhamento.
4. Para participar, use **Entrar** ou **Conta** e crie sua conta do Bairro em Dia.
5. Escolha **Registrar problema**, preencha as informações e toque em **Enviar relato**.
6. Acompanhe e edite seus registros em **Meus relatos**. Nos relatos de outras pessoas, use **Também sou afetado** quando o problema também afetar você.
7. Para mudar a aparência, use o controle de tema no cabeçalho.

O cadastro público cria um perfil **Morador**. Quem já possui uma conta habilitada como **Gestor** pode acessar **Gerenciar ocorrências** pela tela **Minha conta**.
