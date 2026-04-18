using Gymaui_App.Models;

namespace Gymaui_App.Views
{
    public partial class ExercisePickerPopup : ContentPage
    {
        private readonly List<Exercise> _allExercises;
        private readonly TaskCompletionSource<Exercise?> _tcs = new();

        public Task<Exercise?> Result => _tcs.Task;

        public ExercisePickerPopup(List<Exercise> exercises)
        {
            InitializeComponent();
            _allExercises = exercises ?? new List<Exercise>();
            ExerciseListView.ItemsSource = _allExercises;
        }

        private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
        {
            var searchText = e.NewTextValue?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(searchText))
            {
                ExerciseListView.ItemsSource = _allExercises;
            }
            else
            {
                ExerciseListView.ItemsSource = _allExercises
                    .Where(ex => ex.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        private async void OnExerciseTapped(object? sender, EventArgs e)
        {
            if (sender is BindableObject bo && bo.BindingContext is Exercise exercise)
            {
                _tcs.TrySetResult(exercise);
                await Navigation.PopModalAsync(animated: true);
            }
        }

        private async void OnCancelClicked(object? sender, EventArgs e)
        {
            _tcs.TrySetResult(null);
            await Navigation.PopModalAsync(animated: true);
        }

        protected override bool OnBackButtonPressed()
        {
            _tcs.TrySetResult(null);
            return base.OnBackButtonPressed();
        }
    }
}
