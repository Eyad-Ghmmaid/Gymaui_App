using Gymaui_App.Models;
using Gymaui_App.Services;
using Gymaui_App.Utilities;
using System.Collections.ObjectModel;

namespace Gymaui_App.Views
{
    [QueryProperty(nameof(ExerciseId), "ExerciseId")]
    [QueryProperty(nameof(WorkoutSessionId), "WorkoutSessionId")]
    public partial class ActiveWorkoutPage : ContentPage
    {
        public const string Route = nameof(ActiveWorkoutPage);
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

        private readonly DatabaseService _databaseService;
        private readonly CalendarService _calendarService;
        private WorkoutSession _session;
        private PlanDay? _currentPlanDay;
        private ObservableCollection<WorkoutExerciseItem> _exerciseItems = new();

        public ActiveWorkoutPage(DatabaseService databaseService, CalendarService calendarService)
        {
            InitializeComponent();
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _calendarService = calendarService ?? throw new ArgumentNullException(nameof(calendarService));
            _session = new WorkoutSession();

            // Wire up custom header events using helper
            HeaderEventHelper.SetupHeaderEvents(this);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                await _databaseService.InitializeAsync();

                // Always try to load the current plan day for completion tracking
                var planDay = await _databaseService.GetTodaysPlanDayAsync();
                if (planDay != null && planDay.IsTrainingDay)
                    _currentPlanDay = planDay;

                if (AppShell.PendingWorkoutSessionId.HasValue && AppShell.PendingWorkoutSessionId > 0)
                {
                    var sid = AppShell.PendingWorkoutSessionId.Value;
                    var s = await _databaseService.GetWorkoutSessionAsync(sid);
                    if (s != null)
                    {
                        _session = s;
                        await BindExercisesWithCompletionAsync(_session.Exercises);
                        AppShell.PendingWorkoutSessionId = null;
                        return;
                    }
                }

                if (!string.IsNullOrWhiteSpace(WorkoutSessionId) && int.TryParse(WorkoutSessionId, out var sid2))
                {
                    var s = await _databaseService.GetWorkoutSessionAsync(sid2);
                    if (s != null)
                    {
                        _session = s;
                        await BindExercisesWithCompletionAsync(_session.Exercises);
                        return;
                    }
                }

                if (!string.IsNullOrWhiteSpace(ExerciseId) && int.TryParse(ExerciseId, out var id))
                {
                    var exercise = await _databaseService.GetExerciseAsync(id);
                    if (exercise != null)
                    {
                        if (_session.Id == 0)
                        {
                            await _databaseService.AddWorkoutSessionAsync(_session);
                        }

                        var exists = _session.Exercises.Any(e => e.Id == exercise.Id);
                        if (!exists)
                        {
                            _session.Exercises.Add(exercise);
                            await _databaseService.UpdateWorkoutSessionAsync(_session);
                        }

                        await BindExercisesWithCompletionAsync(_session.Exercises);
                        return;
                    }
                }

                if (_currentPlanDay != null)
                {
                    // Check if a session for today already exists
                    var existingSession = await _databaseService.GetTodaysSessionAsync();
                    if (existingSession != null)
                    {
                        _session = existingSession;
                        await BindExercisesWithCompletionAsync(_session.Exercises);
                        return;
                    }

                    // Batch-load all exercises in one query instead of N+1
                    var planExercises = await _databaseService.GetExercisesForDayAsync(_currentPlanDay.Id);
                    var exerciseIds = planExercises.OrderBy(pe => pe.Order).Select(pe => pe.ExerciseId);
                    var exercises = await _databaseService.GetExercisesByIdsAsync(exerciseIds);

                    if (exercises.Count > 0)
                    {
                        var planMap = planExercises.ToDictionary(pe => pe.ExerciseId);
                        var snapshot = exercises.Select(e => new Exercise
                        {
                            Id = e.Id,
                            Name = e.Name,
                            MuscleGroup = e.MuscleGroup,
                            YouTubeUrl = e.YouTubeUrl,
                            ImagePath = e.ImagePath,
                            TargetSets = planMap.ContainsKey(e.Id) ? planMap[e.Id].TargetSets : 0,
                            TargetReps = planMap.ContainsKey(e.Id) ? planMap[e.Id].TargetReps : 0
                        }).ToList();

                        _session.Exercises = snapshot;
                        _session.Name = _currentPlanDay.Name;
                        if (_session.Id == 0)
                        {
                            await _databaseService.AddWorkoutSessionAsync(_session);
                        }
                        else
                        {
                            await _databaseService.UpdateWorkoutSessionAsync(_session);
                        }
                    }
                }

                await BindExercisesWithCompletionAsync(_session.Exercises);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnAppearing: {ex.Message}");
            }
        }

