using System.ComponentModel.DataAnnotations;

namespace TodoApi.ResourceModels;

public class UpdateTodoRequest
{
    [Required] public required string Text { get; set; }
}
