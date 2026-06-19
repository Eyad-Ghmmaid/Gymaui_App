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

        // Sets and Reps defined at the plan level
        public int TargetSets { get; set; } = 3;

        public int TargetReps { get; set; } = 10;

        [Ignore]
        public Exercise? Exercise { get; set; }
    }
}
