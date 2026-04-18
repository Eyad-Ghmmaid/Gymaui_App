using Gymaui_App.Models;
using Gymaui_App.Services;
using Gymaui_App.Utilities;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Gymaui_App.Views
{
    public partial class WorkoutHistoryPage : ContentPage
    {
        private readonly DatabaseService _db;
        private ObservableCollection<PlanDayDisplay> _displayItems = new();
        private PlanDayDisplay? _draggedItem;

        public WorkoutHistoryPage(DatabaseService db)
        {
            InitializeComponent();
            _db = db ?? throw new ArgumentNullException(nameof(db));

            HeaderEventHelper.SetupHeaderEvents(this);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                await _db.InitializeAsync();

                var activePlan = await _db.GetActivePlanAsync();
                if (activePlan == null)
                {
                    EmptyState.IsVisible = true;
                    DaysCollection.IsVisible = false;
                    return;
                }

                var days = await _db.GetDaysForPlanAsync(activePlan.Id);
                var allExercises = await _db.GetExercisesAsync();

                _displayItems = new ObservableCollection<PlanDayDisplay>();

                foreach (var day in days.OrderBy(d => d.Order))
                {
                    var planExercises = await _db.GetExercisesForDayAsync(day.Id);
                    var exerciseNames = new List<string>();
                    foreach (var pe in planExercises.OrderBy(p => p.Order))
                    {
                        var ex = allExercises.FirstOrDefault(e => e.Id == pe.ExerciseId);
                        if (ex != null)
                            exerciseNames.Add(ex.Name);
                    }

                    _displayItems.Add(new PlanDayDisplay(day, exerciseNames));
                }

                if (_displayItems.Count == 0)
                {
                    EmptyState.IsVisible = true;
                    DaysCollection.IsVisible = false;
                }
                else
                {
                    EmptyState.IsVisible = false;
                    DaysCollection.IsVisible = true;
                    DaysCollection.ItemsSource = _displayItems;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WorkoutHistoryPage Error: {ex.Message}");
            }
        }

        private async void OnDaySelected(object? sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count == 0)
                return;

            if (e.CurrentSelection[0] is PlanDayDisplay display && display.IsTrainingDay)
            {
                var activePlan = await _db.GetActivePlanAsync();
                if (activePlan != null)
                {
                    await Shell.Current.GoToAsync(
                        $"{nameof(DayEditorPage)}?planId={activePlan.Id}&planDayId={display.PlanDayId}");
                }
            }

            ((CollectionView)sender!).SelectedItem = null;
        }

        private void OnDragStarting(object? sender, DragStartingEventArgs e)
        {
            if (sender is not GestureRecognizer recognizer)
                return;

            var border = FindParent<Border>(recognizer.Parent as Element);
            if (border?.BindingContext is PlanDayDisplay item)
            {
                _draggedItem = item;
                e.Data.Properties["PlanDayDisplay"] = item;
                border.Opacity = 0.5;
            }
        }

        private void OnDragOver(object? sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        private async void OnDrop(object? sender, DropEventArgs e)
        {
            if (sender is GestureRecognizer recognizer
                && FindParent<Border>(recognizer.Parent as Element) is Border targetBorder
                && targetBorder.BindingContext is PlanDayDisplay targetItem
                && _draggedItem != null
                && _draggedItem != targetItem)
            {
                int oldIndex = _displayItems.IndexOf(_draggedItem);
                int newIndex = _displayItems.IndexOf(targetItem);

                if (oldIndex >= 0 && newIndex >= 0)
                {
                    _displayItems.Move(oldIndex, newIndex);
                    await ReIndexAndSaveAsync();
                }
            }

            RestoreDragVisuals();
            _draggedItem = null;
        }

        private async Task ReIndexAndSaveAsync()
        {
            for (int i = 0; i < _displayItems.Count; i++)
            {
                var display = _displayItems[i];
                var planDay = display.PlanDay;

                planDay.Order = i;
                planDay.DayOfWeek = i;

                // Re-index the name: preserve the descriptive suffix after the dash
                var currentName = planDay.Name ?? string.Empty;
                var dashIndex = currentName.IndexOf('\u2013'); // en-dash
                if (dashIndex < 0)
                    dashIndex = currentName.IndexOf('-'); // regular dash

                if (dashIndex >= 0)
                {
                    var suffix = currentName.Substring(dashIndex);
                    planDay.Name = $"Tag {i + 1} {suffix}";
                }
                else if (currentName.StartsWith("Tag ", StringComparison.OrdinalIgnoreCase)
                      || currentName.StartsWith("Ruhetag", StringComparison.OrdinalIgnoreCase))
                {
                    planDay.Name = planDay.IsTrainingDay
                        ? $"Tag {i + 1}"
                        : "Ruhetag";
                }
                else
                {
                    planDay.Name = $"Tag {i + 1} \u2013 {currentName}";
                }

                // Update display properties
                display.UpdateFrom(planDay);

                // Persist to DB
                await _db.UpdatePlanDayAsync(planDay);
            }
        }

        private void RestoreDragVisuals()
        {
            var source = DaysCollection.ItemsSource;
            DaysCollection.ItemsSource = null;
            DaysCollection.ItemsSource = source;
        }

        private static T? FindParent<T>(Element? element) where T : Element
        {
            while (element != null)
            {
                if (element is T match)
                    return match;
                element = element.Parent;
            }
            return null;
        }
    }

    public class PlanDayDisplay : INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        public PlanDay PlanDay { get; }
        public int PlanDayId => PlanDay.Id;
        public bool IsTrainingDay => PlanDay.IsTrainingDay;

        private string _dayNumber = string.Empty;
        public string DayNumber
        {
            get => _dayNumber;
            private set { _dayNumber = value; PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(DayNumber))); }
        }

        private string _dayName = string.Empty;
        public string DayName
        {
            get => _dayName;
            private set { _dayName = value; PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(DayName))); }
        }

        private string _statusText = string.Empty;
        public string StatusText
        {
            get => _statusText;
            private set { _statusText = value; PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(StatusText))); }
        }

        private string _exerciseCountText = string.Empty;
        public string ExerciseCountText
        {
            get => _exerciseCountText;
            private set { _exerciseCountText = value; PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(ExerciseCountText))); }
        }

        public bool HasExercises { get; }

        private Color _badgeColor = Colors.Gray;
        public Color BadgeColor
        {
            get => _badgeColor;
            private set { _badgeColor = value; PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(BadgeColor))); }
        }

        public PlanDayDisplay(PlanDay planDay, List<string> exerciseNames)
        {
            PlanDay = planDay;
            _dayNumber = $"{planDay.Order + 1}";
            _dayName = planDay.Name;

            if (planDay.IsTrainingDay)
            {
                _statusText = "Trainingstag";
                _badgeColor = Color.FromArgb("#E8FF47");
            }
            else
            {
                _statusText = "Ruhetag";
                _badgeColor = Color.FromArgb("#444444");
            }

            HasExercises = exerciseNames.Count > 0;
            _exerciseCountText = exerciseNames.Count == 0
                ? "Keine Uebungen"
                : exerciseNames.Count == 1
                    ? "1 Uebung"
                    : $"{exerciseNames.Count} Uebungen";
        }

        public void UpdateFrom(PlanDay planDay)
        {
            DayNumber = $"{planDay.Order + 1}";
            DayName = planDay.Name;
            StatusText = planDay.IsTrainingDay ? "Trainingstag" : "Ruhetag";
            BadgeColor = planDay.IsTrainingDay
                ? Color.FromArgb("#E8FF47")
                : Color.FromArgb("#444444");
        }
    }
}
