# TaskManagementAPI
O TaskManagementAPI é uma API RESTful desenvolvida em .NET para gerenciar uma lista de tarefas. Ele oferece operações CRUD (Create, Read, Update, Delete), autenticação/autorização e listagem de tarefas.

## Introdução

O Task Management API é uma API RESTful criada em .NET para o gerenciamento de tarefas. Ela inclui funcionalidades como:

- Criação, leitura, atualização e remoção de tarefas.

- Autenticação e autorização baseada em JWT (JSON Web Token).

- Documentação via Swagger.

Este documento detalha a estrutura do projeto e explica como executar o projeto e como interagir com seus endpoints.

## Estrutura do Projeto

O projeto está organizado nos seguintes pacotes:

- **Config:** Configurações gerais do projeto, incluindo serviços e middlewares.

- **Controller:** Controladores para definir as rotas e pontos de entrada da API.

- **DTO (Data Transfer Objects):** Classes para transferir dados entre diferentes camadas da aplicação.

- **Models:** Modelos representando as entidades principais, como Todo e User.

- **Serialization:** Configurações de serialização/deserialização, como conversores customizados.

- **Services:** Camada de lógica de negócios que realiza operações específicas relacionadas às entidades.

## Banco de Dados

O banco de dados da API é gerenciado com o Entity Framework Core In-Memory, onde os dados estão disponíveis apenas em tempo de execução. 
Já existem _seeds_ de registros de 3 tarefas e de um usuário superuser, que deve ser utilizado para cadastrar novos usuários.

## Instruções para Execução

Compilar e executar o projeto TaskManagementAPI no Visual Studio. Ao ser executado, será carregada automaticamente a página do Swagger com os endpoints em http://localhost:5233/swagger/ .

## Auth

O AuthController gerencia a autenticação e o cadastro de usuários, considerados como `User`. Todas as operações necessitam que o usuário esteja autenticado.

Cada usuário possui os atributos `Id` (inteiro identidade), `Username` (nome escolhido pelo usuário), `Password` (senha escolhida pelo usuário) e `IsSuperUser` (flag que determina se o usuário pode ou não cadastrar novos usuários).

Se encontra disponível o usuário "su" (de superuser), que possui a flag `IsSUperUser` como **true**.

### POST /Auth/login

Operação de login, utilizada para obter o token de autorização do usuário.
Deve ser utilizado para logar o usuário na API com as credenciais `Username` e `Password`.
O sistema apenas possui cadastrado o usuário "su". 

**Condições:**

- O usuário deve estar cadastrado no sistema.

1. Envie uma requisição POST com as credenciais de usuário:
```json
{
    "username": "su",
    "password": "password"
}
```
2. A resposta conterá um token no formato:
```json
{
    "token": "{seu-token-jwt}"
}
```
3. Em Authorize, inserir o token retornado anteriormente, adicionando o prefixo "Bearer", no seguinte formato
   `Bearer {seu-token-jwt}`.
    Em seguida,  pressione Authorize. Com isso, o usuário se encontrará autenticado na API e será permitido realizar as outras operações. Caso contrário, é retornado como _Unauthorized_.

### POST /Auth/register

Operação de cadastro de novos usuários.

**Condições:**

 - Disponível apenas para usuários autenticados e que possuam a flag `IsSuperUser` como **true**.

1. Envie uma requisição POST com os parâmetros do novo usuário:
```json
{
    "username": "string",
    "password": "string",
    "issuperuser": true
}
```
2. Antes de acessar a API com o novo usuário cadastrado, deve ser realizado o logout em Authorize.

### Todos

O TodosController gerencia as operações relacionadas às tarefas, que são chamadas de `Todo` (do inglês, a fazer). 
O objeto **Todo** possui os atributos `Id` (inteiro identidade), `Title` (título da tarefa), `Description` (descrição da tarefa), `CreatedAt` (data de criação da tarefa no formado **YYYY-MM-DD**) e `UserID` (ID do usuário que criou a tarefa).

Abaixo se encontram os endpoints e suas condições:

### GET /api/Todos

