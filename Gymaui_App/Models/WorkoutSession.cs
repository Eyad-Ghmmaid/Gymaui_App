using System;
using System.Collections.Generic;
using SQLite;
using System.Text.Json;

namespace Gymaui_App.Models
{
    [Table("WorkoutSessions")]
    public class WorkoutSession
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;

        // Serialized representation stored in the DB
        public string ExercisesJson { get; set; } = string.Empty;

        [Ignore]
        public List<Exercise> Exercises
        {
            get => string.IsNullOrEmpty(ExercisesJson) ? new List<Exercise>() : JsonSerializer.Deserialize<List<Exercise>>(ExercisesJson) ?? new List<Exercise>();
            set => ExercisesJson = JsonSerializer.Serialize(value ?? new List<Exercise>());
        }
    }
}
