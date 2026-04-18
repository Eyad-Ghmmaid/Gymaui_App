using Gymaui_App.Services;
using Gymaui_App.Utilities;

namespace Gymaui_App.Views
{
    public partial class CalendarPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private readonly CalendarService _calendarService;
        private DateTime _currentDate;

        public CalendarPage(DatabaseService databaseService, CalendarService calendarService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _calendarService = calendarService ?? throw new ArgumentNullException(nameof(calendarService));
            _currentDate = DateTime.Now;
            InitializeComponent();

            // Wire up custom header events using helper
            HeaderEventHelper.SetupHeaderEvents(this);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                await _databaseService.InitializeAsync();
                await LoadCalendarAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in CalendarPage OnAppearing: {ex.Message}");
                await DisplayAlert("Fehler", $"Kalender konnte nicht geladen werden: {ex.Message}", "OK");
            }
        }

        private async Task LoadCalendarAsync()
        {
            try
            {
                var days = await _calendarService.GetMonthCalendarAsync(_currentDate.Year, _currentDate.Month);
                CalendarHelper.PopulateCalendarGrid(CalendarGrid, days, _currentDate);
                MonthYearLabel.Text = _currentDate.ToString("MMMM yyyy");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading calendar: {ex.Message}");
                throw;
            }
        }

        private async void OnPreviousMonthClicked(object sender, EventArgs e)
        {
            _currentDate = _currentDate.AddMonths(-1);
            await LoadCalendarAsync();
        }

        private async void OnNextMonthClicked(object sender, EventArgs e)
        {
            _currentDate = _currentDate.AddMonths(1);
            await LoadCalendarAsync();
        }
    }
}
