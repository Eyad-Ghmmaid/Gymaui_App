using Gymaui_App.Services;
using Gymaui_App.Views;

namespace Gymaui_App
{
    public partial class AppShellContainer : ContentPage
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly INavigationService _navigationService;
        private int _currentTabIndex = -1;

        // Tab content cache to avoid recreating pages on every tab switch
        private readonly View?[] _tabCache = new View?[5];

        public AppShellContainer(IServiceProvider serviceProvider, INavigationService navigationService)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            System.Diagnostics.Debug.WriteLine("AppShellContainer initialized, loading first tab");

            // Subscribe to navigation events
            _navigationService.OnNavigateToTab += HandleNavigationRequest;

            // Load first tab (Home/StartPage) immediately
            LoadTab(0);
        }

        private void HandleNavigationRequest(int tabIndex)
        {
            System.Diagnostics.Debug.WriteLine($"Navigation request to tab {tabIndex}");
            // Force reload for navigation requests (e.g. starting a workout)
            _tabCache[tabIndex] = null;
            LoadTab(tabIndex);
        }

        private void OnHomeTabClicked(object sender, EventArgs e) => LoadTab(0);
        private void OnWorkoutTabClicked(object sender, EventArgs e) => LoadTab(1);
        private void OnPlansTabClicked(object sender, EventArgs e) => LoadTab(2);
        private void OnStatsTabClicked(object sender, EventArgs e) => LoadTab(3);
        private void OnExercisesTabClicked(object sender, EventArgs e) => LoadTab(4);

        private void LoadTab(int tabIndex)
        {
            if (tabIndex == _currentTabIndex && ContentArea.Content != null)
                return;

            _currentTabIndex = tabIndex;
            UpdateTabButtons();

            try
            {
                // Check cache first
                var cachedContent = _tabCache[tabIndex];
                if (cachedContent != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Loading tab {tabIndex} from cache");
                    ContentArea.Content = cachedContent;
                    return;
                }

                View? pageContent = null;

                switch (tabIndex)
                {
                    case 0:
                        pageContent = CreateStartPageContent();
                        break;
                    case 1:
                        pageContent = CreateActiveWorkoutPageContent();
                        break;
                    case 2:
                        pageContent = CreatePlansPageContent();
                        break;
                    case 3:
                        pageContent = CreateStatisticsPageContent();
                        break;
                    case 4:
                        pageContent = CreateExerciseListPageContent();
                        break;
                }

                if (pageContent != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Loading tab {tabIndex}");
                    _tabCache[tabIndex] = pageContent;
                    ContentArea.Content = pageContent;
                }
                else
                {
                    ShowErrorPage(tabIndex, "Could not create page content");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading tab {tabIndex}: {ex}");
                ShowErrorPage(tabIndex, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Invalidates the cache for a specific tab, forcing it to be recreated on next visit.
        /// </summary>
        public void InvalidateTab(int tabIndex)
        {
            if (tabIndex >= 0 && tabIndex < _tabCache.Length)
                _tabCache[tabIndex] = null;
        }

        private View CreateStartPageContent()
        {
            var db = _serviceProvider.GetService(typeof(DatabaseService)) as DatabaseService
                ?? throw new InvalidOperationException("DatabaseService not registered");
            var calendarService = _serviceProvider.GetService(typeof(CalendarService)) as CalendarService
                ?? throw new InvalidOperationException("CalendarService not registered");

            var page = new StartPage(db, calendarService);
            return page.Content ?? new Label { Text = "Failed to load StartPage content" };
        }

        private View CreateActiveWorkoutPageContent()
        {
            var db = _serviceProvider.GetService(typeof(DatabaseService)) as DatabaseService
                ?? throw new InvalidOperationException("DatabaseService not registered");
            var calendarService = _serviceProvider.GetService(typeof(CalendarService)) as CalendarService
                ?? throw new InvalidOperationException("CalendarService not registered");

            var page = new ActiveWorkoutPage(db, calendarService);
            return page.Content ?? new Label { Text = "Failed to load ActiveWorkout content" };
        }

        private View CreatePlansPageContent()
        {
            var db = _serviceProvider.GetService(typeof(DatabaseService)) as DatabaseService
                ?? throw new InvalidOperationException("DatabaseService not registered");

            var page = new PlansPage(db);
            return page.Content ?? new Label { Text = "Failed to load PlansPage content" };
        }

        private View CreateStatisticsPageContent()
        {
            var db = _serviceProvider.GetService(typeof(DatabaseService)) as DatabaseService
                ?? throw new InvalidOperationException("DatabaseService not registered");

            var page = new StatisticsPage(db);
            return page.Content ?? new Label { Text = "Failed to load StatisticsPage content" };
        }

        private View CreateExerciseListPageContent()
        {
            var vm = _serviceProvider.GetService(typeof(ViewModels.ExerciseListViewModel)) as ViewModels.ExerciseListViewModel
                ?? throw new InvalidOperationException("ExerciseListViewModel not registered");

            var page = new ExerciseListPage(vm);
            return page.Content ?? new Label { Text = "Failed to load ExerciseList content" };
        }

        private void ShowErrorPage(int tabIndex, string errorMessage)
        {
            ContentArea.Content = new VerticalStackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Spacing = 16,
                Padding = 20,
                Children =
                {
                    new Label
                    {
                        Text = $"Tab {tabIndex} Error",
                        TextColor = Color.FromArgb("#E8FF47"),
                        FontSize = 20,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalTextAlignment = TextAlignment.Center
                    },
                    new Label
                    {
                        Text = errorMessage,
                        TextColor = Color.FromArgb("#FFFFFF"),
                        FontSize = 14,
                        HorizontalTextAlignment = TextAlignment.Center
                    }
                }
            };
        }

        private void UpdateTabButtons()
        {
            var buttons = new[] { HomeTabBtn, WorkoutTabBtn, PlansTabBtn, StatsTabBtn, ExercisesTabBtn };

            foreach (var btn in buttons)
            {
                btn.TextColor = Color.FromArgb("#8A8A8A");
                btn.FontAttributes = FontAttributes.None;
            }

            buttons[_currentTabIndex].TextColor = Color.FromArgb("#E8FF47");
            buttons[_currentTabIndex].FontAttributes = FontAttributes.Bold;
        }

        // Public method for external navigation
        public void NavigateToTab(int tabIndex)
        {
            if (tabIndex >= 0 && tabIndex <= 4)
            {
                LoadTab(tabIndex);
            }
        }
    }
}
