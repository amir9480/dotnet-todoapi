using System.ComponentModel.DataAnnotations;

namespace TodoApi.ResourceModels;

public class NewTodoRequest
{
    [Required] public required string Text { get; set; }
}