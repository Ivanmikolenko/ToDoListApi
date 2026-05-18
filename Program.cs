using Microsoft.EntityFrameworkCore;
using TodoListApi.Data;
using TodoListApi.Interfaces;
using TodoListApi.Repositories;
using TodoListApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TodoContext>(options =>
    options.UseSqlite("Data Source=todolist.db"));

builder.Services.AddScoped<ITodoRepository, TodoRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    Console.WriteLine($"→ {DateTime.Now:HH:mm:ss} {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"← {DateTime.Now:HH:mm:ss} {context.Request.Method} {context.Request.Path} → {context.Response.StatusCode}");
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TodoContext>();
    
    context.Database.Migrate();
    
    if (!context.TodoItems.Any())
    {
        context.TodoItems.AddRange(
            new TodoItem { Text = "Learn ASP.NET Core", IsCompleted = true },
            new TodoItem { Text = "Build Todo API", IsCompleted = false },
            new TodoItem { Text = "Test the API endpoints", IsCompleted = false }
        );
        context.SaveChanges();
        Console.WriteLine("Initial data seeded.");
    }
    else
    {
        var count = context.TodoItems.Count();
        Console.WriteLine($"Database contains {count} existing todo items.");
    }
}

var port = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://+:8080";
Console.WriteLine($"🚀 Server starting on: {port}");
Console.WriteLine($"📊 Swagger UI: {port}/swagger");
app.Urls.Add(port);
app.Run();