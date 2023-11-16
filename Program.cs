using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using TodoStore.Models;
using System.Net;
using System.Text;
using Newtonsoft.Json;

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



app.MapPost("/RemindTodos", async (TodoDb db) =>
{
    static bool IsDateToday(DateTime? inputDate)
    {
        if (!inputDate.HasValue)
        {
            return false;
        }

        // Get today's date
        DateTime today = DateTime.Now.Date;

        // Compare the year, month, and day of both dates
        return inputDate.Value.Year == today.Year &&
               inputDate.Value.Month == today.Month &&
               inputDate.Value.Day == today.Day;
    }


    var todos = await db.Todos.Where(x => (x.Category == Category.Todos || x.Category == Category.TodaysTodos) && x.Status == 1).ToListAsync();

    var newTodos = todos.Where(x => IsDateToday(x.DueDate)).ToList();

    var body = "Hi John, here are your Todos for today: \n";

    foreach (var todo in newTodos)
    {
        body += $"{todo.Name}\n";
    }
    await SendSmsViaByteFlow(body);

    return newTodos;

});

static async Task SendSmsViaByteFlow(string body)
{
  
    string baseUrl = "http://host.docker.internal:8005/sms/john";


    Console.WriteLine($"here in fn");

    // Create a new instance of HttpClient
    using (HttpClient client = new HttpClient())
    {
        try
        {
            Console.WriteLine($"here before");

            var postData = new
            {
                message = body
            };

            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));


            // Convert the object to a JSON string
            string jsonData = JsonConvert.SerializeObject(postData);

            HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(baseUrl, content);
        }
        catch (HttpRequestException e)
        {
            // Log.Error($"API Request Error: {e.Message}");
            Console.WriteLine(e.Message);
        }
    }
}

app.MapPost("/SendReminder", async (CheckChoresReminder request) =>
{

    var body = request.Action == "Create" ? "Good morning John, \n Reminder to use vault and set your todos for the day" : "Hi John \n Reminder to review your todos and update their status";

    await SendSmsViaByteFlow(body);
});





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
                    Status = 1,
                    DueDate = change.Category == Category.TodaysTodos ? DateTime.Now : null
                };

                todoList.Add(todo);
                break;

            case BatchChangeType.Update:
                var toUpdate = await db.Todos.FindAsync(change.Id);
                if (toUpdate is null) return Results.NotFound();
                toUpdate.Name = change.Name;
                toUpdate.DateUpdated = DateTime.Now;
                toUpdate.DueDate = change.DueDate;
                toUpdate.Status = change.Status;

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
