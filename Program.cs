using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using TodoStore.Models;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Todos") ?? "Data Source=Todos.db";
builder.Services.AddSqlite<TodoDb>(connectionString);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Todo API", Description = "Minimal API for Todos", Version = "v1" });
});
builder.Services.AddCors();
// builder.Services.AddDbContext<TodoDb>(options => options.UseInMemoryDatabase("items"));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API V1");
    c.RoutePrefix = string.Empty;
});

app.UseCors(builder => builder
.AllowAnyOrigin()
.AllowAnyMethod()
.AllowAnyHeader()
);


app.MapGet("/Todos", async (TodoDb db) => await db.Todos.ToListAsync());
app.MapGet("/Todo/{id}", async (TodoDb db, int id) => await db.Todos.FindAsync(id));
app.MapPost("/Todo", async (TodoDb db, Todo Todo) =>
{
    Todo.DateCreated = DateTime.Now;
    Todo.DateUpdated = DateTime.Now;
    Todo.Status = 1;
    await db.Todos.AddAsync(Todo);
    await db.SaveChangesAsync();
    return Results.Created($"/Todo/{Todo.Id}", Todo);
});
app.MapPost("/Todos/UpdateFromGrid", async (TodoDb db, BatchChangesRequest request) =>
{
    var todoList = new List<Todo>();
    foreach (var change in request.Changes)
    {
        switch (change.Type)
        {
            case BatchChangeType.Insert:
                var todo = new Todo
                {
                    Id = 0,
                    Name = change.Name,
                    Category = change.Category,
                    DateCreated = DateTime.Now,
                    DateUpdated = DateTime.Now,
                    Status = 1
                };

                todoList.Add(todo);
                break;

            case BatchChangeType.Update:
                var toUpdate = await db.Todos.FindAsync(change.Id);
                if (toUpdate is null) return Results.NotFound();
                toUpdate.Name = change.Name;
                toUpdate.DateUpdated = DateTime.Now;
                break;

            default:
                break;
        }

    }
    await db.Todos.AddRangeAsync(todoList);
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapPut("/Todo/{id}", async (TodoDb db, Todo updateTodo, int id) =>
{
    var Todo = await db.Todos.FindAsync(id);
    if (Todo is null) return Results.NotFound();
    Todo.Name = updateTodo.Name;
    Todo.DateUpdated = DateTime.Now;
    Todo.Status = updateTodo.Status;
    await db.SaveChangesAsync();
    return Results.NoContent();
});
app.MapDelete("/Todo/{id}", async (TodoDb db, int id) =>
{
    var Todo = await db.Todos.FindAsync(id);
    if (Todo is null)
    {
        return Results.NotFound();
    }
    db.Todos.Remove(Todo);
    await db.SaveChangesAsync();
    return Results.Ok();
});



app.Run();
