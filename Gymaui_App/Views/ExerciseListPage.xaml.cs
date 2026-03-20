using System;
using Microsoft.Maui.Controls;
using Gymaui_App.Models;

namespace Gymaui_App.Views
{
    public partial class ExerciseListPage : ContentPage
    {
        private readonly ViewModels.ExerciseViewModel _viewModel;

        public ExerciseListPage(ViewModels.ExerciseViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel ?? throw new System.ArgumentNullException(nameof(viewModel));
            BindingContext = _viewModel;
        }

        private async void OnStatsClicked(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is int id)
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync($"{nameof(StatisticsPage)}?ExerciseId={id}");
                    return;
                }
            }

            await DisplayAlert("Fehler", "Statistik nicht verfügbar.", "OK");
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

        private async void OnItemTapped(object? sender, EventArgs e)
        {
            try
            {
                // the BindingContext of the tapped Frame is the Exercise
                if (sender is VisualElement ve && ve.BindingContext is Exercise exercise)
                {
                    if (Shell.Current != null)
                    {
                        await Shell.Current.GoToAsync($"//{nameof(ActiveWorkoutPage)}?ExerciseId={exercise.Id}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                await DisplayAlert("Fehler", ex.Message, "OK");
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
                    // fallback: push page directly
                    await Navigation.PushAsync(new AddExercisePage());
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fehler", ex.Message, "OK");
            }
        }

        private async void OnEditClicked(object? sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                var exercise = btn.CommandParameter as Exercise;
                if (exercise != null)
                {
                    await DisplayAlert("Edit", $"Edit {exercise.Name}", "OK");
                    return;
                }
            }

            await DisplayAlert("Edit", "Exercise not found.", "OK");
        }
    }
}
