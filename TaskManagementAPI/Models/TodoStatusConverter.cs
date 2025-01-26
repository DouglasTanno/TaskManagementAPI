using System.Text.Json;
using System.Text.Json.Serialization;

namespace TaskManagementAPI.Models
{
    public class TodoStatusConverter : JsonConverter<TodoStatus>
    {
        public override TodoStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string value = reader.GetString();
            return value switch
            {
                "Pendente" => TodoStatus.Pendente,
                "Em Andamento" => TodoStatus.EmAndamento,
                "Concluída" => TodoStatus.Concluida,
                _ => throw new JsonException($"Status inválido: {value}")
            };
        }

        public override void Write(Utf8JsonWriter writer, TodoStatus value, JsonSerializerOptions options)
        {
            string stringValue = value switch
            {
                TodoStatus.Pendente => "Pendente",
                TodoStatus.EmAndamento => "Em Andamento",
                TodoStatus.Concluida => "Concluída",
                _ => throw new JsonException($"Status inválido: {value}")
            };

            writer.WriteStringValue(stringValue);
        }
    }
}
