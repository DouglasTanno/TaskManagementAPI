using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskManagementAPI.Controllers;
using TaskManagementAPI.DTO;
using TaskManagementAPI.Models;

public class TodosControllerTests
{
    private readonly Mock<ITodoService> _mockTodoService;
    private readonly TodosController _controller;

    public TodosControllerTests()
    {
        _mockTodoService = new Mock<ITodoService>();
        _controller = new TodosController(_mockTodoService.Object);
    }

    private void SetUserContext(string userId)
    {
        var userClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };
        var userContext = new ClaimsPrincipal(new ClaimsIdentity(userClaims));

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = userContext }
        };
    }

    [Fact]
    public void GetTodos_DeveRetornarTodosComSucesso()
    {
        var todos = new List<Todo>
        {
            new Todo { Id = 1, Title = "Tarefa Teste 1" },
            new Todo { Id = 2, Title = "Tarefa Teste 2" }
        };
        _mockTodoService.Setup(service => service.GetAllTodos()).Returns(todos);

        var result = _controller.GetTodos(null) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(todos, result.Value);
    }

    [Fact]
    public void GetTodoById_QuandoTarefaExistir_DeveRetornarTarefa()
    {
        var todo = new Todo { Id = 1, Title = "Tarefa Teste" };
        _mockTodoService.Setup(service => service.GetAllTodos()).Returns(new List<Todo> { todo });

        var result = _controller.GetTodoById(1) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(todo, result.Value);
    }

    [Fact]
    public void GetTodoById_QuandoTarefaNaoExistir_DeveRetornarNaoEncontrado()
    {
        _mockTodoService.Setup(service => service.GetAllTodos()).Returns(new List<Todo>());

        var result = _controller.GetTodoById(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void CreateTodo_QuandoNaoAutenticado_DeveRetornarNaoAutorizado()
    {
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext()
        };

        var todoCreateDTO = new TodoCreateDTO
        {
            Title = "Título da tarefa",
            Description = "Descrição da tarefa"
        };

        var result = _controller.CreateTodo(todoCreateDTO);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void CreateTodo_QuandoDadosValidos_DeveRetornarCriado()
    {
        var todoCreateDTO = new TodoCreateDTO { Title = "Nova Tarefa", Description = "Descrição da tarefa" };
        var userId = "1";
        SetUserContext(userId);

        var createdTodo = new Todo { Id = 1, Title = "Nova Tarefa", Description = "Descrição da tarefa" };
        _mockTodoService.Setup(service => service.AddTodo(todoCreateDTO, 1)).Returns(createdTodo);

        var result = _controller.CreateTodo(todoCreateDTO);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal("GetTodoById", createdResult.ActionName);
        Assert.Equal(createdTodo.Id, createdResult.RouteValues["id"]);
        Assert.Equal(createdTodo, createdResult.Value);
    }

    [Fact]
    public void CreateTodo_QuandoDadosInvalidos_DeveRetornarArgumentException()
    {
        var todoCreateDTO = new TodoCreateDTO { Title = "", Description = "Descrição da tarefa" };
        var userId = "1";
        SetUserContext(userId);

        var exception = Assert.Throws<ArgumentException>(() => _controller.CreateTodo(todoCreateDTO));

        Assert.Equal("Título e descrição são obrigatórios.", exception.Message);
    }

    [Fact]
    public void UpdateTodo_QuandoUsuarioNaoAutenticado_DeveRetornarUnauthorized()
    {
        var todoUpdateDTO = new TodoUpdateDTO { Title = "Tarefa Atualizada", Description = "Nova descrição" };
        var userContext = new ClaimsPrincipal(new ClaimsIdentity());
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = userContext }
        };

        var result = _controller.UpdateTodo(1, todoUpdateDTO);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void UpdateTodo_QuandoTarefaNaoEncontrada_DeveRetornarNotFound()
    {
        var todoUpdateDTO = new TodoUpdateDTO { Title = "Tarefa Atualizada", Description = "Nova descrição" };
        SetUserContext("1");

        _mockTodoService.Setup(service => service.GetAllTodos()).Returns(new List<Todo>());

        var result = _controller.UpdateTodo(1, todoUpdateDTO);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void UpdateTodo_QuandoUsuarioNaoForOProprietario_DeveRetornarForbid()
    {
        var todoUpdateDTO = new TodoUpdateDTO { Title = "Tarefa Atualizada", Description = "Nova descrição" };
        SetUserContext("1");

        var otherTodo = new Todo { Id = 1, Title = "Tarefa Existente", Description = "Descrição", UserId = 2 };
        _mockTodoService.Setup(service => service.GetAllTodos()).Returns(new List<Todo> { otherTodo });

        var result = _controller.UpdateTodo(1, todoUpdateDTO);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public void UpdateTodo_QuandoAtualizacaoComSucesso_DeveRetornarOk()
    {
        var todoUpdateDTO = new TodoUpdateDTO { Title = "Tarefa Atualizada", Description = "Nova descrição" };
        SetUserContext("1");

        var existingTodo = new Todo { Id = 1, Title = "Tarefa Existente", Description = "Descrição", UserId = 1 };
        _mockTodoService.Setup(service => service.GetAllTodos()).Returns(new List<Todo> { existingTodo });

        _mockTodoService.Setup(service => service.UpdateTodo(It.IsAny<Todo>(), It.IsAny<TodoUpdateDTO>()))
                        .Callback<Todo, TodoUpdateDTO>((todo, updateDTO) =>
                        {
                            todo.Title = updateDTO.Title;
                            todo.Description = updateDTO.Description;
                        });

        var result = _controller.UpdateTodo(1, todoUpdateDTO) as OkObjectResult;

        Assert.NotNull(result);
        var updatedTodo = result.Value as Todo;
        Assert.NotNull(updatedTodo);
        Assert.Equal("Tarefa Atualizada", updatedTodo.Title);
        Assert.Equal("Nova descrição", updatedTodo.Description);
    }

    [Fact]
    public void DeleteTodo_QuandoUsuarioNaoAutenticado_DeveRetornarUnauthorized()
    {
        var userContext = new ClaimsPrincipal(new ClaimsIdentity());
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = userContext }
        };

        var result = _controller.DeleteTodo(1);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public void DeleteTodo_QuandoTarefaNaoEncontrada_DeveRetornarNotFound()
    {
        SetUserContext("1");

        _mockTodoService.Setup(service => service.GetAllTodos()).Returns(new List<Todo>());

        var result = _controller.DeleteTodo(1);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void DeleteTodo_QuandoUsuarioNaoForOProprietario_DeveRetornarForbid()
    {
        SetUserContext("1");

        var otherTodo = new Todo { Id = 1, Title = "Tarefa Existente", Description = "Descrição", UserId = 2 };
        _mockTodoService.Setup(service => service.GetAllTodos()).Returns(new List<Todo> { otherTodo });

        var result = _controller.DeleteTodo(1);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public void DeleteTodo_QuandoDelecaoComSucesso_DeveRetornarNoContent()
    {
        SetUserContext("1");

        var existingTodo = new Todo { Id = 1, Title = "Tarefa Existente", Description = "Descrição", UserId = 1 };
        _mockTodoService.Setup(service => service.GetAllTodos()).Returns(new List<Todo> { existingTodo });

        _mockTodoService.Setup(service => service.RemoveTodo(It.IsAny<Todo>(), It.IsAny<int>()))
                        .Verifiable();

        var result = _controller.DeleteTodo(1);

        Assert.IsType<NoContentResult>(result);
        _mockTodoService.Verify(service => service.RemoveTodo(existingTodo, 1), Times.Once);
    }
}
