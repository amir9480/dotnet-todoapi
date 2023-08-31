using System.ComponentModel.DataAnnotations;

namespace TodoApi.ResourceModels;

public class NewTodoRequest
{
    [Required] public string Text { get; set; } = null!;
}