using SQLite;

namespace Gymaui_App.Models
{
    [Table("DailyProgress")]
    public class DailyProgress
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // FK to PlanDay
        public int PlanDayId { get; set; }

        // The date this progress is for (stored as UTC, midnight)
        public DateTime Date { get; set; }

        // How many exercises are marked as completed for this day
        public int CompletedExerciseCount { get; set; }

        // Total exercises for this day (cached for quick lookup)
        public int TotalExerciseCount { get; set; }

        // Timestamp when the record was created/updated
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        [Ignore]
        public PlanDay? PlanDay { get; set; }

        public bool IsComplete => CompletedExerciseCount > 0 && CompletedExerciseCount == TotalExerciseCount;
    }
}
