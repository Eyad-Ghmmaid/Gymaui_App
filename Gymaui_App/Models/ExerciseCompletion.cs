using SQLite;

namespace Gymaui_App.Models
{
    [Table("ExerciseCompletion")]
    public class ExerciseCompletion
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // FK to PlanExercise
        public int PlanExerciseId { get; set; }

        // The date this exercise was marked complete (UTC, midnight)
        public DateTime Date { get; set; }

        // Whether this exercise was completed on this date
        public bool IsCompleted { get; set; }

        // Timestamp when marked as completed
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

        [Ignore]
        public PlanExercise? PlanExercise { get; set; }
    }
}
