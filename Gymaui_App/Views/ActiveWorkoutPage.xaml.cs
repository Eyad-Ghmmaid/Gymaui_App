using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Gymaui_App.Models;
using Gymaui_App.Services;

namespace Gymaui_App.Views
{
[QueryProperty(nameof(ExerciseId), "ExerciseId")]
[QueryProperty(nameof(WorkoutSessionId), "WorkoutSessionId")]
public partial class ActiveWorkoutPage : ContentPage
    {
        private string? _exerciseId;
        public string? ExerciseId
        {
            get => _exerciseId;
            set => _exerciseId = value;
        }

        private string? _workoutSessionId;
        public string? WorkoutSessionId
        {
            get => _workoutSessionId;
            set => _workoutSessionId = value;
        }
        private readonly DatabaseService _databaseService = new DatabaseService();
        private WorkoutSession _session;

        public ActiveWorkoutPage(WorkoutSession? session = null)
        {
            InitializeComponent();
            _session = session ?? new WorkoutSession();
            LoadSessionAsync();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                await _databaseService.InitializeAsync();

                // Check for pending workout session from navigation
                if (AppShell.PendingWorkoutSessionId.HasValue && AppShell.PendingWorkoutSessionId > 0)
                {
                    var sid = AppShell.PendingWorkoutSessionId.Value;
                    var s = await _databaseService.GetWorkoutSessionAsync(sid);
                    if (s != null)
                    {
                        _session = s;
                        ExercisesCollection.ItemsSource = _session.Exercises?.ToList() ?? new List<Exercise>();
                        AppShell.PendingWorkoutSessionId = null; // Clear it after use
                        return;
                    }
                }

                // Check for WorkoutSessionId from query parameter
                if (!string.IsNullOrWhiteSpace(WorkoutSessionId) && int.TryParse(WorkoutSessionId, out var sid2))
                {
                    var s = await _databaseService.GetWorkoutSessionAsync(sid2);
                    if (s != null)
                    {
                        _session = s;
                        ExercisesCollection.ItemsSource = _session.Exercises?.ToList() ?? new List<Exercise>();
                        return;
                    }
                }

                // Check for ExerciseId
                if (!string.IsNullOrWhiteSpace(ExerciseId) && int.TryParse(ExerciseId, out var id))
                {
                    var exercise = await _database_service_get_exercise_guard(id);
                    if (exercise != null)
                    {
                        // ensure session exists in DB
                        if (_session.Id == 0)
                        {
                            await _databaseService.AddWorkoutSessionAsync(_session);
                        }

                        // add exercise to session if not already present
                        var exists = _session.Exercises.Any(e => e.Id == exercise.Id);
                        if (!exists)
                        {
                            var list = _session.Exercises;
                            list.Add(exercise);
                            _session.Exercises = list;
                            await _databaseService.UpdateWorkoutSessionAsync(_session);
                        }

                        // refresh UI
                        ExercisesCollection.ItemsSource = _session.Exercises?.ToList() ?? new List<Exercise>();
                        return;
                    }
                }

                // If no specific session or exercise, try to load today's plan exercises
                var planDay = await _databaseService.GetTodaysPlanDayAsync();
                if (planDay != null && planDay.IsTrainingDay)
                {
                    var planExercises = await _databaseService.GetExercisesForDayAsync(planDay.Id);
                    
                    _session.Exercises.Clear();
                    foreach (var pe in planExercises.OrderBy(pe => pe.Order))
                    {
                        var exercise = await _databaseService.GetExerciseAsync(pe.ExerciseId);
                        if (exercise != null)
                        {
                            _session.Exercises.Add(exercise);
                        }
                    }
                }
                
                ExercisesCollection.ItemsSource = _session.Exercises?.ToList() ?? new List<Exercise>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnAppearing: {ex.Message}");
            }
        }

        // helper to get exercise and handle potential exceptions
        private async Task<Exercise?> _database_service_get_exercise_guard(int id)
        {
            try
            {
                return await _databaseService.GetExerciseAsync(id);
            }
            catch
            {
                return null;
            }
        }

        private async void LoadSessionAsync()
        {
            await _databaseService.InitializeAsync();

            if (_session.Id == 0)
            {
                // save a new session to get an Id
                await _databaseService.AddWorkoutSessionAsync(_session);
            }

            ExercisesCollection.ItemsSource = _session.Exercises;
        }

        private async void OnExerciseSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
            {
                var exercise = e.CurrentSelection[0] as Exercise;
                if (exercise != null)
                {
                    // navigate to sets entry page using Shell and pass exercise id
                    if (Shell.Current != null)
                    {
                        var sid = _session?.Id ?? 0;
                        await Shell.Current.GoToAsync($"{nameof(ExerciseSetsPage)}?exerciseId={exercise.Id}&workoutSessionId={sid}");
                    }
                }

                // clear selection
                ((CollectionView)sender).SelectedItem = null;
            }
        }
    }
}
