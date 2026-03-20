using System;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using Gymaui_App.Models;
using Gymaui_App.Services;
using Microsoft.Maui.Graphics;
using Gymaui_App.Converters;

namespace Gymaui_App.Views
{
    [QueryProperty(nameof(ExerciseIdQuery), "exerciseId")]
    public partial class ExerciseSetsPage : ContentPage
    {
        private int _workoutSessionId = 0;
        private Exercise _exercise = new Exercise();
        private readonly DatabaseService _databaseService;

        // keep refs to entries per set
        private readonly List<Entry> _weightEntries = new List<Entry>();
        private readonly List<Entry> _repsEntries = new List<Entry>();

        public ExerciseSetsPage(DatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            InitializeComponent();
        }

        private async void OnYouTubeClicked(object? sender, EventArgs e)
        {
            await OpenYouTubeVideo(_exercise?.YouTubeUrl ?? string.Empty);
        }

        private async Task OpenYouTubeVideo(string url)
        {
            if (string.IsNullOrWhiteSpace(url) ||
                !(url.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) || url.Contains("youtu.be", StringComparison.OrdinalIgnoreCase)))
            {
                await DisplayAlert("Fehler", "Kein gültiger Video-Link vorhanden", "OK");
                return;
            }

            var trimmed = url.Trim();
            if (!trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = "https://" + trimmed;
            }

            try
            {
                var uri = new Uri(trimmed);
                var canOpen = await Launcher.Default.CanOpenAsync(uri);
                if (canOpen)
                {
                    await Launcher.Default.OpenAsync(uri);
                }
                else
                {
                    await DisplayAlert("Fehler", "Kein gültiger Video-Link vorhanden", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fehler", $"Die URL konnte nicht geöffnet werden: {ex.Message}", "OK");
            }
        }

        private void BuildSetInputs(double suggestedWeight = 0)
        {
            SetsContainer.Children.Clear();
            _weightEntries.Clear();
            _repsEntries.Clear();

            for (int i = 0; i < Math.Max(1, _exercise.TargetSets); i++)
            {
                var setNumber = i + 1;

                var grid = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, Padding = new Thickness(4), BackgroundColor = Colors.Transparent };

                var weightEntry = new Entry { Placeholder = "Weight (kg)", Keyboard = Keyboard.Numeric };
                var repsEntry = new Entry { Placeholder = "Reps", Keyboard = Keyboard.Numeric };
                var statusBox = new BoxView { WidthRequest = 20, HeightRequest = 20, Color = Colors.Gray, VerticalOptions = LayoutOptions.Center };

                repsEntry.TextChanged += (s, e) =>
                {
                    UpdateStatusColor(statusBox, repsEntry.Text);
                };

                // prefill suggested weight if available
                if (suggestedWeight > 0)
                    weightEntry.Text = suggestedWeight.ToString("0.##");

                _weightEntries.Add(weightEntry);
                _repsEntries.Add(repsEntry);

                grid.Add(weightEntry, 0, 0);
                grid.Add(repsEntry, 1, 0);
                grid.Add(statusBox, 2, 0);

                SetsContainer.Children.Add(grid);
            }
        }

        // QueryProperty setter will call this when navigation passes exerciseId
        public string ExerciseIdQuery
        {
            set
            {
                if (int.TryParse(value, out var id))
                {
                    _ = LoadExerciseAsync(id);
                }
            }
        }

        // allow passing the workout session id so logs get associated with the right session
        public string WorkoutSessionIdQuery
        {
            set
            {
                if (int.TryParse(value, out var id))
                {
                    _workoutSessionId = id;
                }
            }
        }

        private async Task LoadExerciseAsync(int exerciseId)
        {
            await _database_service_initialize_guard();

            var exercise = await _databaseService.GetExerciseAsync(exerciseId);
            if (exercise == null)
            {
                await DisplayAlert("Error", "Exercise not found.", "OK");
                return;
            }

            _exercise = exercise;
            ExerciseNameLabel.Text = _exercise.Name;
            // load recent logs to show history and suggest weight
            var logs = await _databaseService.GetLogsForExerciseAsync(exerciseId);
            var recent = logs.OrderByDescending(l => l.Timestamp).Take(8).ToList();
            PreviousLogsCollection.ItemsSource = recent;

            double suggestedWeight = 0;
            if (recent.Count > 0)
            {
                suggestedWeight = recent.First().Weight;
            }

            BuildSetInputs(suggestedWeight);
        }

        private async Task _database_service_initialize_guard()
        {
            try
            {
                await _databaseService.InitializeAsync();
            }
            catch
            {
                // ignore for now
            }
        }

        // model used for per-set binding
        private class SetModel : BindableObject
        {
            private string _actualReps = string.Empty;
            public string ActualReps
            {
                get => _actualReps;
                set
                {
                    if (_actualReps == value)
                        return;
                    _actualReps = value;
                    OnPropertyChanged();
                }
            }

            public int TargetReps { get; set; }
        }

        private void UpdateStatusColor(BoxView box, string? repsText)
        {
            if (int.TryParse(repsText, out var reps))
            {
                if (reps >= _exercise.TargetReps)
                    box.Color = Colors.Green;
                else
                    box.Color = Colors.Red;
            }
            else
            {
                box.Color = Colors.Gray;
            }
        }

        private async void OnSaveClicked(object? sender, EventArgs e)
        {
            try
            {
                await _databaseService.InitializeAsync();

                // if we don't have a workout session yet, create one and attach this exercise
                if (_workoutSessionId == 0)
                {
                    var session = new WorkoutSession();
                    session.Exercises = new List<Exercise> { _exercise };
                    await _databaseService.AddWorkoutSessionAsync(session);
                    _workoutSessionId = session.Id;
                }

                var logs = new List<ExerciseLog>();

                for (int i = 0; i < _repsEntries.Count; i++)
                {
                    var repsText = _repsEntries[i].Text;
                    var weightText = _weightEntries[i].Text;

                    if (!int.TryParse(repsText, out var reps))
                        reps = 0;
                    if (!double.TryParse(weightText, out var weight))
                        weight = 0;

                    var log = new ExerciseLog
                    {
                        ExerciseId = _exercise.Id,
                        WorkoutSessionId = _workoutSessionId,
                        SetNumber = i + 1,
                        Reps = reps,
                        Weight = weight,
                        Timestamp = DateTime.UtcNow
                    };

                    logs.Add(log);
                }

                if (logs.Count > 0)
                {
                    await _databaseService.AddExerciseLogsAsync(logs);
                }

                await DisplayAlert("Saved", "Exercise logs saved.", "OK");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
