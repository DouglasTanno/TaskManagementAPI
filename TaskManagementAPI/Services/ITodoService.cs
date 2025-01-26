using TaskManagementAPI.DTO;
using TaskManagementAPI.Models;

public interface ITodoService
{
    IEnumerable<Todo> GetAllTodos();
    IEnumerable<Todo> GetTodosByStatus(TodoStatus status);
    Todo AddTodo(TodoCreateDTO todoUpdateDTO, int userId);
    void RemoveTodo(Todo todo, int userId);
    void UpdateTodo(Todo todo, TodoUpdateDTO todoCreateDTO);
    void CreateTodoExamples();
}