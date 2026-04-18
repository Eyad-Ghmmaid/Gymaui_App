using SQLite;

namespace Gymaui_App.Models
{
    [Table("PlanExercises")]
    public class PlanExercise
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // FK to PlanDay
        public int PlanDayId { get; set; }

        // FK to Exercise
        public int ExerciseId { get; set; }

        // ordering within the day
        public int Order { get; set; }

        public string Notes { get; set; } = string.Empty;

        [Ignore]
        public Exercise? Exercise { get; set; }
    }
}
