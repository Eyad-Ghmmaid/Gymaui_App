using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Gymaui_App.Models;
using Gymaui_App.Services;

namespace Gymaui_App.ViewModels
{
    public class ExerciseViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;

        public ObservableCollection<Exercise> Exercises { get; } = new ObservableCollection<Exercise>();

        public ICommand AddExerciseCommand { get; }
        public ICommand DeleteExerciseCommand { get; }

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

            AddExerciseCommand = new Command(async () => await AddExerciseAsync());
            DeleteExerciseCommand = new Command<Exercise>(async (ex) => await DeleteExerciseAsync(ex));
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

        public async Task DeleteExerciseAsync(Exercise exercise)
        {
            if (exercise == null)
                return;

            await _database_service_delete_guard(exercise);
        }

        // helper to ensure exceptions from DB operations are handled consistently
        private async Task _database_service_delete_guard(Exercise exercise)
        {
            try
            {
                await _databaseService.DeleteExerciseAsync(exercise);
                Exercises.Remove(exercise);
            }
            catch
            {
                // swallow or rethrow depending on app policy; keep minimal here
                throw;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
