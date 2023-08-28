using System.ComponentModel.DataAnnotations;

namespace TodoApi.ResourceModels;

public class UpdateTodoRequest
{
    [Required]
    public string Text { get; set; } = null!;
}
