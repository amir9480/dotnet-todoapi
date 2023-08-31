using System.ComponentModel.DataAnnotations;

namespace TodoApi.ResourceModels;

public class UpdateTodoRequest
{
    //Todo: Please Use Required  specifier
    [Required] public string Text { get; set; } = null!;
}
