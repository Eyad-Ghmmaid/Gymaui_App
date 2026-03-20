using System;
using SQLite;

namespace Gymaui_App.Models
{
    [Table("PlanDays")]
    public class PlanDay
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // FK to Plan
        public int PlanId { get; set; }

        // Day of week: 0 = Monday, 1 = Tuesday, ..., 6 = Sunday
        public int DayOfWeek { get; set; }

        // Name of the day (Monday, Tuesday, etc.)
        public string Name { get; set; } = string.Empty;

        // Is this a training day or rest day?
        public bool IsTrainingDay { get; set; } = true;

        // Display order
        public int Order { get; set; }
    }
}
