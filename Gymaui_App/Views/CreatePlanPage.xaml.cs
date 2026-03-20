using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gymaui_App.Models;
using Gymaui_App.Services;
using Gymaui_App.Utilities;

namespace Gymaui_App.Views
{
    public partial class CreatePlanPage : ContentPage
    {
        private readonly DatabaseService _db;
        private int _currentStep = 1;
        private PlanCreationDto _planData = new();
        private WeekDayInfo[] _weekDays = WeekDayInfo.GetWeekDays();
        private List<Exercise> _allExercises = new();

        public CreatePlanPage(DatabaseService db)
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
                _allExercises = await _db.GetExercisesAsync();
                SetupStep1();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load: {ex.Message}", "OK");
            }
        }

        private void SetupStep1()
        {
            ShowStep(1);
            PlanNameEntry.Focus();
        }

        private void SetupStep2()
        {
            ShowStep(2);
            DaysFlexLayout.Children.Clear();

            foreach (var day in _weekDays)
            {
                var dayCard = CreateDayCard(day);
                DaysFlexLayout.Children.Add(dayCard);
            }
        }

        private Border CreateDayCard(WeekDayInfo day)
        {
            var border = new Border
            {
                BackgroundColor = Color.FromArgb("#1A1A1A"),
                Stroke = Color.FromArgb("#2A2A2A"),
                StrokeThickness = 1,
                Padding = new Thickness(12, 8)
            };

            var label = new Label
            {
                Text = day.DayAbbreviation,
                TextColor = Color.FromArgb("#8A8A8A"),
                HorizontalOptions = LayoutOptions.Center,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold
            };

            border.Content = label;

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) => OnDayCardTapped(day, border);
            border.GestureRecognizers.Add(tapGesture);

            return border;
        }

        private void OnDayCardTapped(WeekDayInfo day, Border card)
        {
            day.IsSelected = !day.IsSelected;
            
            if (day.IsSelected)
            {
                _planData.SelectedTrainingDays.Add(day.DayOfWeek);
                card.BackgroundColor = Color.FromArgb("#E8FF47");
                ((Label)card.Content).TextColor = Color.FromArgb("#000000");
            }
            else
            {
                _planData.SelectedTrainingDays.Remove(day.DayOfWeek);
                card.BackgroundColor = Color.FromArgb("#1A1A1A");
                ((Label)card.Content).TextColor = Color.FromArgb("#8A8A8A");
            }
        }

        private async void SetupStep3()
        {
            ShowStep(3);
            await LoadExercisesForDays();
        }

        private async Task LoadExercisesForDays()
        {
            var dayData = new List<DayExercisesViewModel>();

            foreach (var dayOfWeek in _planData.SelectedTrainingDays.OrderBy(d => d))
            {
                var dayName = _weekDays[dayOfWeek].DayName;
                var exercises = new List<Exercise>();
                
                if (_planData.ExercisesPerDay.ContainsKey(dayOfWeek))
                {
                    foreach (var exerciseId in _planData.ExercisesPerDay[dayOfWeek])
                    {
                        var ex = _allExercises.FirstOrDefault(e => e.Id == exerciseId);
                        if (ex != null)
                            exercises.Add(ex);
                    }
                }

                dayData.Add(new DayExercisesViewModel 
                { 
                    DayOfWeek = dayOfWeek,
                    DayName = dayName, 
                    Exercises = exercises 
                });
            }

            DaysExercisesCollection.ItemsSource = dayData;
        }

        private async void OnAddExerciseToDayClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.CommandParameter is int dayOfWeek)
                {
                    // Show a dialog to select exercises
                    var action = await DisplayActionSheet(
                        "Select Exercise", 
                        "Cancel", 
                        null, 
                        _allExercises.Select(e => e.Name).ToArray());
                    
                    if (action != null && action != "Cancel")
                    {
                        var selectedExercise = _allExercises.FirstOrDefault(e => e.Name == action);
                        if (selectedExercise != null)
                        {
                            if (!_planData.ExercisesPerDay.ContainsKey(dayOfWeek))
                            {
                                _planData.ExercisesPerDay[dayOfWeek] = new List<int>();
                            }
                            
                            if (!_planData.ExercisesPerDay[dayOfWeek].Contains(selectedExercise.Id))
                            {
                                _planData.ExercisesPerDay[dayOfWeek].Add(selectedExercise.Id);
                                await LoadExercisesForDays();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to add exercise: {ex.Message}", "OK");
            }
        }

        private async void OnRemoveExerciseClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.CommandParameter is int exerciseId)
                {
                    foreach (var dayExercises in _planData.ExercisesPerDay.Values)
                    {
                        dayExercises.RemoveAll(id => id == exerciseId);
                    }
                    await LoadExercisesForDays();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to remove exercise: {ex.Message}", "OK");
            }
        }

        private void ShowStep(int step)
        {
            _currentStep = step;
            Step1Content.IsVisible = step == 1;
            Step2Content.IsVisible = step == 2;
            Step3Content.IsVisible = step == 3;

            StepLabel.Text = step switch
            {
                1 => "Step 1: Plan Name",
                2 => "Step 2: Select Training Days",
                3 => "Step 3: Add Exercises",
                _ => "Create Plan"
            };

            ProgressBar.Progress = step * 0.33;
            BackButton.IsVisible = step > 1;
            NextButton.Text = step == 3 ? "Create Plan" : "Next";
        }

        private void OnBackClicked(object sender, EventArgs e)
        {
            if (_currentStep > 1)
            {
                _currentStep--;
                if (_currentStep == 1)
                    ShowStep(1);
                else if (_currentStep == 2)
                    SetupStep2();
            }
        }

        private async void OnNextClicked(object sender, EventArgs e)
        {
            try
            {
                if (_currentStep == 1)
                {
                    var planName = PlanNameEntry.Text?.Trim();
                    if (string.IsNullOrEmpty(planName))
                    {
                        await DisplayAlert("Error", "Please enter a plan name", "OK");
                        return;
                    }

                    _planData.PlanName = planName;
                    SetupStep2();
                }
                else if (_currentStep == 2)
                {
                    if (_planData.SelectedTrainingDays.Count == 0)
                    {
                        await DisplayAlert("Error", "Please select at least one training day", "OK");
                        return;
                    }

                    SetupStep3();
                }
                else if (_currentStep == 3)
                {
                    await CreatePlan();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async Task CreatePlan()
        {
            try
            {
                // Create plan
                var plan = new Plan 
                { 
                    Name = _planData.PlanName, 
                    Created = DateTime.UtcNow, 
                    IsActive = true 
                };
                await _db.AddPlanAsync(plan);

                // Deactivate other plans
                await _db.SetActivePlanAsync(plan.Id);

                // Create plan days
                int order = 0;
                foreach (var dayOfWeek in _planData.SelectedTrainingDays.OrderBy(d => d))
                {
                    var dayName = _weekDays[dayOfWeek].DayName;
                    var planDay = new PlanDay
                    {
                        PlanId = plan.Id,
                        DayOfWeek = dayOfWeek,
                        Name = dayName,
                        IsTrainingDay = true,
                        Order = order++
                    };
                    await _db.AddPlanDayAsync(planDay);

                    // Add exercises for this day
                    if (_planData.ExercisesPerDay.ContainsKey(dayOfWeek))
                    {
                        int exerciseOrder = 1;
                        foreach (var exerciseId in _planData.ExercisesPerDay[dayOfWeek])
                        {
                            var planExercise = new PlanExercise
                            {
                                PlanDayId = planDay.Id,
                                ExerciseId = exerciseId,
                                Order = exerciseOrder++
                            };
                            await _db.AddPlanExerciseAsync(planExercise);
                        }
                    }
                }

                await DisplayAlert("Success", $"Plan '{_planData.PlanName}' created!", "OK");
                await Shell.Current.GoToAsync("///plans");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to create plan: {ex.Message}", "OK");
            }
        }
    }

    public class DayExercisesViewModel
    {
        public int DayOfWeek { get; set; }
        public string DayName { get; set; } = string.Empty;
        public List<Exercise> Exercises { get; set; } = new();
    }
}
