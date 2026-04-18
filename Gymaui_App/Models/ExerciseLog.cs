using SQLite;

namespace Gymaui_App.Models
{
    [Table("ExerciseLogs")]
    public class ExerciseLog
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int ExerciseId { get; set; }

        public int WorkoutSessionId { get; set; }

        public int SetNumber { get; set; }

        public double Weight { get; set; }

        public int Reps { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string? Notes { get; set; }

        [Ignore]
        public Exercise? Exercise { get; set; }

        public ExerciseLog() { }

        public ExerciseLog(Exercise exercise)
        {
            Exercise = exercise ?? throw new ArgumentNullException(nameof(exercise));
            ExerciseId = exercise.Id;
        }
    }
}
