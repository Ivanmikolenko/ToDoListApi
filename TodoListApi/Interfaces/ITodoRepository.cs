using TodoListApi.Models;

namespace TodoListApi.Interfaces
{
    public interface ITodoRepository
    {
        Task<List<TodoItem>> GetAllAsync();
        Task<TodoItem?> GetByIdAsync(int id);
        Task<TodoItem> AddAsync(TodoItem item);
        Task<TodoItem?> UpdateAsync(int id, TodoItem item);
        Task<bool> DeleteAsync(int id);
        Task<bool> BulkInsertAsync(List<TodoItem> items);
        Task<bool> ClearAllAsync();
    }
}