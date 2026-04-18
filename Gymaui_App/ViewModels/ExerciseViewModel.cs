using Gymaui_App.Models;
using Gymaui_App.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Gymaui_App.ViewModels
{
    public class ExerciseViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;

        public ObservableCollection<Exercise> Exercises { get; } = new ObservableCollection<Exercise>();

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value)
                    return;
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        public ExerciseViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        }

        public async Task LoadExercisesAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                await _databaseService.InitializeAsync();
                var list = await _databaseService.GetExercisesAsync();

                Exercises.Clear();
                foreach (var e in list)
                    Exercises.Add(e);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task AddExerciseAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                var exercise = new Exercise
                {
                    Name = "New Exercise",
                    MuscleGroup = string.Empty,
                    YouTubeUrl = string.Empty,
                    ImagePath = string.Empty,
                    TargetReps = 0,
                    TargetSets = 0
                };

                await _databaseService.AddExerciseAsync(exercise);
                Exercises.Add(exercise);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task DeleteExerciseAsync(Exercise exercise)
        {
            if (exercise == null || IsBusy)
                return;

            try
            {
                IsBusy = true;
                await _databaseService.DeleteExerciseAsync(exercise);
                Exercises.Remove(exercise);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
