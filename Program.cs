using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using TodoStore.Models;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Todos") ?? "Data Source=Todos.db";

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
     c.SwaggerDoc("v1", new OpenApiInfo { Title = "Todo API", Description = "Minimal API for Todos", Version = "v1" });
});
builder.Services.AddSqlite<TodoDb>(connectionString);
// builder.Services.AddDbContext<TodoDb>(options => options.UseInMemoryDatabase("items"));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
   c.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API V1");
});

app.MapGet("/Todos", async (TodoDb db) => await db.Todos.ToListAsync());
app.MapGet("/Todo/{id}", async (TodoDb db, int id) => await db.Todos.FindAsync(id));
app.MapPost("/Todo", async (TodoDb db, Todo Todo) =>
{
   Todo.DateCreated = DateTime.Now;
   Todo.DateUpdated= DateTime.Now;
   Todo.Status = 1;
    await db.Todos.AddAsync(Todo);
    await db.SaveChangesAsync();
    return Results.Created($"/Todo/{Todo.Id}", Todo);
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
