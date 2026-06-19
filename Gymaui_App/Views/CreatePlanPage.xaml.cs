using Gymaui_App.Models;
using Gymaui_App.Services;
using Gymaui_App.Utilities;

namespace Gymaui_App.Views
{
    public partial class CreatePlanPage : ContentPage
    {
        public const string Route = nameof(CreatePlanPage);
        private readonly DatabaseService _db;
        private int _currentStep = 1;
        private PlanCreationDto _planData = new();
        private HashSet<int> _selectedDays = new(); // 0-based indices of selected training days
        private List<Exercise> _allExercises = new();
        private bool _isInitialized = false;

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

                if (!_isInitialized)
                {
                    _isInitialized = true;
                    SetupStep1();
                }
                else if (_currentStep == 3)
                {
                    // Refresh exercises list when returning from the picker popup
                    LoadExercisesForDays();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fehler", $"Laden fehlgeschlagen: {ex.Message}", "OK");
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

            // Show toggle buttons for Tag 1 - Tag 7
            for (int i = 0; i < 7; i++)
            {
                var dayIndex = i;
                var isSelected = _selectedDays.Contains(dayIndex);
                var border = new Border
                {
                    BackgroundColor = Color.FromArgb(isSelected ? "#E8FF47" : "#1A1A1A"),
                    Stroke = Color.FromArgb("#2A2A2A"),
                    StrokeThickness = 1,
                    Padding = new Thickness(12, 8),
                    Margin = new Thickness(4)
                };

                var stack = new VerticalStackLayout
                {
                    Spacing = 2,
                    HorizontalOptions = LayoutOptions.Center
                };

                var dayLabel = new Label
                {
                    Text = $"Tag {dayIndex + 1}",
                    TextColor = Color.FromArgb(isSelected ? "#000000" : "#FFFFFF"),
                    HorizontalOptions = LayoutOptions.Center,
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold
                };

                var statusLabel = new Label
                {
                    Text = isSelected ? "Training" : "Pause",
                    TextColor = Color.FromArgb(isSelected ? "#000000" : "#8A8A8A"),
                    HorizontalOptions = LayoutOptions.Center,
                    FontSize = 10
                };

                stack.Children.Add(dayLabel);
                stack.Children.Add(statusLabel);
                border.Content = stack;

                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (s, e) => OnDayToggleTapped(dayIndex);
                border.GestureRecognizers.Add(tapGesture);

                DaysFlexLayout.Children.Add(border);
            }
        }

        private void OnDayToggleTapped(int dayIndex)
        {
            // Toggle the day selection
            if (_selectedDays.Contains(dayIndex))
                _selectedDays.Remove(dayIndex);
            else
                _selectedDays.Add(dayIndex);

            // Update plan data
            _planData.SelectedTrainingDays = _selectedDays.OrderBy(d => d).ToList();
            _planData.NumberOfDays = _selectedDays.Count;

            // Refresh the UI
            SetupStep2Visual();
        }

        private void SetupStep2Visual()
        {
            // Update all cards visual state
            for (int i = 0; i < DaysFlexLayout.Children.Count; i++)
            {
                if (DaysFlexLayout.Children[i] is Border card && card.Content is VerticalStackLayout stack)
                {
                    var isSelected = _selectedDays.Contains(i);
                    card.BackgroundColor = Color.FromArgb(isSelected ? "#E8FF47" : "#1A1A1A");

                    if (stack.Children.Count >= 2)
                    {
                        if (stack.Children[0] is Label dayLbl)
                            dayLbl.TextColor = Color.FromArgb(isSelected ? "#000000" : "#FFFFFF");
                        if (stack.Children[1] is Label statusLbl)
                        {
                            statusLbl.Text = isSelected ? "Training" : "Pause";
                            statusLbl.TextColor = Color.FromArgb(isSelected ? "#000000" : "#8A8A8A");
                        }
                    }
                }
            }
        }

        private void SetupStep3()
        {
            ShowStep(3);
            LoadExercisesForDays();
        }

