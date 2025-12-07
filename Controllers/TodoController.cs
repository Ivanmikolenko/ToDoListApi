// TodoController.cs
using Microsoft.AspNetCore.Mvc;
using TodoListApi.DTOs;
using TodoListApi.Interfaces;
using TodoListApi.Models;

namespace TodoListApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoController : ControllerBase
    {
        private readonly ITodoRepository _repository;
        private readonly ILogger<TodoController> _logger;

        public TodoController(ITodoRepository repository, ILogger<TodoController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItemDto>>> GetTodoItems()
        {
            _logger.LogInformation("📥 GET /api/todo - Getting all todo items");
            try
            {
                var items = await _repository.GetAllAsync();
                var dtos = items.Select(item => new TodoItemDto
                {
                    Id = item.Id,
                    Text = item.Text,
                    Completed = item.IsCompleted
                });
                _logger.LogInformation($"📤 GET /api/todo - Returning {dtos.Count()} items");
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ GET /api/todo - Error getting todo items");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("bulk-web")]
        public async Task<ActionResult<BulkOperationResult>> BulkInsert(List<ExternalTodoItemDto> externalItems)
        {
            _logger.LogInformation($"📥 POST /api/todo/bulk-web - Received {externalItems?.Count ?? 0} items");

            if (externalItems == null || !externalItems.Any())
            {
                _logger.LogWarning("⚠️ POST /api/todo/bulk-web - No items provided");
                return BadRequest("No items provided");
            }

            try
            {
                _logger.LogInformation("🧹 Clearing all existing items");
                await _repository.ClearAllAsync();

                var todoItems = externalItems.Where(dto => !string.IsNullOrWhiteSpace(dto.Text))
                                           .Select(dto => new TodoItem
                                           {
                                               Text = dto.Text.Trim(),
                                               IsCompleted = dto.Completed
                                           }).ToList();

                _logger.LogInformation($"💾 Bulk inserting {todoItems.Count} items");
                await _repository.BulkInsertAsync(todoItems);

                _logger.LogInformation($"✅ POST /api/todo/bulk-web - Successfully synced {todoItems.Count} items");
                return Ok(new BulkOperationResult
                {
                    Message = "Items synced successfully",
                    Count = todoItems.Count,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ POST /api/todo/bulk-web - Error during sync");
                return StatusCode(500, new BulkOperationResult
                {
                    Message = $"Error during sync: {ex.Message}",
                    Count = 0,
                    Success = false
                });
            }
        }

        [HttpPost("bulk")]
        public async Task<ActionResult<BulkOperationResult>> BulkInsertOldFormat(BulkInsertDto bulkDto)
        {
            if (bulkDto?.Items == null || !bulkDto.Items.Any())
            {
                return BadRequest("No items provided");
            }

            try
            {
                await _repository.ClearAllAsync();
                var todoItems = bulkDto.Items.Where(dto => !string.IsNullOrWhiteSpace(dto.Text))
                                           .Select(dto => new TodoItem
                                           {
                                               Text = dto.Text.Trim(),
                                               IsCompleted = dto.Completed
                                           }).ToList();

                await _repository.BulkInsertAsync(todoItems);
                return Ok(new BulkOperationResult
                {
                    Message = "Items added successfully",
                    Count = todoItems.Count,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BulkOperationResult
                {
                    Message = $"Error during bulk insert: {ex.Message}",
                    Count = 0,
                    Success = false
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodoItem(int id)
        {
            var result = await _repository.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAllTodoItems()
        {
            await _repository.ClearAllAsync();
            return NoContent();
        }
        
        [HttpPost]
        public async Task<ActionResult<TodoItemDto>> CreateTodoItem(CreateTodoItemDto createDto)
        {
            _logger.LogInformation("📥 POST /api/todo - Creating new todo item");
            
            if (string.IsNullOrWhiteSpace(createDto.Text))
            {
                return BadRequest("Text is required");
            }

            try
            {
                var todoItem = new TodoItem
                {
                    Text = createDto.Text.Trim(),
                    IsCompleted = createDto.Completed
                };

                var createdItem = await _repository.AddAsync(todoItem);
                
                var resultDto = new TodoItemDto
                {
                    Id = createdItem.Id,
                    Text = createdItem.Text,
                    Completed = createdItem.IsCompleted
                };

                _logger.LogInformation($"✅ POST /api/todo - Created item with ID {createdItem.Id}");
                return CreatedAtAction(nameof(GetTodoItems), resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ POST /api/todo - Error creating todo item");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class BulkOperationResult
    {
        public string Message { get; set; } = string.Empty;
        public int Count { get; set; }
        public bool Success { get; set; }
    }
}