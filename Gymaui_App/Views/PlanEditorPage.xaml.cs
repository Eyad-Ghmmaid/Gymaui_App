using Gymaui_App.Models;
using Gymaui_App.Services;
using Gymaui_App.Utilities;
using System.Collections.ObjectModel;

namespace Gymaui_App.Views
{
    [QueryProperty(nameof(PlanId), "planId")]
    public partial class PlanEditorPage : ContentPage
    {
        public const string Route = nameof(PlanEditorPage);
        private readonly DatabaseService _db;
        private Plan? _plan;
        private ObservableCollection<PlanDay> _planDays = new();
        private PlanDay? _draggedDay;

        public int PlanId { get; set; }

        public PlanEditorPage(DatabaseService db)
        {
            InitializeComponent();
            _db = db ?? throw new ArgumentNullException(nameof(db));

            // Wire up custom header events using helper
            HeaderEventHelper.SetupHeaderEvents(this);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                await _db.InitializeAsync();

                if (PlanId == 0) return;

                _plan = await _db.GetPlanAsync(PlanId);
                if (_plan == null) return;

                PlanNameEntry.Text = _plan.Name;

                await LoadOrCreatePlanDays();

                TrainingDaysCollection.ItemsSource = _planDays;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnAppearing: {ex.Message}");
                await DisplayAlert("Fehler", ex.Message, "OK");
            }
        }

        private async Task LoadOrCreatePlanDays()
        {
            if (_plan == null) return;

            var days = await _db.GetDaysForPlanAsync(_plan.Id);

            _planDays.Clear();

            if (days.Count == 0)
            {
                for (int i = 0; i < 7; i++)
                {
                    var day = new PlanDay
                    {
                        PlanId = _plan.Id,
                        DayOfWeek = i,
                        Name = $"Tag {i + 1}",
                        IsTrainingDay = true,
                        Order = i
                    };
                    await _db.AddPlanDayAsync(day);
                    _planDays.Add(day);
                }
            }
            else
            {
                foreach (var day in days.OrderBy(d => d.Order))
                {
                    _planDays.Add(day);
                }
            }
        }

        private async void OnEditDayExercisesClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is int planDayId)
            {
                try
                {
                    await Shell.Current.GoToAsync($"{nameof(DayEditorPage)}?planId={PlanId}&planDayId={planDayId}");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Fehler", $"Navigation fehlgeschlagen: {ex.Message}", "OK");
                }
            }
        }

        private void ReIndexDays()
        {
            for (int i = 0; i < _planDays.Count; i++)
            {
                _planDays[i].Order = i;
                _planDays[i].DayOfWeek = i;

                // Re-index the name: preserve the descriptive suffix after the dash
                var currentName = _planDays[i].Name ?? string.Empty;
                var dashIndex = currentName.IndexOf('\u2013'); // en-dash
                if (dashIndex < 0)
                    dashIndex = currentName.IndexOf('-'); // regular dash

                if (dashIndex >= 0)
                {
                    var suffix = currentName.Substring(dashIndex);
                    _planDays[i].Name = $"Tag {i + 1} {suffix}";
                }
                else if (currentName.StartsWith("Tag ", StringComparison.OrdinalIgnoreCase)
                      || currentName.StartsWith("Ruhetag", StringComparison.OrdinalIgnoreCase))
                {
                    _planDays[i].Name = _planDays[i].IsTrainingDay
                        ? $"Tag {i + 1}"
                        : "Ruhetag";
                }
                else
                {
                    _planDays[i].Name = $"Tag {i + 1} \u2013 {currentName}";
                }
            }
        }

        private void OnDragStarting(object? sender, DragStartingEventArgs e)
        {
            if (sender is not GestureRecognizer recognizer)
                return;

            // The DragGestureRecognizer is on the handle Label inside the Border
            var element = recognizer.Parent as VisualElement;
            if (element == null)
                return;

            // Walk up the visual tree to find the Border item container
            var border = FindParent<Border>(element);

            if (border?.BindingContext is PlanDay day)
            {
                _draggedDay = day;
                e.Data.Properties["PlanDay"] = day;
                border.Opacity = 0.5;
            }
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

        private void OnDragOver(object? sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        private void OnDrop(object? sender, DropEventArgs e)
        {
            if (sender is GestureRecognizer recognizer
                && FindParent<Border>(recognizer.Parent as Element) is Border targetBorder
                && targetBorder.BindingContext is PlanDay targetDay
                && _draggedDay != null
                && _draggedDay != targetDay)
            {
                int oldIndex = _planDays.IndexOf(_draggedDay);
                int newIndex = _planDays.IndexOf(targetDay);

                if (oldIndex >= 0 && newIndex >= 0)
                {
                    _planDays.Move(oldIndex, newIndex);
                    ReIndexDays();
                }
            }

            RestoreDragVisuals();
            _draggedDay = null;
        }

        private void RestoreDragVisuals()
        {
            // Force visual refresh by reassigning ItemsSource to reset opacity
            var source = TrainingDaysCollection.ItemsSource;
            TrainingDaysCollection.ItemsSource = null;
            TrainingDaysCollection.ItemsSource = source;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            SaveButton.IsEnabled = false;
            try
            {
                if (_plan == null) return;

                if (!string.IsNullOrWhiteSpace(PlanNameEntry.Text))
                {
                    _plan.Name = PlanNameEntry.Text.Trim();
                    await _db.UpdatePlanAsync(_plan);
                }

                foreach (var day in _planDays)
                {
                    await _db.UpdatePlanDayAsync(day);
                }

                await DisplayAlert("Erfolg", "Plan gespeichert", "OK");

                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync("..", true);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving plan: {ex.Message}");
                await DisplayAlert("Fehler", $"Fehler beim Speichern: {ex.Message}", "OK");
            }
            finally
            {
                SaveButton.IsEnabled = true;
            }
        }
    }
}
