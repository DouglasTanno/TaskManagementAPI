using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TaskManagementAPI.DTO;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodosController : ControllerBase
    {
        private readonly ITodoService _todoService;

        public TodosController(ITodoService todoService)
        {
            _todoService = todoService;
        }

        // GET: api/todos
        [HttpGet]
        [SwaggerOperation("Retorna todas as tarefas ou filtra por status", "Retorna uma lista de todas as tarefas ou filtra com base no status informado.")]
        [Authorize]
        public IActionResult GetTodos([FromQuery] string? status)
        {
            if (!string.IsNullOrEmpty(status))
            {
                try
                {
                    var parsedStatus = JsonSerializer.Deserialize<TodoStatus>($"\"{status}\"", new JsonSerializerOptions
                    {
                        Converters = { new TodoStatusConverter() }
                    });

                    return Ok(_todoService.GetTodosByStatus(parsedStatus));
                }
                catch (JsonException)
                {
                    return BadRequest(new ErrorResponse
                    {
                        Message = $"Status inválido: {status}. Valores permitidos: Pendente, Em Andamento, Concluída."
                    });
                }
            }

            return Ok(_todoService.GetAllTodos());
        }

        // GET: api/todos/{id}
        [HttpGet("{id}")]
        [SwaggerOperation("Retorna uma tarefa por ID", "Retorna uma tarefa específica com base no ID fornecido.")]
        [Authorize]
        public IActionResult GetTodoById(int id)
        {
            var todo = _todoService.GetAllTodos().FirstOrDefault(t => t.Id == id);
            return todo != null ? Ok(todo) : NotFound();
        }

        // POST: api/todos
        [HttpPost]
        [SwaggerOperation("Cria uma nova tarefa", "Cria uma nova tarefa no sistema com o título e descrição.")]
        [Authorize]
        public IActionResult CreateTodo([FromBody] TodoCreateDTO todoCreateDTO)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var authenticatedUserId))
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(todoCreateDTO.Title) || string.IsNullOrWhiteSpace(todoCreateDTO.Description))
            {
                throw new ArgumentException("Título e descrição são obrigatórios.");
            }

            var createdTodo = _todoService.AddTodo(todoCreateDTO, authenticatedUserId);

            return CreatedAtAction(nameof(GetTodoById), new { id = createdTodo.Id }, createdTodo);
        }

        // PUT: api/todos/{id}
        [HttpPut("{id}")]
        [SwaggerOperation("Atualiza uma tarefa", "Atualiza uma tarefa existente com base no ID fornecido.")]
        [Authorize]
        public IActionResult UpdateTodo(int id, [FromBody] TodoUpdateDTO todoUpdateDTO)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var authenticatedUserId))
            {
                return Unauthorized();
            }

            var todo = _todoService.GetAllTodos().FirstOrDefault(t => t.Id == id);
            if (todo == null)
            {
                return NotFound();
            }

            if (todo.UserId != authenticatedUserId)
            {
                return Forbid();
            }

            _todoService.UpdateTodo(todo, todoUpdateDTO);
            return Ok(todo);
        }

        // DELETE: api/todos/{id}
        [HttpDelete("{id}")]
        [SwaggerOperation("Deleta uma tarefa", "Deleta uma tarefa com base no ID fornecido.")]
        [Authorize]
        public IActionResult DeleteTodo(int id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var authenticatedUserId))
            {
                return Unauthorized();
            }

            var todo = _todoService.GetAllTodos().FirstOrDefault(t => t.Id == id);
            if (todo == null)
            {
                return NotFound();
            }

            if (todo.UserId != authenticatedUserId)
            {
                return Forbid();
            }

            _todoService.RemoveTodo(todo, authenticatedUserId);
            return NoContent();
        }
    }

    public class ErrorResponse
    {
        public string Message { get; set; }
    }
}
