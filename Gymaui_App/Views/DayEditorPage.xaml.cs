using System;
using System.Collections.Generic;
using System.Linq;
using Gymaui_App.Models;
using Gymaui_App.Services;
using Microsoft.Maui.Controls;

namespace Gymaui_App.Views
{
    [QueryProperty(nameof(PlanId), "planId")]
    [QueryProperty(nameof(DayIndex), "dayIndex")]
    public partial class DayEditorPage : ContentPage
    {
        private readonly DatabaseService _db;

        public int PlanId { get; set; }
        public int DayIndex { get; set; }

        private PlanDay? _day;
        private List<Exercise>? _allExercises;
        private List<PlanExercise>? _dayExercises;

        private Label DayTitle;
        private Picker ExercisePicker;
        private CollectionView DayExercisesCollection;

        public DayEditorPage(DatabaseService db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            InitializeComponent();

            DayTitle = new Label { FontAttributes = FontAttributes.Bold, FontSize = 18, TextColor = Color.FromArgb("#FFFFFF") };
            ExercisePicker = new Picker { Title = "Übung auswählen", BackgroundColor = Color.FromArgb("#1A1A1A"), TextColor = Color.FromArgb("#FFFFFF") };
            var addBtn = new Button { Text = "+ Hinzufügen", BackgroundColor = Color.FromArgb("#E8FF47"), TextColor = Color.FromArgb("#000000"), FontAttributes = FontAttributes.Bold };
            addBtn.Clicked += OnAddExerciseClicked;

            DayExercisesCollection = new CollectionView { SelectionMode = SelectionMode.None };
            DayExercisesCollection.ItemTemplate = new DataTemplate(() =>
            {
                var border = new Border { BackgroundColor = Color.FromArgb("#1A1A1A"), Stroke = Color.FromArgb("#2A2A2A"), StrokeThickness = 1, Padding = 12 };
                var grid = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, Padding = 12 };
                var nameLabel = new Label { VerticalOptions = LayoutOptions.Center, TextColor = Color.FromArgb("#FFFFFF"), FontAttributes = FontAttributes.Bold };
                nameLabel.SetBinding(Label.TextProperty, "Exercise.Name");
                var removeBtn = new Button { Text = "Entfernen", BackgroundColor = Color.FromArgb("#8A8A8A"), TextColor = Color.FromArgb("#000000"), FontSize = 12 };
                removeBtn.SetBinding(Button.CommandParameterProperty, "Id");
                removeBtn.Clicked += OnRemoveExerciseClicked;
                grid.Add(nameLabel);
                grid.Add(removeBtn, 1, 0);
                border.Content = grid;
                return border;
            });

            var startBtn = new Button { Text = "Training starten", BackgroundColor = Color.FromArgb("#E8FF47"), TextColor = Color.FromArgb("#000000"), FontAttributes = FontAttributes.Bold };
            startBtn.Clicked += OnStartTrainingClicked;

            var stack = new StackLayout { Padding = 12, Spacing = 12, BackgroundColor = Color.FromArgb("#0D0D0D") };
            stack.Children.Add(DayTitle);
            stack.Children.Add(ExercisePicker);
            stack.Children.Add(addBtn);
            stack.Children.Add(DayExercisesCollection);
            stack.Children.Add(startBtn);

            ContentGrid.Children.Add(stack);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _db.InitializeAsync();

            if (PlanId == 0 || DayIndex == 0) return;

            _day = await _db.GetPlanDayByPlanAndIndexAsync(PlanId, DayIndex);
            if (_day == null)
            {
                // DayIndex is now Order
                var dayOfWeek = (DayIndex - 1) % 7;
                var dayNames = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
                _day = new PlanDay { PlanId = PlanId, DayOfWeek = dayOfWeek, Name = dayNames[dayOfWeek], Order = DayIndex - 1, IsTrainingDay = true };
                await _db.AddPlanDayAsync(_day);
                // re-fetch with id populated
                _day = await _db.GetPlanDayByPlanAndIndexAsync(PlanId, DayIndex);
            }

            DayTitle.Text = _day?.Name ?? $"Tag {DayIndex}";

            _allExercises = await _db.GetExercisesAsync();
            ExercisePicker.ItemsSource = _allExercises.Select(e => e.Name).ToList();

            await LoadDayExercisesAsync();
        }

        private async System.Threading.Tasks.Task LoadDayExercisesAsync()
        {
            if (_day == null) return;
            _dayExercises = await _db.GetExercisesForDayAsync(_day.Id);

            // populate Exercise property for display
            foreach (var pe in _dayExercises)
            {
                pe.Exercise = _allExercises?.FirstOrDefault(x => x.Id == pe.ExerciseId);
            }

            DayExercisesCollection.ItemsSource = _dayExercises.OrderBy(pe => pe.Order);
        }

        private async void OnAddExerciseClicked(object sender, EventArgs e)
        {
            if (_day == null) return;
            if (ExercisePicker.SelectedIndex < 0) return;

            var selected = _allExercises![ExercisePicker.SelectedIndex];
            var order = (_dayExercises?.Count ?? 0) + 1;
            var pe = new PlanExercise { PlanDayId = _day.Id, ExerciseId = selected.Id, Order = order };
            await _db.AddPlanExerciseAsync(pe);
            await LoadDayExercisesAsync();
        }

        private async void OnRemoveExerciseClicked(object sender, EventArgs e)
        {
            if (sender is Button b && b.CommandParameter is int id)
            {
                var existing = _dayExercises?.FirstOrDefault(x => x.Id == id);
                if (existing != null)
                {
                    await _db.DeletePlanExerciseAsync(existing);
                    await LoadDayExercisesAsync();
                }
            }
        }

        private async void OnStartTrainingClicked(object sender, EventArgs e)
        {
            if (_day == null) return;

            // gather exercises for this day
            var planExercises = await _db.GetExercisesForDayAsync(_day.Id);
            var exercises = new List<Exercise>();
            foreach (var pe in planExercises.OrderBy(pe => pe.Order))
            {
                var ex = await _db.GetExerciseAsync(pe.ExerciseId);
                if (ex != null)
                    exercises.Add(ex);
            }

            if (exercises.Count == 0)
            {
                await DisplayAlert("Keine Übungen", "Für diesen Tag sind keine Übungen hinterlegt.", "OK");
                return;
            }

            var session = new WorkoutSession { Date = DateTime.UtcNow, Exercises = exercises };
            await _db.AddWorkoutSessionAsync(session);

            // Store the session ID to pass to ActiveWorkoutPage
            AppShell.PendingWorkoutSessionId = session.Id;

            // Navigate to workout tab using AppShell helper
            await AppShell.NavigateToTab("workout");
        }
    }
}
