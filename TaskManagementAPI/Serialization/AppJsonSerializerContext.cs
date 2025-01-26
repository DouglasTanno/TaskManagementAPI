using System.Text.Json;
using System.Text.Json.Serialization;
using TaskManagementAPI.Controllers;
using TaskManagementAPI.DTO;
using TaskManagementAPI.Models;

[JsonSerializable(typeof(IEnumerable<Todo>))]
[JsonSerializable(typeof(Todo))]
[JsonSerializable(typeof(List<Todo>))]
[JsonSerializable(typeof(TodoStatus))]
[JsonSerializable(typeof(TodoStatusConverter))]
[JsonSerializable(typeof(JsonException))]
[JsonSerializable(typeof(LoginRequest))]
[JsonSerializable(typeof(JwtResponse))]
[JsonSerializable(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
[JsonSerializable(typeof(Microsoft.AspNetCore.Mvc.ValidationProblemDetails))]
[JsonSerializable(typeof(TodoUpdateDTO))]
[JsonSerializable(typeof(TodoCreateDTO))]
[JsonSerializable(typeof(UserCreateDTO))]
[JsonSerializable(typeof(ErrorResponse))]

internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
