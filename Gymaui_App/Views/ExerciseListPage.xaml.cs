using Gymaui_App.Models;
using Gymaui_App.Utilities;
using Gymaui_App.ViewModels;

namespace Gymaui_App.Views
{
    public partial class ExerciseListPage : ContentPage
    {
        private readonly ExerciseListViewModel _viewModel;

        public ExerciseListPage(ExerciseListViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel ?? throw new System.ArgumentNullException(nameof(viewModel));
            BindingContext = _viewModel;

            // Wire up custom header events using helper
            HeaderEventHelper.SetupHeaderEvents(this);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                await _viewModel.LoadExercisesAsync();
            }
            catch (System.Exception ex)
            {
                await DisplayAlert("Fehler", ex.Message, "OK");
            }
        }

        private async void OnEditExerciseSwipe(object? sender, EventArgs e)
        {
            try
            {
                if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is Exercise exercise)
                {
                    if (Shell.Current != null)
                    {
                        await Shell.Current.GoToAsync($"{nameof(AddExercisePage)}?ExerciseId={exercise.Id}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                await DisplayAlert("Fehler", ex.Message, "OK");
            }
        }

        private async void OnDeleteExerciseSwipe(object? sender, EventArgs e)
        {
            try
            {
                if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is Exercise exercise)
                {
                    bool confirm = await DisplayAlert(
                        "Loeschen", $"'{exercise.Name}' wirklich loeschen?", "Ja", "Nein");
                    if (confirm)
                    {
                        await _viewModel.DeleteExerciseAsync(exercise);
                    }
                }
            }
            catch (System.Exception ex)
            {
                await DisplayAlert("Fehler", ex.Message, "OK");
            }
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
            catch (System.Exception ex)
            {
                await DisplayAlert("Fehler", $"Die URL konnte nicht geoeffnet werden: {ex.Message}", "OK");
            }
        }

        private async void OnExerciseSelected(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection != null && e.CurrentSelection.Count > 0)
            {
                var exercise = e.CurrentSelection[0] as Exercise;
                if (exercise != null)
                {
                    if (Shell.Current != null)
                    {
                        // navigate to ActiveWorkoutPage and pass the ExerciseId as query parameter
                        var route = $"//{nameof(ActiveWorkoutPage)}?ExerciseId={exercise.Id}";
                        await Shell.Current.GoToAsync(route);
                    }
                }

                // clear selection
                if (sender is CollectionView cv)
                    cv.SelectedItem = null;
            }
        }

        [Obsolete]
        private void OnMuscleGroupChipTapped(object? sender, EventArgs e)
        {
            if (sender is Frame frame && frame.BindingContext is string muscleGroup)
            {
                _viewModel.SelectedMuscleGroup = muscleGroup;
                UpdateChipColors();
            }
        }

        [Obsolete]
        private void UpdateChipColors()
        {
            foreach (var child in MuscleGroupChips.Children)
            {
                if (child is Frame chipFrame && chipFrame.BindingContext is string chipMuscleGroup)
                {
                    bool isSelected = chipMuscleGroup == _viewModel.SelectedMuscleGroup;
                    chipFrame.BackgroundColor = isSelected
                        ? Color.FromArgb("#E8FF47")
                        : Color.FromArgb("#242424");

                    if (chipFrame.Content is Label label)
                    {
                        label.TextColor = isSelected
                            ? Color.FromArgb("#000000")
                            : Color.FromArgb("#8A8A8A");
                    }
                }
            }
        }

        private async void OnAddClicked(object? sender, EventArgs e)
        {
            try
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync(nameof(AddExercisePage));
                }
                else
                {
                    await Navigation.PushAsync(new AddExercisePage(new Services.DatabaseService()));
                }
            }
            catch (System.Exception ex)
            {
                await DisplayAlert("Fehler", ex.Message, "OK");
            }
        }
    }
}

