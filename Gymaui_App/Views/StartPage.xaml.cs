using Gymaui_App.Models;
using Gymaui_App.Services;
using Gymaui_App.Utilities;
using Microsoft.Maui.Controls.Shapes;

namespace Gymaui_App.Views
{
    public partial class StartPage : ContentPage
    {
        private readonly DatabaseService _db;
        private readonly CalendarService _calendarService;
        private DateTime _currentCalendarDate;

        private static readonly string[] MotivationQuotes = new[]
        {
            "Der einzige schlechte Trainingstag ist der, der nicht stattfindet.",
            "Disziplin schlaegt Motivation. Jeden Tag.",
            "Staerke kommt nicht vom Gewinnen. Sie kommt vom Kampf.",
            "Dein Koerper kann fast alles. Ueberzeuge deinen Kopf.",
            "Erfolg ist die Summe kleiner Anstrengungen, Tag fuer Tag.",
            "Trainiere nicht bis du muede bist. Trainiere bis du fertig bist.",
            "Der Schmerz den du heute fuehlst, wird die Staerke von morgen.",
            "Kein Erfolg ohne Schweiss. Bleib dran!",
            "Die beste Zeit zu trainieren ist jetzt.",
            "Jeder Satz zaehlt. Jede Wiederholung zaehlt."
        };

        public StartPage(DatabaseService db, CalendarService calendarService)
        {
            InitializeComponent();
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _calendarService = calendarService ?? throw new ArgumentNullException(nameof(calendarService));
            _currentCalendarDate = DateTime.Now;

            HeaderEventHelper.SetupHeaderEvents(this);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDashboardAsync();
        }

        private async Task LoadDashboardAsync()
        {
            try
            {
                await _db.InitializeAsync();

                // Greeting based on time of day
                var hour = DateTime.Now.Hour;
                GreetingLabel.Text = hour switch
                {
                    < 12 => "Guten Morgen!",
                    < 18 => "Guten Tag! ",
                    _ => "Guten Abend! "
                };

                // Random motivation quote
                var random = new Random();
                MotivationLabel.Text = $"\"{MotivationQuotes[random.Next(MotivationQuotes.Length)]}\"";

                // Load streak & stats
                var streak = await _calendarService.GetCurrentStreakAsync();
                StreakCountLabel.Text = streak.ToString();
                StreakEmojiLabel.Text = streak >= 3 ? "" : (streak > 0 ? "" : "");

                var (remaining, totalExercises) = await _calendarService.GetRemainingExercisesTodayAsync();
                if (totalExercises > 0)
                {
                    TotalWorkoutsLabel.Text = remaining.ToString();
                    TrainingsSubLabel.Text = remaining == 1 ? "Übung offen" : "Übungen offen";
                }
                else
                {
                    TotalWorkoutsLabel.Text = "✔";
                    TrainingsSubLabel.Text = "Ruhetag";
                }

                // Weekly progress
                var weeklyProgress = await _calendarService.GetWeeklyProgressAsync();
                PopulateWeeklyProgress(weeklyProgress);

                var active = await _db.GetActivePlanAsync();
                if (active == null)
                {
                    MessageLabel.IsVisible = false;
                    NoPlanLayout.IsVisible = true;
                    ActivePlanLayout.IsVisible = false;
                    ActivePlanShortLabel.Text = "-";
                }
                else
                {
                    MessageLabel.IsVisible = false;
                    NoPlanLayout.IsVisible = false;
                    ActivePlanLayout.IsVisible = true;
                    ActivePlanShortLabel.Text = active.Name;
                    PlanNameLabel.Text = $"Plan: {active.Name}";

                    await LoadTodayWorkout(active);
                }

                await LoadCalendarAsync();
            }
            catch (Exception ex)
            {
                MessageLabel.Text = $"Fehler: {ex.Message}";
                MessageLabel.IsVisible = true;
                System.Diagnostics.Debug.WriteLine($"StartPage Error: {ex}");
            }
        }

        private void PopulateWeeklyProgress(List<bool?> weeklyProgress)
        {
            WeeklyProgressGrid.Clear();
            var today = DateTime.Now.Date;
            var monday = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));