        private async Task BindExercisesWithCompletionAsync(List<Exercise> exercises)
        {
            _exerciseItems.Clear();

            // Load completion status from database if we have a plan day
            Dictionary<int, bool>? completionMap = null;
            if (_currentPlanDay != null)
            {
                var planExercises = await _databaseService.GetExercisesForDayAsync(_currentPlanDay.Id);
                var today = DateTime.Now.Date;
                completionMap = new Dictionary<int, bool>();
                foreach (var pe in planExercises)
                {
                    var isCompleted = await _calendarService.IsExerciseCompletedAsync(pe.Id, today);
                    completionMap[pe.ExerciseId] = isCompleted;
                }
            }

            foreach (var exercise in exercises)
            {
                bool completed = completionMap != null
                    && completionMap.TryGetValue(exercise.Id, out var c) && c;
                _exerciseItems.Add(new WorkoutExerciseItem(exercise, completed));
            }

            ExercisesCollection.ItemsSource = _exerciseItems;
        }

        private async void OnExerciseSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
            {
                var item = e.CurrentSelection[0] as WorkoutExerciseItem;
                var exercise = item?.Exercise;
                if (exercise != null)
                {
                    if (Shell.Current != null)
                    {
                        var sid = _session?.Id ?? 0;
                        await Shell.Current.GoToAsync($"{nameof(ExerciseSetsPage)}?exerciseId={exercise.Id}&workoutSessionId={sid}");
                    }
                }

                ((CollectionView)sender).SelectedItem = null;
            }
        }

        private async void OnExerciseCompletionToggled(object sender, EventArgs e)
        {
            try
            {
                var element = sender as Element;
                var item = element?.BindingContext as WorkoutExerciseItem;
                if (item == null)
                    return;

                var exercise = item.Exercise;

                // Try to find the plan exercise in the current plan day first
                PlanExercise? planExercise = null;

                if (_currentPlanDay != null)
                {
                    var planExercises = await _databaseService.GetExercisesForDayAsync(_currentPlanDay.Id);
                    planExercise = planExercises.FirstOrDefault(pe => pe.ExerciseId == exercise.Id);
                }

                // If not found in current day, search across all days of the active plan
                if (planExercise == null)
                {
                    var activePlan = await _databaseService.GetActivePlanAsync();
                    if (activePlan != null)
                    {
                        var allDays = await _databaseService.GetDaysForPlanAsync(activePlan.Id);
                        foreach (var day in allDays)
                        {
                            var dayExercises = await _databaseService.GetExercisesForDayAsync(day.Id);
                            planExercise = dayExercises.FirstOrDefault(pe => pe.ExerciseId == exercise.Id);
                            if (planExercise != null)
                            {
                                // Also update _currentPlanDay to the correct day for further toggles
                                _currentPlanDay = day;
                                break;
                            }
                        }
                    }
                }

                if (planExercise == null)
                {
                    await DisplayAlert("Fehler", "Uebung nicht im Plan gefunden", "OK");
                    return;
                }

                bool newCompletionStatus = !item.IsCompleted;

                await _calendarService.MarkExerciseCompletedAsync(planExercise.Id, DateTime.Now.Date, newCompletionStatus);

                // Update the bound property so the UI reflects the change persistently
                item.IsCompleted = newCompletionStatus;

                // Haptic feedback
                try
                {
                    HapticFeedback.Default.Perform(newCompletionStatus ? HapticFeedbackType.LongPress : HapticFeedbackType.Click);
                }
                catch { /* Haptic not available on all platforms */ }

                if (newCompletionStatus && sender is VisualElement ve)
                {
                    // Scale animation for completion
                    await ve.ScaleTo(1.3, 100, Easing.CubicOut);
                    await ve.ScaleTo(1.0, 100, Easing.CubicIn);
                }

                System.Diagnostics.Debug.WriteLine($"Exercise '{exercise.Name}' marked as {(newCompletionStatus ? "completed" : "incomplete")}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error toggling exercise completion: {ex.Message}");
                await DisplayAlert("Fehler", $"Fehler beim Aktualisieren der Uebung: {ex.Message}", "OK");
            }
        }
    }
}
