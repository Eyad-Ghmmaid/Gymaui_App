using Gymaui_App.Models;
using Gymaui_App.Services;
using Gymaui_App.Utilities;

namespace Gymaui_App.Views
{
    [QueryProperty(nameof(ExerciseIdQuery), "exerciseId")]
    [QueryProperty(nameof(WorkoutSessionIdQuery), "workoutSessionId")]
    public partial class ExerciseSetsPage : ContentPage
    {
        public const string Route = nameof(ExerciseSetsPage);
        private int _workoutSessionId = 0;
        private Exercise _exercise = new Exercise();
        private int _targetSets = 3; // Default value for target sets
        private int _targetReps = 10; // Default value for target reps
        private readonly DatabaseService _databaseService;

        private readonly List<Entry> _weightEntries = new List<Entry>();
        private readonly List<Entry> _repsEntries = new List<Entry>();
        private readonly List<CheckBox> _checkBoxes = new List<CheckBox>();
        private readonly Dictionary<int, int> _savedLogIds = new Dictionary<int, int>();

        // Rest Timer
        private IDispatcherTimer? _restTimer;
        private int _restTimeRemaining = 0;
        private int _restTimerPreset = 60;
        private bool _timerRunning = false;

        public ExerciseSetsPage(DatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            InitializeComponent();

            HeaderEventHelper.SetupHeaderEvents(this);

            // Initialize timer
            _restTimer = Dispatcher.CreateTimer();
            _restTimer.Interval = TimeSpan.FromSeconds(1);
            _restTimer.Tick += OnTimerTick;
        }

        private async void OnYouTubeClicked(object? sender, EventArgs e)
        {
            await OpenYouTubeVideo(_exercise?.YouTubeUrl ?? string.Empty);
        }

        private async Task OpenYouTubeVideo(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                await DisplayAlert("Fehler", "Kein Video-Link vorhanden", "OK");
                return;
            }

            var trimmed = url.Trim();
            if (!trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = "https://" + trimmed;
            }

            // Convert youtu.be short links to full youtube.com URLs for better compatibility
            if (trimmed.Contains("youtu.be/", StringComparison.OrdinalIgnoreCase))
            {
                var videoId = trimmed.Split("youtu.be/", StringSplitOptions.None).LastOrDefault()?.Split('?').FirstOrDefault();
                if (!string.IsNullOrEmpty(videoId))
                {
                    trimmed = $"https://www.youtube.com/watch?v={videoId}";
                }
            }

            try
            {
                await Browser.Default.OpenAsync(new Uri(trimmed), BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fehler", $"Die URL konnte nicht geoeffnet werden: {ex.Message}", "OK");
            }
        }

        private void BuildSetInputs(double suggestedWeight = 0)
        {
            SetsContainer.Children.Clear();
            _weightEntries.Clear();
            _repsEntries.Clear();
            _checkBoxes.Clear();
            _savedLogIds.Clear();

            for (int i = 0; i < Math.Max(1, _targetSets); i++)
            {
                var setIndex = i;

                var grid = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition(GridLength.Star),
                        new ColumnDefinition(GridLength.Star),
                        new ColumnDefinition(GridLength.Auto),
                        new ColumnDefinition(GridLength.Auto)
                    },
                    Padding = new Thickness(4),
                    BackgroundColor = Colors.Transparent
                };

                var weightEntry = new Entry { Placeholder = "Gewicht (kg)", Keyboard = Keyboard.Numeric };
                var repsEntry = new Entry { Placeholder = "Wdh", Keyboard = Keyboard.Numeric };
                var statusBox = new BoxView { WidthRequest = 20, HeightRequest = 20, Color = Colors.Gray, VerticalOptions = LayoutOptions.Center };
                var checkBox = new CheckBox { Color = Color.FromArgb("#E8FF47"), VerticalOptions = LayoutOptions.Center };

