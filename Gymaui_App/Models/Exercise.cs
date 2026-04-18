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

        public int TargetReps { get; set; }

        public int TargetSets { get; set; }
    }
}
