using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Gymaui_App.Models
{
    public class WorkoutExerciseItem : INotifyPropertyChanged
    {
        private bool _isCompleted;

        public Exercise Exercise { get; set; }

        public string Name => Exercise?.Name ?? string.Empty;
        public int Id => Exercise?.Id ?? 0;

        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                if (_isCompleted != value)
                {
                    _isCompleted = value;
                    OnPropertyChanged();
                }
            }
        }

        public WorkoutExerciseItem(Exercise exercise, bool isCompleted = false)
        {
            Exercise = exercise;
            _isCompleted = isCompleted;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
