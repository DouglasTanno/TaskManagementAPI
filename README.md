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

O banco de dados da API é gerenciado por uma Entity Framework Core In-Memory, onde os dados estão disponíveis apenas em tempo de execução. 
Já existem _seeds_ de registros de 3 tarefas e de um usuário superuser, que deve ser utilizado para cadastrar novos usuários.

## Instruções para Execução

Executar o projeto TaskManagementAPI. Ao ser executado, será carregado automaticamente o Swagger com os endpoints em http://localhost:5233/swagger/ .

## Endpoints e Operações

## Auth

O AuthController gerencia a autenticação e o cadastro de usuários. Todas as operações necessitam que o usuário esteja autenticado.

Cada usuário possui os atributos `Id` (inteiro identidade), `Username` (nome escolhido pelo usuário), `Password` (senha escolhida pelo usuário) e `IsSuperUser` (flag que determina se o usuário pode ou não cadastrar novos usuários).

Se encontra disponível o usuário "su" (de superuser), que possui a flag `IsSUperUser` como **true**.

### POST /Auth/login

Deve ser utilizado para logar o usuário na API com as credenciais `Username` e `Password`.
O sistema apenas possui cadastrado o usuário "su". 

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
3. Em Authorize, inserir o token retornado anteriormente, junto do prefixo "Bearer", no formato **'Bearer {seu-token}'** e pressione Authorize. Com isso, o usuário se encontrará autenticado na API e será permitido realizar as outras operações. Caso contrário, é retornado como _Unauthorized_.

### POST /Auth/register

Ação disponível apenas para usuários autenticados e que possuam a flag `IsSuperUser` como **true**.

1. Envie uma requisição POST com os parâmetros do novo usuário:
```json
{
    "username": "string",
    "password": "string",
    "issuperuser": true
}
```
2. Antes de acessar a API com o novo usuário cadastrado, deve ser realizado o logout em Authorize.

### TodosController

O TodosController gerencia as operações relacionadas às tarefas, que são chamadas de `todo` (do inglês, a fazer). Abaixo estão os endpoints e suas condições:

**GET /api/todos**

Retorna a lista de todas as tarefas cadastradas. Também aceita o parâmetro de status como filtro, retornando a lista de todas as tarefas cadastradas com determinado status.
Os status existentes são: Pendente, Em Andamento e Concluída. 

- Condicionalidades:

  - As tarefas são retornadas apenas para usuários autenticados.
  - São retornadas as tarefas, independente do usuário.
  - Só são aceitos os parâmetros de status de filtro no formado "Pendente", "Em Andamento" ou "Concluída".

GET /api/todos/{id}

Retorna os detalhes de uma tarefa específica com base no ID.

Condicionalidades:

Verifica se a tarefa pertence ao usuário autenticado.

Retorna um erro 404 se a tarefa não for encontrada.

POST /api/todos

Cria uma nova tarefa.
