using System.Text.Json.Serialization;
using TodoApi.Interfaces;

namespace TodoApi.Models;

public class TodoItem : IEntityTimestamps
{
    //Todo: Please Use Required  specifier
    public int Id { get; set; }

    public bool IsCompleted { get; set; }

    public string Text { get; set; } = null!;

    public string UserId { get; set; } = null!;

    [JsonIgnore]
    public ApplicationUser User { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }
}
