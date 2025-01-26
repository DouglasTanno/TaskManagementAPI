using TaskManagementAPI.Data;
using TaskManagementAPI.DTO;
using TaskManagementAPI.Models;

public class TodoServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly AppDbContext _dbContext;
    private readonly TodoService _service;

    public TodoServiceTests(DatabaseFixture fixture)
    {
        _dbContext = fixture.DbContext;
        _service = fixture.TodoService;
    }

    [Fact]
    public void GetAllTodos_DeveRetornarTodosComSucesso()
    {
        var expectedCount = _dbContext.Todos.Count();

        var todos = _service.GetAllTodos();

        Assert.Equal(expectedCount, todos.Count());
    }


    [Fact]
    public void GetAllTodos_DeveRetornarTodosOrdenadosPorId()
    {
        var todos = _service.GetAllTodos();

        var todosList = todos.ToList();

        Assert.True(todosList[0].Id < todosList[1].Id);
        Assert.True(todosList[1].Id < todosList[2].Id);
    }

    [Fact]
    public void GetTodosByStatus_DeveRetornarTodosComStatusCorreto()
    {
        var status = TodoStatus.Pendente;

        var expectedCount = _dbContext.Todos.Count(t => t.Status == status);

        var todos = _service.GetTodosByStatus(status);

        Assert.All(todos, t => Assert.Equal(status, t.Status));
        Assert.Equal(expectedCount, todos.Count());
    }

    [Fact]
    public void GetTodosByStatus_DeveRetornarApenasTodosComStatusPendente()
    {
        var status = TodoStatus.Pendente;

        var todos = _service.GetTodosByStatus(status);

        Assert.All(todos, t => Assert.Equal(status, t.Status));

        var expectedCount = _dbContext.Todos.Count(t => t.Status == status);
        Assert.Equal(expectedCount, todos.Count());
    }


    [Fact]
    public void AddTodo_DeveAdicionarTodoMantendoIntegridadeComSucesso()
    {
        var lastTodoId = _dbContext.Todos.Max(t => t.Id);

        var todoDto = new TodoCreateDTO
        {
            Title = "Título task teste",
            Description = "Descrição da task teste"
        };

        _service.AddTodo(todoDto, 1);

        var todo = _dbContext.Todos.FirstOrDefault(t => t.Title == "Título task teste");

        Assert.NotNull(todo);
        Assert.Equal(lastTodoId + 1, todo.Id);
        Assert.Equal("Descrição da task teste", todo.Description);
        Assert.Equal(DateOnly.FromDateTime(DateTime.Now), todo.CreatedAt);
        Assert.Equal(1, todo.UserId);
    }

    [Fact]
    public void AddTodo_DeveDefinirStatusPendentePorPadrao()
    {
        var todoDto = new TodoCreateDTO
        {
            Title = "Título task sem status",
            Description = "Descrição da task sem status"
        };

        var todo = _service.AddTodo(todoDto, 1);

        Assert.Equal(TodoStatus.Pendente, todo.Status);
    }

    [Fact]
    public void RemoveTodo_DeveRemoverComSucesso()
    {
        var todoDto = new TodoCreateDTO
        {
            Title = "Título task para remoção",
            Description = "Descrição task para remoção"
        };
        _service.AddTodo(todoDto, 1);

        var todo = _dbContext.Todos.FirstOrDefault(t => t.Title == "Título task para remoção");

        Assert.NotNull(todo);

        _service.RemoveTodo(todo, 1);

        var removedTodo = _dbContext.Todos.FirstOrDefault(t => t.Id == todo.Id);

        Assert.Null(removedTodo);
    }

    [Fact]
    public void UpdateTodo_DeveAtualizarTituloEDescricaoComSucesso()
    {
        var todoDto = new TodoCreateDTO
        {
            Title = "Título Original",
            Description = "Descrição Original"
        };
        _service.AddTodo(todoDto, 1);

        var todo = _dbContext.Todos.FirstOrDefault(t => t.Title == "Título Original");

        Assert.NotNull(todo);

        var todoUpdateDTO = new TodoUpdateDTO
        {
            Title = "Novo Título",
            Description = "Nova Descrição"
        };

        _service.UpdateTodo(todo, todoUpdateDTO);

        var updatedTodo = _dbContext.Todos.FirstOrDefault(t => t.Id == todo.Id);
        Assert.Equal("Novo Título", updatedTodo.Title);
        Assert.Equal("Nova Descrição", updatedTodo.Description);
    }

    [Fact]
    public void UpdateTodo_DeveAtualizarStatusComSucesso()
    {
        var todoDto = new TodoCreateDTO
        {
            Title = "Título Original",
            Description = "Descrição Original"
        };
        _service.AddTodo(todoDto, 1);

        var todo = _dbContext.Todos.FirstOrDefault(t => t.Title == "Título Original");

        Assert.NotNull(todo);

        var todoUpdateDTO = new TodoUpdateDTO
        {
            Status = TodoStatus.Concluida
        };

        _service.UpdateTodo(todo, todoUpdateDTO);

        var updatedTodo = _dbContext.Todos.FirstOrDefault(t => t.Id == todo.Id);
        Assert.Equal(TodoStatus.Concluida, updatedTodo.Status);
    }

    [Fact]
    public void UpdateTodo_DeveAtualizarTodosCamposComSucesso()
    {
        var todoDto = new TodoCreateDTO
        {
            Title = "Título Original",
            Description = "Descrição Original"
        };
        _service.AddTodo(todoDto, 1);

        var todo = _dbContext.Todos.FirstOrDefault(t => t.Title == "Título Original");

        Assert.NotNull(todo);

        var todoUpdateDTO = new TodoUpdateDTO
        {
            Title = "Novo Título",
            Description = "Nova Descrição",
            Status = TodoStatus.Pendente
        };

        _service.UpdateTodo(todo, todoUpdateDTO);

        var updatedTodo = _dbContext.Todos.FirstOrDefault(t => t.Id == todo.Id);
        Assert.Equal("Novo Título", updatedTodo.Title);
        Assert.Equal("Nova Descrição", updatedTodo.Description);
        Assert.Equal(TodoStatus.Pendente, updatedTodo.Status);
    }

    [Fact]
    public void UpdateTodo_NaoDeveAlterarQuandoValoresNaoForemPassados()
    {
        var todoDto = new TodoCreateDTO
        {
            Title = "Título Original",
            Description = "Descrição Original"
        };
        _service.AddTodo(todoDto, 1);

        var todo = _dbContext.Todos.FirstOrDefault(t => t.Title == "Título Original");

        Assert.NotNull(todo);

        var todoUpdateDTO = new TodoUpdateDTO();

        _service.UpdateTodo(todo, todoUpdateDTO);

        var updatedTodo = _dbContext.Todos.FirstOrDefault(t => t.Id == todo.Id);
        Assert.Equal("Título Original", updatedTodo.Title);
        Assert.Equal("Descrição Original", updatedTodo.Description);
        Assert.Equal(TodoStatus.Pendente, updatedTodo.Status);
    }

    [Fact]
    public void UpdateTodo_NaoDeveAlterarCamposQuandoValoresForemNulos()
    {
        var todoDto = new TodoCreateDTO
        {
            Title = "Título Original",
            Description = "Descrição Original"
        };
        _service.AddTodo(todoDto, 1);

        var todo = _dbContext.Todos.FirstOrDefault(t => t.Title == "Título Original");

        Assert.NotNull(todo);

        var todoUpdateDTO = new TodoUpdateDTO
        {
            Title = null,
            Description = null,
            Status = null
        };

        _service.UpdateTodo(todo, todoUpdateDTO);

        var updatedTodo = _dbContext.Todos.FirstOrDefault(t => t.Id == todo.Id);
        Assert.Equal("Título Original", updatedTodo.Title);
        Assert.Equal("Descrição Original", updatedTodo.Description);
        Assert.Equal(TodoStatus.Pendente, updatedTodo.Status);
    }

    [Fact]
    public void AddTodo_DeveSerEficienteCriandoMultiplasTarefas()
    {
        int numeroDeTarefas = 500;
        var todoDto = new TodoCreateDTO
        {
            Title = "Título task de desempenho",
            Description = "Descrição da task de desempenho"
        };

        var startTime = DateTime.Now;

        for (int i = 0; i < numeroDeTarefas; i++)
        {
            _service.AddTodo(todoDto, 1);
        }

        var endTime = DateTime.Now;
        var duration = endTime - startTime;

        Console.WriteLine($"Tempo para criar {numeroDeTarefas} tarefas: {duration.TotalMilliseconds} ms");

        Assert.True(duration.TotalMilliseconds < 1000, "A criação de múltiplas tarefas demorou mais que o esperado.");
    }


}
