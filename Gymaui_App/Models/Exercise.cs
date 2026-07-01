using SQLite;

namespace Gymaui_App.Models
{
    [Table("Exercises")]
    public class Exercise
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string MuscleGroup { get; set; } = string.Empty;

        public string YouTubeUrl { get; set; } = string.Empty;

        public string ImagePath { get; set; } = string.Empty;

        // Optional snapshot of plan targets (serialized into WorkoutSession for historical accuracy)
        public int TargetSets { get; set; } = 0;

        public int TargetReps { get; set; } = 0;
    }
}
