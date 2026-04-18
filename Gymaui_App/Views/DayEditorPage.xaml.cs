using Gymaui_App.Models;
using Gymaui_App.Services;
using Gymaui_App.Utilities;
using System.Collections.ObjectModel;

namespace Gymaui_App.Views
{
    [QueryProperty(nameof(PlanId), "planId")]
    [QueryProperty(nameof(PlanDayId), "planDayId")]
    public partial class DayEditorPage : ContentPage
    {
        public const string Route = nameof(DayEditorPage);
        private readonly DatabaseService _db;
        private PlanDay? _planDay;
        private List<Exercise> _allExercises = new();
        private ObservableCollection<PlanExercise> _dayExercises = new();

        public int PlanId { get; set; }
        public int PlanDayId { get; set; }

        public DayEditorPage(DatabaseService db)
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


                if (PlanDayId == 0) return;

                _planDay = await _db.GetPlanDayAsync(PlanDayId);
                if (_planDay == null) return;

                var header = this.FindByName<Controls.CustomHeader>("CustomHeader");
                if (header != null)
                    header.Title = $"{_planDay.Name} - Übungen";

                // Lade alle verfügbaren Übungen
                _allExercises = await _db.GetExercisesAsync();

                // Lade die Übungen für diesen Tag
                await LoadDayExercises();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnAppearing: {ex.Message}");
                await DisplayAlert("Fehler", ex.Message, "OK");
            }
        }

        private async Task LoadDayExercises()
        {
            if (_planDay == null) return;

            var planExercises = await _db.GetExercisesForDayAsync(_planDay.Id);

            _dayExercises.Clear();
            foreach (var pe in planExercises.OrderBy(x => x.Order))
            {
                pe.Exercise = _allExercises.FirstOrDefault(e => e.Id == pe.ExerciseId);
                _dayExercises.Add(pe);
            }

            DayExercisesCollection.ItemsSource = _dayExercises;
        }

        private async void OnSearchAndAddExerciseClicked(object sender, EventArgs e)
        {
            if (_planDay == null)
            {
                await DisplayAlert("Warnung", "Kein Tag ausgewählt", "OK");
                return;
            }

            try
            {
                var popup = new ExercisePickerPopup(_allExercises);
                await Navigation.PushModalAsync(popup, animated: true);
                var selectedExercise = await popup.Result;

                if (selectedExercise == null) return;

                var order = (_dayExercises?.Count ?? 0) + 1;

                var pe = new PlanExercise
                {
                    PlanDayId = _planDay.Id,
                    ExerciseId = selectedExercise.Id,
                    Order = order
                };

                await _db.AddPlanExerciseAsync(pe);
                await LoadDayExercises();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fehler", $"Fehler beim Hinzufügen: {ex.Message}", "OK");
            }
        }

        private async void OnRemoveExerciseSwipe(object? sender, EventArgs e)
        {
            if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is PlanExercise planExercise)
            {
                try
                {
                    var confirm = await DisplayAlert("Entfernen", $"'{planExercise.Exercise?.Name}' wirklich entfernen?", "Ja", "Nein");
                    if (!confirm) return;

                    await _db.DeletePlanExerciseAsync(planExercise);
                    await LoadDayExercises();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Fehler", $"Fehler beim Entfernen: {ex.Message}", "OK");
                }
            }
        }

        private async void OnStartTrainingClicked(object sender, EventArgs e)
        {
            if (_planDay == null || _dayExercises.Count == 0)
            {
                await DisplayAlert("Warnung", "Keine Übungen für diesen Tag", "OK");
                return;
            }

            try
            {
                var exercises = _dayExercises
                    .OrderBy(pe => pe.Order)
                    .Select(pe => pe.Exercise)
                    .OfType<Exercise>()
                    .ToList();

                if (exercises.Count == 0)
                {
                    await DisplayAlert("Fehler", "Übungen konnten nicht geladen werden", "OK");
                    return;
                }

                var session = new WorkoutSession
                {
                    Date = DateTime.UtcNow,
                    Name = _planDay.Name,
                    Exercises = exercises
                };

                await _db.AddWorkoutSessionAsync(session);
                AppShell.PendingWorkoutSessionId = session.Id;

                await AppShell.NavigateToTab("workout");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fehler", $"Fehler beim Starten des Trainings: {ex.Message}", "OK");
            }
        }
    }
}
