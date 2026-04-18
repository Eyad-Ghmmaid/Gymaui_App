using Gymaui_App.Models;
using Gymaui_App.Services;
using Gymaui_App.Utilities;

namespace Gymaui_App.Views
{
    [QueryProperty(nameof(ExerciseId), "ExerciseId")]
    public partial class AddExercisePage : ContentPage
    {
        public const string Route = nameof(AddExercisePage);
        private string _selectedMuscleGroup = string.Empty;
        private readonly DatabaseService _databaseService;
        private Exercise? _editingExercise;
        private bool _isEditMode;

        public string? ExerciseId { get; set; }

        public AddExercisePage(DatabaseService databaseService)
        {
            InitializeComponent();
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));

            // Wire up custom header events using helper
            HeaderEventHelper.SetupHeaderEvents(this);

            // Initialize muscle group collection
            MuscleGroupCollection.ItemsSource = MuscleGroups.All;
        }

        [Obsolete]
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!string.IsNullOrEmpty(ExerciseId) && int.TryParse(ExerciseId, out int id))
            {
                await LoadExerciseForEditAsync(id);
            }
        }

        [Obsolete]
        private async Task LoadExerciseForEditAsync(int exerciseId)
        {
            try
            {
                await _databaseService.InitializeAsync();
                _editingExercise = await _databaseService.GetExerciseAsync(exerciseId);

                if (_editingExercise != null)
                {
                    _isEditMode = true;
                    CustomHeader.Title = "Uebung Bearbeiten";
                    SaveButton.Text = "Aktualisieren";
                    DeleteButton.IsVisible = true;

                    NameEntry.Text = _editingExercise.Name;
                    SetsEntry.Text = _editingExercise.TargetSets.ToString();
                    RepsEntry.Text = _editingExercise.TargetReps.ToString();
                    YouTubeEntry.Text = _editingExercise.YouTubeUrl;

                    if (!string.IsNullOrEmpty(_editingExercise.MuscleGroup))
                    {
                        _selectedMuscleGroup = _editingExercise.MuscleGroup;
                        HighlightSelectedMuscleGroup();
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fehler", ex.Message, "OK");
            }
        }

        [Obsolete]
        private void HighlightSelectedMuscleGroup()
        {
            foreach (var item in MuscleGroupCollection.GetVisualTreeDescendants())
            {
                if (item is Frame f)
                {
                    f.BorderColor = (f.BindingContext as string) == _selectedMuscleGroup
                        ? Color.FromArgb("#E8FF47")
                        : Color.FromArgb("#2A2A2A");
                }
            }
        }

        [Obsolete]
        private void OnMuscleGroupSelected(object? sender, EventArgs e)
        {
            string? muscleGroup = null;

            if (sender is Frame frame)
                muscleGroup = frame.BindingContext as string;
            else if (sender is VisualElement ve)
                muscleGroup = ve.BindingContext as string;

            if (!string.IsNullOrEmpty(muscleGroup))
            {
                _selectedMuscleGroup = muscleGroup;
                MuscleGroupErrorLabel.IsVisible = false;

                // Visual feedback: highlight the selected muscle group
                foreach (var item in MuscleGroupCollection.GetVisualTreeDescendants())
                {
                    if (item is Frame f)
                    {
                        f.BorderColor = (f.BindingContext as string) == muscleGroup
                            ? Color.FromArgb("#E8FF47")
                            : Color.FromArgb("#2A2A2A");
                    }
                }
            }
        }

        private bool ValidateForm()
        {
            bool isValid = true;

            // Clear all error messages
            NameErrorLabel.IsVisible = false;
            MuscleGroupErrorLabel.IsVisible = false;
            SetsErrorLabel.IsVisible = false;
            RepsErrorLabel.IsVisible = false;
            YoutubeErrorLabel.IsVisible = false;

            // Validate Name
            if (string.IsNullOrWhiteSpace(NameEntry.Text) || NameEntry.Text.Length < 2)
            {
                NameErrorLabel.Text = "Mindestens 2 Zeichen erforderlich";
                NameErrorLabel.IsVisible = true;
                isValid = false;
            }

            // Validate Muscle Group
            if (string.IsNullOrWhiteSpace(_selectedMuscleGroup))
            {
                MuscleGroupErrorLabel.Text = "Muskelgruppe auswählen";
                MuscleGroupErrorLabel.IsVisible = true;
                isValid = false;
            }

            // Validate Sets
            if (!int.TryParse(SetsEntry.Text, out int sets) || sets <= 0)
            {
                SetsErrorLabel.Text = "Positive Zahl erforderlich";
                SetsErrorLabel.IsVisible = true;
                isValid = false;
            }

            // Validate Reps
            if (!int.TryParse(RepsEntry.Text, out int reps) || reps <= 0)
            {
                RepsErrorLabel.Text = "Positive Zahl erforderlich";
                RepsErrorLabel.IsVisible = true;
                isValid = false;
            }

            // Validate YouTube URL if provided
            if (!string.IsNullOrWhiteSpace(YouTubeEntry.Text))
            {
                if (!IsValidYouTubeUrl(YouTubeEntry.Text))
                {
                    YoutubeErrorLabel.Text = "Ungültiges YouTube-Format";
                    YoutubeErrorLabel.IsVisible = true;
                    isValid = false;
                }
            }

            return isValid;
        }

        private bool IsValidYouTubeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return true;

            return url.Contains("youtube.com") || url.Contains("youtu.be");
        }

        private async void OnSaveClicked(object? sender, EventArgs e)
        {
            SaveButton.IsEnabled = false;
            try
            {
                if (!ValidateForm())
                {
                    SaveButton.IsEnabled = true;
                    return;
                }

                int sets = int.Parse(SetsEntry.Text);
                int reps = int.Parse(RepsEntry.Text);

                if (_isEditMode && _editingExercise != null)
                {
                    _editingExercise.Name = NameEntry.Text?.Trim() ?? string.Empty;
                    _editingExercise.MuscleGroup = _selectedMuscleGroup;
                    _editingExercise.TargetReps = reps;
                    _editingExercise.TargetSets = sets;
                    _editingExercise.YouTubeUrl = YouTubeEntry.Text?.Trim() ?? string.Empty;

                    await _databaseService.InitializeAsync();
                    await _databaseService.UpdateExerciseAsync(_editingExercise);

                    await DisplayAlert("Gespeichert", "Uebung wurde aktualisiert.", "OK");
                }
                else
                {
                    var exercise = new Exercise
                    {
                        Name = NameEntry.Text?.Trim() ?? string.Empty,
                        MuscleGroup = _selectedMuscleGroup,
                        TargetReps = reps,
                        TargetSets = sets,
                        YouTubeUrl = YouTubeEntry.Text?.Trim() ?? string.Empty,
                        ImagePath = string.Empty
                    };

                    await _databaseService.InitializeAsync();
                    await _databaseService.AddExerciseAsync(exercise);

                    await DisplayAlert("Gespeichert", "Uebung wurde hinzugefuegt.", "OK");
                }

                // navigate back
                if (Shell.Current != null)
                    await Shell.Current.GoToAsync("..", true);
                else
                    await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fehler", ex.Message, "OK");
            }
            finally
            {
                SaveButton.IsEnabled = true;
            }
        }

        private async void OnDeleteClicked(object? sender, EventArgs e)
        {
            if (_editingExercise == null)
                return;

            bool confirm = await DisplayAlert(
                "Loeschen", $"'{_editingExercise.Name}' wirklich loeschen?", "Ja", "Nein");

            if (!confirm)
                return;

            try
            {
                await _databaseService.InitializeAsync();
                await _databaseService.DeleteExerciseAsync(_editingExercise);

                await DisplayAlert("Geloescht", "Uebung wurde geloescht.", "OK");

                if (Shell.Current != null)
                    await Shell.Current.GoToAsync("..", true);
                else
                    await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fehler", ex.Message, "OK");
            }
        }

        private async void OnCancelClicked(object? sender, EventArgs e)
        {
            if (Shell.Current != null)
                await Shell.Current.GoToAsync("..", true);
            else
                await Navigation.PopAsync();
        }
    }
}
