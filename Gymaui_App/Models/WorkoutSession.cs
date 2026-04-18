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

        // User-editable display name for this training day
        public string Name { get; set; } = string.Empty;

        // Serialized representation stored in the DB
        private string _exercisesJson = string.Empty;
        private List<Exercise>? _cachedExercises;

        public string ExercisesJson
        {
            get => _exercisesJson;
            set
            {
                _exercisesJson = value;
                _cachedExercises = null; // invalidate cache when JSON changes
            }
        }

        [Ignore]
        public List<Exercise> Exercises
        {
            get
            {
                if (_cachedExercises != null)
                    return _cachedExercises;

                _cachedExercises = string.IsNullOrEmpty(ExercisesJson)
                    ? new List<Exercise>()
                    : JsonSerializer.Deserialize<List<Exercise>>(ExercisesJson) ?? new List<Exercise>();

                return _cachedExercises;
            }
            set
            {
                _cachedExercises = value ?? new List<Exercise>();
                _exercisesJson = JsonSerializer.Serialize(_cachedExercises);
            }
        }
    }
}
