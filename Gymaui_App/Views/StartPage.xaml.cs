using System;
using System;
using System.Collections.Generic;
using System.Linq;
using Gymaui_App.Models;
using Gymaui_App.Services;
using Gymaui_App.Utilities;
using Microsoft.Maui.Controls;

namespace Gymaui_App.Views
{
    public partial class StartPage : ContentPage
    {
        private readonly DatabaseService _db;

        public StartPage(DatabaseService db)
        {
            InitializeComponent();
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                await _db.InitializeAsync();

                var active = await _db.GetActivePlanAsync();
                if (active == null)
                {
                    MessageLabel.Text = "No active training plan.";
                    CreatePlanButton.IsVisible = true;
                    ActivePlanLayout.IsVisible = false;
                }
                else
                {
                    MessageLabel.Text = string.Empty;
                    PlanNameLabel.Text = active.Name;
                    CreatePlanButton.IsVisible = false;
                    ActivePlanLayout.IsVisible = true;
                    
                    // Load today's workout info
                    await LoadTodayWorkout(active);
                }
            }
            catch (Exception ex)
            {
                MessageLabel.Text = $"Error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"StartPage Error: {ex}");
            }
        }

        private async Task LoadTodayWorkout(Plan activePlan)
        {
            try
            {
                var today = DateTime.Now.DayOfWeek;
                var dayOfWeek = (int)((int)today + 6) % 7; // Convert to 0=Monday
                var dayNames = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
                
                TodayLabel.Text = $"Today: {dayNames[dayOfWeek]}";
                
                var planDay = await _db.GetPlanDayByDayOfWeekAsync(activePlan.Id, dayOfWeek);
                
                if (planDay != null && planDay.IsTrainingDay)
                {
                    TodayStatusLabel.Text = "Training day ??";
                    var planExercises = await _db.GetExercisesForDayAsync(planDay.Id);
                    
                    var exercises = new List<Exercise>();
                    foreach (var pe in planExercises.OrderBy(pe => pe.Order))
                    {
                        var ex = await _db.GetExerciseAsync(pe.ExerciseId);
                        if (ex != null)
                            exercises.Add(ex);
                    }
                    
                    TodayExercisesCollection.ItemsSource = exercises;
                    StartTrainingButton.IsEnabled = exercises.Count > 0;
                }
                else
                {
                    TodayStatusLabel.Text = "Rest day ??";
                    TodayExercisesCollection.ItemsSource = new List<Exercise>();
                    StartTrainingButton.IsEnabled = false;
                    StartTrainingButton.Text = "Rest day";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading today's workout: {ex.Message}");
            }
        }

        private async void OnCreatePlanClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("createplan");
        }

        private async void OnStartTrainingClicked(object sender, EventArgs e)
        {
            try
            {
                await _db.InitializeAsync();

                var planDay = await _db.GetTodaysPlanDayAsync();
                if (planDay == null || !planDay.IsTrainingDay)
                {
                    await DisplayAlert("Rest Day", "Today is a rest day!", "OK");
                    return;
                }

                // Gather exercises for today
                var planExercises = await _db.GetExercisesForDayAsync(planDay.Id);
                var exercises = new List<Exercise>();
                foreach (var pe in planExercises.OrderBy(pe => pe.Order))
                {
                    var ex = await _db.GetExerciseAsync(pe.ExerciseId);
                    if (ex != null)
                        exercises.Add(ex);
                }

                if (exercises.Count == 0)
                {
                    await DisplayAlert("No Exercises", "No exercises scheduled for today.", "OK");
                    return;
                }

                // Create workout session
                var session = new WorkoutSession { Date = DateTime.UtcNow, Exercises = exercises };
                await _db.AddWorkoutSessionAsync(session);

                // Store and navigate
                AppShell.PendingWorkoutSessionId = session.Id;
                await AppShell.NavigateToTab("workout");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to start training: {ex.Message}", "OK");
            }
        }
    }
}
