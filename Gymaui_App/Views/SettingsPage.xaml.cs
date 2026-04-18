using Gymaui_App.Services;
using Gymaui_App.Services;

namespace Gymaui_App.Views
{
    public partial class SettingsPage : ContentPage
    {
        private readonly DatabaseService _db;
        private readonly ThemeService _themeService;
        private Button[] _timerButtons = Array.Empty<Button>();

        public SettingsPage(DatabaseService db, ThemeService themeService)
        {
            InitializeComponent();
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
            _timerButtons = new[] { Timer30Btn, Timer60Btn, Timer90Btn, Timer120Btn };
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadInfoAsync();
            LoadRestTimerPreference();
            LoadThemePreference();
        }

        private void LoadThemePreference()
        {
            ThemeSwitch.IsToggled = _themeService.IsDarkMode;
            ThemeModeLabel.Text = _themeService.IsDarkMode ? "Dunkelmodus" : "Hellmodus";
        }

        private void OnThemeToggled(object sender, ToggledEventArgs e)
        {
            var newTheme = e.Value ? AppThemeMode.Dark : AppThemeMode.Light;
            _themeService.SetTheme(newTheme);
            ThemeModeLabel.Text = e.Value ? "Dunkelmodus" : "Hellmodus";

            // Refresh the current page to show updated colors
            RefreshPageColors();
        }

        private void RefreshPageColors()
        {
            // Force re-read of resource colors for this page
            if (Application.Current?.Resources.TryGetValue("BackgroundDark", out var bgColor) == true)
                BackgroundColor = (Color)bgColor;

            // Update timer buttons based on current theme
            LoadRestTimerPreference();
        }

        private void LoadRestTimerPreference()
        {
            var savedTimer = Preferences.Get("default_rest_timer", 60);
            HighlightTimerButton(savedTimer);
        }

        private void HighlightTimerButton(int seconds)
        {
            // Get theme-aware colors
            Color unselectedBg;
            Color unselectedText;
            Color selectedBg;
            Color selectedText;

            if (Application.Current?.Resources.TryGetValue("Surface2", out var s2) == true)
                unselectedBg = (Color)s2;
            else
                unselectedBg = Color.FromArgb("#2A2A2A");

            if (Application.Current?.Resources.TryGetValue("TextPrimary", out var tp) == true)
                unselectedText = (Color)tp;
            else
                unselectedText = Color.FromArgb("#FFFFFF");

            if (Application.Current?.Resources.TryGetValue("PrimaryAccent", out var pa) == true)
                selectedBg = (Color)pa;
            else
                selectedBg = Color.FromArgb("#E8FF47");

            selectedText = Color.FromArgb("#000000");

            foreach (var btn in _timerButtons)
            {
                btn.BackgroundColor = unselectedBg;
                btn.TextColor = unselectedText;
                btn.FontAttributes = FontAttributes.None;
            }

            var index = seconds switch
            {
                30 => 0,
                60 => 1,
                90 => 2,
                120 => 3,
                _ => 1
            };

            _timerButtons[index].BackgroundColor = selectedBg;
            _timerButtons[index].TextColor = selectedText;
            _timerButtons[index].FontAttributes = FontAttributes.Bold;
        }

        private void OnRestTimerChanged(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is string param && int.TryParse(param, out var seconds))
            {
                Preferences.Set("default_rest_timer", seconds);
                HighlightTimerButton(seconds);
            }
        }

        private async Task LoadInfoAsync()
        {
            try
            {
                await _db.InitializeAsync();

                var exercises = await _db.GetExercisesAsync();
                var plans = await _db.GetPlansAsync();
                var sessions = await _db.GetWorkoutSessionsAsync();

                DbInfoLabel.Text = $"Uebungen: {exercises.Count} | Plaene: {plans.Count} | Workouts: {sessions.Count}";
            }
            catch (Exception ex)
            {
                DbInfoLabel.Text = $"Fehler beim Laden: {ex.Message}";
            }
        }

        private async void OnResetAllClicked(object sender, EventArgs e)
        {
            bool firstConfirm = await DisplayAlert(
                "Alles zuruecksetzen?",
                "Moechtest du wirklich ALLE Daten loeschen?\n\n- Alle Trainingsplaene\n- Alle Workout-Sessions\n- Alle Statistiken\n- Alle Uebungseintraege\n\nDie Standard-Uebungen werden neu geladen.",
                "Ja, zuruecksetzen",
                "Abbrechen");

            if (!firstConfirm)
                return;

            bool secondConfirm = await DisplayAlert(
                "Bist du sicher?",
                "Diese Aktion kann NICHT rueckgaengig gemacht werden!",
                "Endgueltig loeschen",
                "Abbrechen");

            if (!secondConfirm)
                return;

            try
            {
                ResetAllButton.IsEnabled = false;
                ResetAllButton.Text = "Wird zurueckgesetzt...";

                await _db.ResetAllDataAsync();

                await DisplayAlert(
                    "Fertig",
                    "Alle Daten wurden zurueckgesetzt. Die Standard-Uebungen wurden neu geladen.",
                    "OK");

                await LoadInfoAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Fehler", $"Zuruecksetzen fehlgeschlagen: {ex.Message}", "OK");
            }
            finally
            {
                ResetAllButton.IsEnabled = true;
                ResetAllButton.Text = "Alles zuruecksetzen";
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
