using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TodoStore.Models 
{
    public class Todo
    {
          public int Id { get; set; }
          public string? Name { get; set; }
          [Range(1,2)]
          public int Status { get; set; } = 1;

          public DateTime DateCreated  {get; set;} = DateTime.Now;

          public DateTime DateUpdated  {get; set;} = DateTime.Now;

    }

    class TodoDb : DbContext
{
    public TodoDb(DbContextOptions options) : base(options) { }
    public DbSet<Todo> Todos { get; set; } = null!;
}
}