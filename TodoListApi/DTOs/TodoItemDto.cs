// TodoItemDto.cs
namespace TodoListApi.DTOs
{
    public class TodoItemDto
    {
        public int Id { get; set; }
        public string? Text { get; set; }
        public bool Completed { get; set; }
        public bool Editing { get; set; } = false; // Добавляем для синхронизации с фронтендом
    }

    public class CreateTodoItemDto
    {
        public string? Text { get; set; }
        public bool Completed { get; set; } = false;
    }

    public class UpdateTodoItemDto
    {
        public string? Text { get; set; }
        public bool Completed { get; set; }
    }

    public class BulkInsertDto
    {
        public List<CreateTodoItemDto>? Items { get; set; }
    }

    public class ExternalTodoItemDto
    {
        public string? Text { get; set; }
        public bool Completed { get; set; }
        public bool Editing { get; set; }
    }
}