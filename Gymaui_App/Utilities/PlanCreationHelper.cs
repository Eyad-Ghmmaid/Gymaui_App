using System;
using System.Collections.Generic;
using Gymaui_App.Models;

namespace Gymaui_App.Utilities
{
    /// <summary>
    /// Data transfer object for the multi-step plan creation flow
    /// </summary>
    public class PlanCreationDto
    {
        public string PlanName { get; set; } = string.Empty;
        
        /// <summary>
        /// Selected training days (0=Monday, 6=Sunday)
        /// </summary>
        public List<int> SelectedTrainingDays { get; set; } = new();
        
        /// <summary>
        /// Exercises per day: key = DayOfWeek, value = list of exercise IDs
        /// </summary>
        public Dictionary<int, List<int>> ExercisesPerDay { get; set; } = new();
    }
    
    /// <summary>
    /// Represents a day of the week with its properties
    /// </summary>
    public class WeekDayInfo
    {
        public int DayOfWeek { get; set; } // 0=Monday, 6=Sunday
        public string DayName { get; set; } = string.Empty;
        public string DayAbbreviation { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
        
        public static WeekDayInfo[] GetWeekDays()
        {
            return new[]
            {
                new WeekDayInfo { DayOfWeek = 0, DayName = "Monday", DayAbbreviation = "Mo", IsSelected = false },
                new WeekDayInfo { DayOfWeek = 1, DayName = "Tuesday", DayAbbreviation = "Di", IsSelected = false },
                new WeekDayInfo { DayOfWeek = 2, DayName = "Wednesday", DayAbbreviation = "Mi", IsSelected = false },
                new WeekDayInfo { DayOfWeek = 3, DayName = "Thursday", DayAbbreviation = "Do", IsSelected = false },
                new WeekDayInfo { DayOfWeek = 4, DayName = "Friday", DayAbbreviation = "Fr", IsSelected = false },
                new WeekDayInfo { DayOfWeek = 5, DayName = "Saturday", DayAbbreviation = "Sa", IsSelected = false },
                new WeekDayInfo { DayOfWeek = 6, DayName = "Sunday", DayAbbreviation = "So", IsSelected = false }
            };
        }
    }
}