            for (int i = 0; i < 7 && i < weeklyProgress.Count; i++)
            {
                var date = monday.AddDays(i);
                var isToday = date == today;
                var status = weeklyProgress[i];

                Color bgColor;
                string icon;

                if (status == true)
                {
                    bgColor = Color.FromArgb("#00AA00");
                    icon = "✔";
                }
                else if (status == false)
                {
                    bgColor = date < today ? Color.FromArgb("#CC0000") : Color.FromArgb("#444444");
                    icon = date < today ? "✖" : "–";
                }
                else
                {
                    bgColor = Color.FromArgb("#2A2A2A");
                    icon = "–";
                }

                var border = new Border
                {
                    StrokeShape = new RoundRectangle { CornerRadius = 8 },
                    Stroke = isToday ? Color.FromArgb("#E8FF47") : Colors.Transparent,
                    StrokeThickness = isToday ? 2 : 0,
                    BackgroundColor = bgColor,
                    HeightRequest = 40,
                    Content = new Label
                    {
                        Text = icon,
                        TextColor = Colors.White,
                        FontSize = 16,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center
                    }
                };

                WeeklyProgressGrid.Add(border, i, 0);
            }
        }

        private async Task LoadTodayWorkout(Plan activePlan)
        {
            try
            {
                var planDay = await _db.GetTodaysPlanDayAsync();

                if (planDay != null && planDay.IsTrainingDay)
                {
                    TodayLabel.Text = $"Heute: {planDay.Name}";
                    TodayStatusLabel.Text = "Trainingstag";
                    TodayBadge.BackgroundColor = Color.FromArgb("#E8FF47");
                    var planExercises = await _db.GetExercisesForDayAsync(planDay.Id);

                    var exerciseIds = planExercises.OrderBy(pe => pe.Order).Select(pe => pe.ExerciseId);
                    var exercises = await _db.GetExercisesByIdsAsync(exerciseIds);

                    // Create snapshot copies that include plan targets so the WorkoutSession remains historical and offline-safe
                    var planMap = planExercises.ToDictionary(pe => pe.ExerciseId);
                    var snapshot = exercises.Select(e => new Exercise
                    {
                        Id = e.Id,
                        Name = e.Name,
                        MuscleGroup = e.MuscleGroup,
                        YouTubeUrl = e.YouTubeUrl,
                        ImagePath = e.ImagePath,
                        TargetSets = planMap.ContainsKey(e.Id) ? planMap[e.Id].TargetSets : 0,
                        TargetReps = planMap.ContainsKey(e.Id) ? planMap[e.Id].TargetReps : 0
                    }).ToList();

                    TodayExercisesCollection.ItemsSource = snapshot;
                    StartTrainingButton.IsEnabled = snapshot.Count > 0;
                    StartTrainingButton.Text = "Training starten";
                }
                else
                {
                    TodayLabel.Text = "Heute";
                    TodayStatusLabel.Text = "Ruhetag";
                    TodayBadge.BackgroundColor = Color.FromArgb("#0066FF");
                    TodayExercisesCollection.ItemsSource = new List<Exercise>();
                    StartTrainingButton.IsEnabled = false;
                    StartTrainingButton.Text = "Ruhetag";
                    StartTrainingButton.BackgroundColor = Color.FromArgb("#2A2A2A");
                    StartTrainingButton.TextColor = Color.FromArgb("#8A8A8A");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading today's workout: {ex.Message}");
            }
        }

        private async void OnRefreshing(object? sender, EventArgs e)
        {
            await LoadDashboardAsync();
            MainRefreshView.IsRefreshing = false;
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
                    await DisplayAlert("Ruhetag", "Heute ist ein Ruhetag!", "OK");
                    return;
                }

                var planExercises = await _db.GetExercisesForDayAsync(planDay.Id);
                var exerciseIds = planExercises.OrderBy(pe => pe.Order).Select(pe => pe.ExerciseId);
                var exercises = await _db.GetExercisesByIdsAsync(exerciseIds);

                if (exercises.Count == 0)
                {
                    await DisplayAlert("Keine Uebungen", "Keine Uebungen fuer heute geplant.", "OK");
                    return;
                }

                var session = new WorkoutSession { Date = DateTime.UtcNow, Name = planDay.Name, Exercises = exercises };
                await _db.AddWorkoutSessionAsync(session);

                AppShell.PendingWorkoutSessionId = session.Id;
                await AppShell.NavigateToTab("workout");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fehler", $"Training konnte nicht gestartet werden: {ex.Message}", "OK");
            }
        }

        #region Calendar

        private async Task LoadCalendarAsync()
        {
            try
            {
                var days = await _calendarService.GetMonthCalendarAsync(_currentCalendarDate.Year, _currentCalendarDate.Month);
                CalendarHelper.PopulateCalendarGrid(CalendarGrid, days, _currentCalendarDate);
                MonthYearLabel.Text = _currentCalendarDate.ToString("MMMM yyyy");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading calendar: {ex.Message}");
            }
        }

        private async void OnPreviousMonthClicked(object sender, EventArgs e)
        {
            _currentCalendarDate = _currentCalendarDate.AddMonths(-1);
            await LoadCalendarAsync();
        }

        private async void OnNextMonthClicked(object sender, EventArgs e)
        {
            _currentCalendarDate = _currentCalendarDate.AddMonths(1);
            await LoadCalendarAsync();
        }

        #endregion
    }
}