        private void LoadExercisesForDays()
        {
            var dayData = new List<DayExercisesViewModel>();

            foreach (var dayIndex in _planData.SelectedTrainingDays.OrderBy(d => d))
            {
                var dayName = $"Tag {dayIndex + 1}";
                var exercises = new List<Exercise>();

                if (_planData.ExercisesPerDay.ContainsKey(dayIndex))
                {
                    foreach (var (exerciseId, targetSets, targetReps) in _planData.ExercisesPerDay[dayIndex])
                    {
                        var ex = _allExercises.FirstOrDefault(e => e.Id == exerciseId);
                        if (ex != null)
                            exercises.Add(ex);
                    }
                }

                dayData.Add(new DayExercisesViewModel
                {
                    DayOfWeek = dayIndex,
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
                    // Show searchable exercise picker popup
                    var popup = new ExercisePickerPopup(_allExercises);
                    await Navigation.PushModalAsync(popup, animated: true);
                    var selectedExercise = await popup.Result;

                    if (selectedExercise != null)
                    {
                        // Benutzer auffordern, Sätze einzugeben
                        string setsInput = await DisplayPromptAsync(
                            "Sätze eingeben",
                            $"Anzahl der Sätze für {selectedExercise.Name}:",
                            placeholder: "z.B. 3",
                            maxLength: 2,
                            keyboard: Keyboard.Numeric
                        );

                        if (string.IsNullOrWhiteSpace(setsInput) || !int.TryParse(setsInput, out int sets) || sets < 1)
                        {
                            await DisplayAlert("Ungültig", "Bitte geben Sie eine Zahl größer als 0 ein.", "OK");
                            return;
                        }

                        // Benutzer auffordern, Wiederholungen einzugeben
                        string repsInput = await DisplayPromptAsync(
                            "Wiederholungen eingeben",
                            $"Anzahl der Wiederholungen für {selectedExercise.Name}:",
                            placeholder: "z.B. 10",
                            maxLength: 2,
                            keyboard: Keyboard.Numeric
                        );

                        if (string.IsNullOrWhiteSpace(repsInput) || !int.TryParse(repsInput, out int reps) || reps < 1)
                        {
                            await DisplayAlert("Ungültig", "Bitte geben Sie eine Zahl größer als 0 ein.", "OK");
                            return;
                        }

                        if (!_planData.ExercisesPerDay.ContainsKey(dayOfWeek))
                        {
                            _planData.ExercisesPerDay[dayOfWeek] = new List<(int, int, int)>();
                        }

                        // Check if exercise already exists
                        if (!_planData.ExercisesPerDay[dayOfWeek].Any(ex => ex.ExerciseId == selectedExercise.Id))
                        {
                            _planData.ExercisesPerDay[dayOfWeek].Add((selectedExercise.Id, sets, reps));
                            LoadExercisesForDays();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fehler", $"Übung konnte nicht hinzugefügt werden: {ex.Message}", "OK");
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
                        dayExercises.RemoveAll(ex => ex.ExerciseId == exerciseId);
                    }
                    LoadExercisesForDays();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fehler", $"Übung konnte nicht entfernt werden: {ex.Message}", "OK");
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
                1 => "Schritt 1: Plan Name",
                2 => "Schritt 2: Trainingstage auswählen",
                3 => "Schritt 3: Übungen hinzufügen",
                _ => "Plan erstellen"
            };

            ProgressBar.Progress = step * 0.33;
            BackButton.IsVisible = step > 1;
            NextButton.Text = step == 3 ? "Plan erstellen" : "Weiter";
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
                        await DisplayAlert("Fehler", "Bitte gib einen Plan-Namen ein", "OK");
                        return;
                    }

                    _planData.PlanName = planName;
                    SetupStep2();
                }
                else if (_currentStep == 2)
                {
                    if (_selectedDays.Count == 0)
                    {
                        await DisplayAlert("Fehler", "Bitte wähle mindestens einen Trainingstag aus", "OK");
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
                await DisplayAlert("Fehler", $"Ein Fehler ist aufgetreten: {ex.Message}", "OK");
            }
        }

        private async Task CreatePlan()
        {
            try
            {
                // Create plan with local date (start of today)
                var plan = new Plan
                {
                    Name = _planData.PlanName,
                    Created = DateTime.Now.Date,
                    IsActive = true
                };
                await _db.AddPlanAsync(plan);

                // Deactivate other plans
                await _db.SetActivePlanAsync(plan.Id);

                // Create all 7 days in the cycle
                for (int dayIndex = 0; dayIndex < 7; dayIndex++)
                {
                    bool isTraining = _selectedDays.Contains(dayIndex);
                    var dayName = isTraining ? $"Tag {dayIndex + 1}" : $"Tag {dayIndex + 1} (Pause)";
                    var planDay = new PlanDay
                    {
                        PlanId = plan.Id,
                        DayOfWeek = dayIndex,
                        Name = dayName,
                        IsTrainingDay = isTraining,
                        Order = dayIndex
                    };
                    await _db.AddPlanDayAsync(planDay);

                    // Add exercises for training days
                    if (isTraining && _planData.ExercisesPerDay.ContainsKey(dayIndex))
                    {
                        int exerciseOrder = 1;
                        foreach (var (exerciseId, targetSets, targetReps) in _planData.ExercisesPerDay[dayIndex])
                        {
                            var planExercise = new PlanExercise
                            {
                                PlanDayId = planDay.Id,
                                ExerciseId = exerciseId,
                                Order = exerciseOrder++,
                                TargetSets = targetSets,
                                TargetReps = targetReps
                            };
                            await _db.AddPlanExerciseAsync(planExercise);
                        }
                    }
                }

                await DisplayAlert("Erfolg", $"Plan '{_planData.PlanName}' erstellt!", "OK");
                await Shell.Current.GoToAsync("///plans");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fehler", $"Plan konnte nicht erstellt werden: {ex.Message}", "OK");
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
