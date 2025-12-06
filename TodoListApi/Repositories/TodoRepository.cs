using Microsoft.EntityFrameworkCore;
using TodoListApi.Data;
using TodoListApi.Interfaces;
using TodoListApi.Models;

namespace TodoListApi.Repositories
{
    public class TodoRepository : ITodoRepository
    {
        private readonly TodoContext _context;

        public TodoRepository(TodoContext context)
        {
            _context = context;
        }

        public async Task<bool> ClearAllAsync()
        {
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM TodoItems");
            return true;
        }
        public async Task<List<TodoItem>> GetAllAsync()
        {
            return await _context.TodoItems.ToListAsync();
        }
        public async Task<TodoItem?> GetByIdAsync(int id)
        {
            return await _context.TodoItems.FindAsync(id);
        }

        public async Task<TodoItem> AddAsync(TodoItem item)
        {
            _context.TodoItems.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<TodoItem?> UpdateAsync(int id, TodoItem item)
        {
            var existingItem = await _context.TodoItems.FindAsync(id);
            if (existingItem == null)
                return null;

            existingItem.Text = item.Text;
            existingItem.IsCompleted = item.IsCompleted;
            
            await _context.SaveChangesAsync();
            return existingItem;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _context.TodoItems.FindAsync(id);
            if (item == null)
                return false;

            _context.TodoItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BulkInsertAsync(List<TodoItem> items)
        {
            await _context.TodoItems.AddRangeAsync(items);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}