                repsEntry.TextChanged += (s, e) =>
                {
                    UpdateStatusColor(statusBox, repsEntry.Text);
                };

                checkBox.CheckedChanged += async (s, e) =>
                {
                    if (e.Value)
                        await SaveSingleSetAsync(setIndex);
                };

                // prefill suggested weight if available
                if (suggestedWeight > 0)
                    weightEntry.Text = suggestedWeight.ToString("0.##");

                _weightEntries.Add(weightEntry);
                _repsEntries.Add(repsEntry);
                _checkBoxes.Add(checkBox);

                grid.Add(weightEntry, 0, 0);
                grid.Add(repsEntry, 1, 0);
                grid.Add(statusBox, 2, 0);
                grid.Add(checkBox, 3, 0);

                SetsContainer.Children.Add(grid);
            }
        }

        private void UpdateStatusColor(BoxView box, string? repsText)
        {
            if (int.TryParse(repsText, out var reps))
            {
                if (reps >= _targetReps)
                    box.Color = Colors.Green;
                else
                    box.Color = Colors.Red;
            }
            else
            {
                box.Color = Colors.Gray;
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
            await _databaseService.InitializeAsync();

            var exercise = await _databaseService.GetExerciseAsync(exerciseId);
            if (exercise == null)
            {
                await DisplayAlert("Fehler", "Übung nicht gefunden.", "OK");
                return;
            }

            _exercise = exercise;
            ExerciseNameLabel.Text = _exercise.Name;
            // load recent logs to show history and suggest weight
            var logs = await _databaseService.GetLogsForExerciseAsync(exerciseId);
            var allLogs = logs.OrderByDescending(l => l.Timestamp).ToList();
            var recent = allLogs.Where(l => l.SetNumber > 0).Take(8).ToList();
            PreviousLogsCollection.ItemsSource = recent;

            double suggestedWeight = 0;
            if (recent.Count > 0)
            {
                suggestedWeight = recent.First().Weight;
            }

            // Pre-fill notes editor with the most recent note (search all logs)
            var lastNote = allLogs.FirstOrDefault(l => !string.IsNullOrWhiteSpace(l.Notes))?.Notes;
            if (!string.IsNullOrWhiteSpace(lastNote))
                NotesEditor.Text = lastNote;

            // Show previous notes (max 5, delete oldest if more)
            var previousNotes = allLogs
                .Where(l => !string.IsNullOrWhiteSpace(l.Notes))
                .GroupBy(l => l.Notes)
                .Select(g => g.First())
                .OrderByDescending(l => l.Timestamp)
                .ToList();

            // Enforce max 5 notes: delete oldest if exceeded
            if (previousNotes.Count > 5)
            {
                var toDelete = previousNotes.Skip(5).ToList();
                foreach (var old in toDelete)
                {
                    old.Notes = string.Empty;
                    await _databaseService.UpdateExerciseLogAsync(old);
                }
                previousNotes = previousNotes.Take(5).ToList();
            }

            if (previousNotes.Count > 0)
            {
                PreviousNotesHeader.IsVisible = true;
                PreviousNotesCollection.IsVisible = true;
                PreviousNotesCollection.ItemsSource = previousNotes;
            }

            BuildSetInputs(suggestedWeight);
        }


        #region Rest Timer

        private void OnTimerPreset(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is string param && int.TryParse(param, out var seconds))
            {
                _restTimerPreset = seconds;
                _restTimeRemaining = seconds;
                UpdateTimerDisplay();

                // Highlight selected preset
                try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); } catch { }
            }
        }

        private void OnStartTimerClicked(object? sender, EventArgs e)
        {
            if (_timerRunning)
            {
                // Pause
                _restTimer?.Stop();
                _timerRunning = false;
                StartTimerButton.Text = "? Weiter";
            }
            else
            {
                // Start
                if (_restTimeRemaining <= 0)
                    _restTimeRemaining = _restTimerPreset;

                _restTimer?.Start();
                _timerRunning = true;
                StartTimerButton.Text = "? Pause";
            }
        }

        private void OnResetTimerClicked(object? sender, EventArgs e)
        {
            _restTimer?.Stop();
            _timerRunning = false;
            _restTimeRemaining = _restTimerPreset;
            UpdateTimerDisplay();
            StartTimerButton.Text = "? Start";
            TimerLabel.TextColor = Color.FromArgb("#E8FF47");
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            _restTimeRemaining--;

            if (_restTimeRemaining <= 0)
            {
                _restTimeRemaining = 0;
                _restTimer?.Stop();
                _timerRunning = false;
                StartTimerButton.Text = "? Start";

                // Notify user: timer done
                TimerLabel.TextColor = Color.FromArgb("#00AA00");
                try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); } catch { }
                try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(500)); } catch { }
            }
            else if (_restTimeRemaining <= 5)
            {
                // Warning color in last 5 seconds
                TimerLabel.TextColor = Color.FromArgb("#FF4444");
            }

            UpdateTimerDisplay();
        }

        private void UpdateTimerDisplay()
        {
            var minutes = _restTimeRemaining / 60;
            var seconds = _restTimeRemaining % 60;
            TimerLabel.Text = $"{minutes:D2}:{seconds:D2}";
        }

        #endregion

        private async Task EnsureWorkoutSessionAsync()
        {
            await _databaseService.InitializeAsync();

            if (_workoutSessionId == 0)
            {
                var session = new WorkoutSession();
                session.Exercises = new List<Exercise> { _exercise };
                await _databaseService.AddWorkoutSessionAsync(session);
                _workoutSessionId = session.Id;
            }
        }

        private async Task RefreshPreviousLogsAsync()
        {
            var logs = await _databaseService.GetLogsForExerciseAsync(_exercise.Id);
            var recent = logs.OrderByDescending(l => l.Timestamp).Where(l => l.SetNumber > 0).Take(8).ToList();
            PreviousLogsCollection.ItemsSource = recent;
        }

        private async void OnEditLogSwipe(object? sender, EventArgs e)
        {
            if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is ExerciseLog log)
            {
                var weightResult = await DisplayPromptAsync("Gewicht bearbeiten", "Neues Gewicht (kg):", initialValue: log.Weight.ToString("0.##"), keyboard: Keyboard.Numeric);
                if (weightResult == null) return;

                var repsResult = await DisplayPromptAsync("Wiederholungen bearbeiten", "Neue Wiederholungen:", initialValue: log.Reps.ToString(), keyboard: Keyboard.Numeric);
                if (repsResult == null) return;

                var notesResult = await DisplayPromptAsync("Notiz bearbeiten", "Notiz:", initialValue: log.Notes ?? string.Empty);
                if (notesResult == null) return;

                if (double.TryParse(weightResult, out var newWeight))
                    log.Weight = newWeight;
                if (int.TryParse(repsResult, out var newReps))
                    log.Reps = newReps;
                log.Notes = notesResult.Trim();

                await _databaseService.UpdateExerciseLogAsync(log);
                await RefreshPreviousLogsAsync();
            }
        }

        private async void OnDeleteNoteSwipe(object? sender, EventArgs e)
        {
            if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is ExerciseLog log)
            {
                var confirm = await DisplayAlert("Notiz löschen", $"Notiz '{log.Notes}' wirklich löschen?", "Ja", "Nein");
                if (!confirm) return;

                // Clear the note text from the log
                log.Notes = string.Empty;
                await _databaseService.UpdateExerciseLogAsync(log);

                // Refresh notes display
                await RefreshPreviousNotesAsync();
            }
        }

        private async Task RefreshPreviousNotesAsync()
        {
            var logs = await _databaseService.GetLogsForExerciseAsync(_exercise.Id);
            var allLogs = logs.OrderByDescending(l => l.Timestamp).ToList();
            var previousNotes = allLogs
                .Where(l => !string.IsNullOrWhiteSpace(l.Notes))
                .GroupBy(l => l.Notes)
                .Select(g => g.First())
                .OrderByDescending(l => l.Timestamp)
                .Take(5)
                .ToList();

            PreviousNotesHeader.IsVisible = previousNotes.Count > 0;
            PreviousNotesCollection.IsVisible = previousNotes.Count > 0;
            PreviousNotesCollection.ItemsSource = previousNotes;
        }

        private async void OnDeleteLogSwipe(object? sender, EventArgs e)
        {
            if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is ExerciseLog log)
            {
                var confirm = await DisplayAlert("Löschen", $"Eintrag (Satz {log.SetNumber}: {log.Weight} kg x {log.Reps}) wirklich löschen?", "Ja", "Nein");
                if (!confirm) return;

                await _databaseService.DeleteExerciseLogAsync(log);
                await RefreshPreviousLogsAsync();
            }
        }

        private async Task SaveSingleSetAsync(int setIndex)
        {
            try
            {
                await EnsureWorkoutSessionAsync();

                var repsText = _repsEntries[setIndex].Text;
                var weightText = _weightEntries[setIndex].Text;

                if (!int.TryParse(repsText, out var reps))
                    reps = 0;
                if (!double.TryParse(weightText, out var weight))
                    weight = 0;

                var log = new ExerciseLog
                {
                    ExerciseId = _exercise.Id,
                    WorkoutSessionId = _workoutSessionId,
                    SetNumber = setIndex + 1,
                    Reps = reps,
                    Weight = weight,
                    Timestamp = DateTime.UtcNow,
                    Notes = string.Empty
                };

                await _databaseService.AddExerciseLogAsync(log);
                _savedLogIds[setIndex] = log.Id;

                // Disable the row so it can't be edited after saving
                _weightEntries[setIndex].IsEnabled = false;
                _repsEntries[setIndex].IsEnabled = false;
                _checkBoxes[setIndex].IsEnabled = false;

                // Refresh the previous logs list so the user sees the saved entry immediately
                await RefreshPreviousLogsAsync();

                try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); } catch { }
            }
            catch (Exception ex)
            {
                // Uncheck on failure so the user can retry
                _checkBoxes[setIndex].IsChecked = false;
                await DisplayAlert("Fehler", ex.Message, "OK");
            }
        }

        private async void OnSaveClicked(object? sender, EventArgs e)
        {
            try
            {
                var notes = NotesEditor.Text?.Trim();

                if (!string.IsNullOrEmpty(notes))
                {
                    await EnsureWorkoutSessionAsync();

                    if (_savedLogIds.Count > 0)
                    {
                        // Update notes on all logs that were saved via checkbox
                        var logs = await _databaseService.GetLogsForWorkoutSessionAsync(_workoutSessionId);
                        foreach (var kvp in _savedLogIds)
                        {
                            var log = logs.FirstOrDefault(l => l.Id == kvp.Value);
                            if (log != null)
                            {
                                log.Notes = notes;
                                await _databaseService.UpdateExerciseLogAsync(log);
                            }
                        }
                    }
                    else
                    {
                        // No sets saved yet – save the note as a standalone log entry
                        var log = new ExerciseLog
                        {
                            ExerciseId = _exercise.Id,
                            WorkoutSessionId = _workoutSessionId,
                            SetNumber = 0,
                            Reps = 0,
                            Weight = 0,
                            Timestamp = DateTime.UtcNow,
                            Notes = notes
                        };
                        await _databaseService.AddExerciseLogAsync(log);
                    }
                }

                await DisplayAlert("Gespeichert", "Einträge wurden gespeichert.", "OK");
                if (Shell.Current != null)
                    await Shell.Current.GoToAsync("..");
                else
                    await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fehler", ex.Message, "OK");
            }
        }
    }
}
