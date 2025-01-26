using TaskManagementAPI.Data;
using TaskManagementAPI.DTO;
using TaskManagementAPI.Models;

public class TodoService : ITodoService
{
    private readonly AppDbContext _dbContext;

    public TodoService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IEnumerable<Todo> GetAllTodos()
    {
        return _dbContext.Todos
        .Select(t => new Todo
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            CreatedAt = t.CreatedAt,
            Status = t.Status,
            UserId = t.UserId
        })
        .OrderBy(t => t.Id)
        .ToList();
    }

    public IEnumerable<Todo> GetTodosByStatus(TodoStatus status)
    {
        return _dbContext.Todos.Where(t => t.Status == status).ToList();
    }

    public Todo AddTodo(TodoCreateDTO todoCreateDTO, int userId)
    {
        var maxId = _dbContext.Todos.Any() ? _dbContext.Todos.Max(t => t.Id) : 0;

        Todo todo = new Todo
        {
            Id = maxId + 1,
            Title = todoCreateDTO.Title,
            Description = todoCreateDTO.Description,
            CreatedAt = DateOnly.FromDateTime(DateTime.Now),
            Status = TodoStatus.Pendente,
            UserId = userId
        };

        _dbContext.Todos.Add(todo);
        _dbContext.SaveChanges();

        return todo;
    }

    public void RemoveTodo(Todo todo, int userId)
    {
        _dbContext.Todos.Remove(todo);
        _dbContext.SaveChanges();
    }

    public void UpdateTodo(Todo todo, TodoUpdateDTO todoUpdateDTO)
    {
        if (!string.IsNullOrEmpty(todoUpdateDTO.Title))
        {
            todo.Title = todoUpdateDTO.Title;
        }

        if (!string.IsNullOrEmpty(todoUpdateDTO.Description))
        {
            todo.Description = todoUpdateDTO.Description;
        }

        if (todoUpdateDTO.Status.HasValue)
        {
            todo.Status = todoUpdateDTO.Status.Value;
        }

        _dbContext.Todos.Update(todo);
        _dbContext.SaveChanges();
    }

    public void CreateTodoExamples()
    {
        var todos = new List<Todo>
            {
                new Todo
                {
                    Title = "Exemplo de Tarefa 1",
                    Description = "Descrição da primeira tarefa",
                    CreatedAt = new DateOnly(2023, 12, 25), 
                    Status = TodoStatus.EmAndamento,
                    UserId = 1
                },
                new Todo
                {
                    Title = "Exemplo de Tarefa 2",
                    Description = "Descrição da segunda tarefa",
                    CreatedAt = new DateOnly(2024, 01, 01),
                    Status = TodoStatus.Pendente,
                    UserId = 1
                },
                new Todo
                {
                    Title = "Exemplo de Tarefa 3",
                    Description = "Descrição da terceira tarefa",
                    CreatedAt = new DateOnly(2024, 02, 15),
                    Status = TodoStatus.Concluida,
                    UserId = 1
                }
        };

        _dbContext.Todos.AddRange(todos);
        _dbContext.SaveChanges();
    }
}
