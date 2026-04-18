using Gymaui_App.Models;
using Gymaui_App.Services;
using Gymaui_App.Utilities;

namespace Gymaui_App.Views
{
    [QueryProperty(nameof(SessionIdQuery), "sessionId")]
    public partial class WorkoutDetailPage : ContentPage
    {
        public const string Route = nameof(WorkoutDetailPage);

        private readonly DatabaseService _db;
        private int _sessionId;
        private WorkoutSession? _session;

        public string SessionIdQuery
        {
            set
            {
                if (int.TryParse(value, out var id))
                {
                    _sessionId = id;
                    _ = LoadSessionAsync();
                }
            }
        }

        public WorkoutDetailPage(DatabaseService db)
        {
            InitializeComponent();
            _db = db ?? throw new ArgumentNullException(nameof(db));

            HeaderEventHelper.SetupHeaderEvents(this);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_sessionId > 0)
                await LoadSessionAsync();
        }

        private async Task LoadSessionAsync()
        {
            try
            {
                await _db.InitializeAsync();

                _session = await _db.GetWorkoutSessionAsync(_sessionId);
                if (_session == null)
                {
                    await DisplayAlert("Fehler", "Trainingstag nicht gefunden.", "OK");
                    return;
                }

                SessionDateLabel.Text = _session.Date.ToLocalTime().ToString("dddd, dd.MM.yyyy  HH:mm");

                var exercises = _session.Exercises ?? new List<Exercise>();
                var allLogs = await _db.GetLogsForWorkoutSessionAsync(_session.Id);

                var displayItems = exercises.Select(ex =>
                {
                    var logsForExercise = allLogs
                        .Where(l => l.ExerciseId == ex.Id)
                        .OrderBy(l => l.SetNumber)
                        .ToList();

                    return new ExerciseWithLogs(ex, logsForExercise);
                }).ToList();

                ExercisesCollection.ItemsSource = displayItems;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WorkoutDetailPage Error: {ex.Message}");
            }
        }

        private async void OnExerciseSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count == 0)
                return;

            if (e.CurrentSelection[0] is ExerciseWithLogs item)
            {
                await Shell.Current.GoToAsync(
                    $"{nameof(ExerciseSetsPage)}?exerciseId={item.ExerciseId}&workoutSessionId={_sessionId}");
            }

            ((CollectionView)sender!).SelectedItem = null;
        }

        private async void OnEditLogSwipe(object? sender, EventArgs e)
        {
            if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is ExerciseLog log)
            {
                var weightResult = await DisplayPromptAsync("Gewicht bearbeiten", "Neues Gewicht (kg):", initialValue: log.Weight.ToString("0.##"), keyboard: Keyboard.Numeric);
                if (weightResult == null) return;

                var repsResult = await DisplayPromptAsync("Wiederholungen bearbeiten", "Neue Wiederholungen:", initialValue: log.Reps.ToString(), keyboard: Keyboard.Numeric);
                if (repsResult == null) return;

                if (double.TryParse(weightResult, out var newWeight))
                    log.Weight = newWeight;
                if (int.TryParse(repsResult, out var newReps))
                    log.Reps = newReps;

                await _db.UpdateExerciseLogAsync(log);
                await LoadSessionAsync();
            }
        }

        private async void OnDeleteLogSwipe(object? sender, EventArgs e)
        {
            if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is ExerciseLog log)
            {
                var confirm = await DisplayAlert("Löschen", $"Satz {log.SetNumber} ({log.Weight} kg x {log.Reps}) wirklich löschen?", "Ja", "Nein");
                if (!confirm) return;

                await _db.DeleteExerciseLogAsync(log);
                await LoadSessionAsync();
            }
        }
    }

    public class ExerciseWithLogs
    {
        public int ExerciseId { get; }
        public string ExerciseName { get; }
        public string MuscleGroup { get; }
        public string TargetInfo { get; }
        public List<ExerciseLog> Logs { get; }
        public bool HasNoLogs => Logs.Count == 0;
        public int LogsHeight => Logs.Count > 0 ? Logs.Count * 28 : 0;

        public ExerciseWithLogs(Exercise exercise, List<ExerciseLog> logs)
        {
            ExerciseId = exercise.Id;
            ExerciseName = exercise.Name;
            MuscleGroup = exercise.MuscleGroup;
            TargetInfo = $"{exercise.TargetSets} Saetze x {exercise.TargetReps} Wdh";
            Logs = logs;
        }
    }
}