Operação que retorna a lista de todas as tarefas cadastradas, caso não seja inserido nenhum parâmetro. Também aceita o parâmetro de `Status`, retornando a lista de todas as tarefas cadastradas filtradas por determinado status.
Os status existentes são: **Pendente**, **Em Andamento** e **Concluída**.

**Condições:**

- Disponível apenas para usuários autenticados.
- São retornadas as tarefas, independente do usuário.
- Só são aceitos os parâmetros de status de filtro no formado "Pendente", "Em Andamento" ou "Concluída".

**Exemplo de Resposta:**
```json
[
  {
    "id": 1,
    "title": "Exemplo de Tarefa 1",
    "description": "Descrição da primeira tarefa",
    "createdAt": "2023-12-25",
    "status": "Em Andamento",
    "userId": 1
  },
  {
    "id": 2,
    "title": "Exemplo de Tarefa 2",
    "description": "Descrição da segunda tarefa",
    "createdAt": "2024-01-01",
    "status": "Pendente",
    "userId": 1
  },
  {
    "id": 3,
    "title": "Exemplo de Tarefa 3",
    "description": "Descrição da terceira tarefa",
    "createdAt": "2024-02-15",
    "status": "Concluída",
    "userId": 1
  }
]
```

### GET /api/Todos/{id}

Operação que retorna os detalhes de uma tarefa específica com base em seu `Id`.

**Condições:**

- Disponível apenas para usuários autenticados.

**Exemplo Resposta:**
```json
{
    "id": 1,
    "title": "Minha nova tarefa",
    "description": "Descrição da tarefa",
    "createdAt": "2025-01-01",
    "status": "Pendente",
    "userId": 1
}
```

### POST /api/Todos

Operação que realiza o cadastro de uma nova tarefa.

São preenchidos automaticamente os parâmetros `Id` (identidade), `CreatedAt` (data atual) e `UserId` (ID do usuário autenticado).

**Condições:**

- Disponível apenas para usuários autenticados.

**Corpo da requisição:**
```json
{
    "title": "Minha nova tarefa",
    "description": "Descrição da tarefa"
}
```

**Exemplo Resposta:**
```json
{
    "id": 1,
    "title": "Minha nova tarefa",
    "description": "Descrição da tarefa",
    "createdAt": "2025-01-01",
    "status": "Pendente",
    "userId": 1
}
```

### PUT /api/Todos/{id}

Operação que atualiza uma tarefa já existente através de seu `Id`.

**Condições:**

- Disponível apenas para usuários autenticados.
- Apenas é permitida a atualização de tarefas criadas pelo usuário autenticado.
- É possível atualizar os atributos `Title`, `Description` e/ou `Status`. Caso algum deles não seja preenchido na requisição, apenas serão atualizados os que forem preenchidos. Os outros atributos se mantém imutáveis.

**Corpo da requisição:**
```json
{
    "title": "Minha tarefa atualizada",
    "description": "Descrição da tarefa atualizada",
    "status": "Concluída"
}
```

**Exemplo Resposta:**
```json
{
    "id": 1,
    "title": "Minha tarefa atualizada",
    "description": "Descrição da tarefa atualizada",
    "createdAt": "2025-01-01",
    "status": "Concluída",
    "userId": 1
}
```

### DELETE /api/Todos/{id}

Exclui uma tarefa já existente através de seu `Id`. 

**Condições:**

- Disponível apenas para usuários autenticados.
- Apenas é permitida a exclusão de tarefas criadas pelo usuário autenticado.

## Testes Unitários

Os testes estão localizados no projeto `TaskManagementAPITests`.

Estão incluídas validações unitárias e testes de integração para os serviços e endopoints da API, já englobando as regras de negócio e validações dos objetos existente.

Os testes podem ser executados pressionando o botão direito em cima do projeto, em "Executar Testes".

## Considerações Finais

Este projeto oferece uma solução simples e escalável para gerenciar tarefas, com foco em boas práticas RESTful e separação de responsabilidades. Use os testes para validar funcionalidades e explore os endpoints pelo Swagger.
Essa API foi projetada para ser simples de usar e pode ser expandida com novas funcionalidades conforme necessário. 
Para qualquer dúvida, entre em contato:

**Contato:**

- Nome: Douglas Rorie Tanno
- Email: douglas.tanno@gmail.com
