using SQLite;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Gymaui_App.Models
{
    [Table("PlanDays")]
    public class PlanDay : INotifyPropertyChanged
    {
        private int _id;
        private int _planId;
        private int _dayOfWeek;
        private string _name = string.Empty;
        private bool _isTrainingDay = true;
        private int _order;

        public event PropertyChangedEventHandler? PropertyChanged;

        [PrimaryKey, AutoIncrement]
        public int Id
        {
            get => _id;
            set { if (_id != value) { _id = value; OnPropertyChanged(); } }
        }

        // FK to Plan
        public int PlanId
        {
            get => _planId;
            set { if (_planId != value) { _planId = value; OnPropertyChanged(); } }
        }

        // Day index within the plan (0-based)
        public int DayOfWeek
        {
            get => _dayOfWeek;
            set { if (_dayOfWeek != value) { _dayOfWeek = value; OnPropertyChanged(); } }
        }

        // Name of the day (Tag 1, Tag 2, etc.)
        public string Name
        {
            get => _name;
            set { if (_name != value) { _name = value; OnPropertyChanged(); } }
        }

        // Is this a training day or rest day?
        public bool IsTrainingDay
        {
            get => _isTrainingDay;
            set { if (_isTrainingDay != value) { _isTrainingDay = value; OnPropertyChanged(); } }
        }

        // Display order
        public int Order
        {
            get => _order;
            set { if (_order != value) { _order = value; OnPropertyChanged(); } }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
