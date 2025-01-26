using System.Text.Json.Serialization;

namespace TaskManagementAPI.Models
{
    [JsonConverter(typeof(TodoStatusConverter))]
    public enum TodoStatus
    {
        Pendente,
        EmAndamento,
        Concluida
    }
}
