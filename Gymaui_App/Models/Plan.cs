using System;
using SQLite;

namespace Gymaui_App.Models
{
    [Table("Plans")]
    public class Plan
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime Created { get; set; } = DateTime.UtcNow;

        // mark which plan is currently active
        public bool IsActive { get; set; }
    }
}
