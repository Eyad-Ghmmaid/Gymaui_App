namespace Gymaui_App.Utilities
{
    /// <summary>
    /// Data transfer object for the multi-step plan creation flow
    /// </summary>
    public class PlanCreationDto
    {
        public string PlanName { get; set; } = string.Empty;

        /// <summary>
        /// Number of training days in this plan
        /// </summary>
        public int NumberOfDays { get; set; }

        /// <summary>
        /// Selected training day indices (0-based)
        /// </summary>
        public List<int> SelectedTrainingDays { get; set; } = new();

        /// <summary>
        /// Exercises per day with their sets and reps
        /// key = day index, value = list of (exerciseId, targetSets, targetReps) tuples
        /// </summary>
        public Dictionary<int, List<(int ExerciseId, int TargetSets, int TargetReps)>> ExercisesPerDay { get; set; } = new();
    }

    /// <summary>
    /// Represents a training day with a sequential number (Tag 1, Tag 2, etc.)
    /// </summary>
    public class TrainingDayInfo
    {
        public int DayIndex { get; set; } // 0-based index
        public string DayName { get; set; } = string.Empty; // "Tag 1", "Tag 2", etc.

        public static List<TrainingDayInfo> CreateDays(int count)
        {
            var days = new List<TrainingDayInfo>();
            for (int i = 0; i < count; i++)
            {
                days.Add(new TrainingDayInfo
                {
                    DayIndex = i,
                    DayName = $"Tag {i + 1}"
                });
            }
            return days;
        }
    }
}
