using System.Text.Json.Serialization;
using TodoApi.Interfaces;

namespace TodoApi.Models;

public class TodoItem : IEntityTimestamps
{
    public int Id { get; set; }

    public bool IsCompleted { get; set; }

    public required string Text { get; set; }

    public required string UserId { get; set; }

    [JsonIgnore]
    public ApplicationUser? User { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }
}
