using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace TodoStore.Models
{

    public class ChangesRequest
    {

        public string? Name { get; set; }

        public int Id { get; set; }

        public string Type { get; set; }
        public Category Category { get; set; }
    }

    public class BatchChangesRequest
    {

        public List<ChangesRequest> Changes { get; set; }
    }
    public struct BatchChangeType
    {
        public const string Insert = "insert";
        public const string Update = "update";
        public const string Remove = "remove";

    }
    public class Todo
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        [Range(1, 2)]
        public int Status { get; set; } = 1;

        public DateTime DateCreated { get; set; } = DateTime.Now;

        public DateTime DateUpdated { get; set; } = DateTime.Now;

        // public DateTime? DueDate { get; set; }

        public Category Category { get; set; } = Category.Todos;

    }



    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Category
    {
        Todos,
        Info,
        ToLearn,
    }

    class TodoDb : DbContext
    {
        public TodoDb(DbContextOptions options) : base(options) { }
        public DbSet<Todo> Todos { get; set; } = null!;
    }
}