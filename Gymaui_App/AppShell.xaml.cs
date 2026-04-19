using Gymaui_App.Views;

namespace Gymaui_App
{
    public partial class AppShell : Shell
    {
        // Static property to pass data between pages
        public static int? PendingWorkoutSessionId { get; set; }

        public AppShell()
        {
            InitializeComponent();

            // Disable Shell header - each page has its own custom header
            Shell.SetNavBarIsVisible(this, false);

            // Register routes for modal and stack navigation
            Routing.RegisterRoute(nameof(PlanEditorPage), typeof(PlanEditorPage));
            Routing.RegisterRoute(nameof(DayEditorPage), typeof(DayEditorPage));
            Routing.RegisterRoute(nameof(AddExercisePage), typeof(AddExercisePage));
            Routing.RegisterRoute(nameof(ExerciseSetsPage), typeof(ExerciseSetsPage));
            Routing.RegisterRoute(nameof(WorkoutDetailPage), typeof(WorkoutDetailPage));
            Routing.RegisterRoute("createplan", typeof(CreatePlanPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));

#if ANDROID
            this.Loaded += OnShellLoaded;
            this.Navigated += OnShellNavigated;
#endif
        }

#if ANDROID
        private void OnShellLoaded(object? sender, EventArgs e)
        {
            if (this.Handler?.PlatformView is global::Android.Views.ViewGroup viewGroup)
            {
                viewGroup.ViewTreeObserver!.GlobalLayout += (s, args) =>
                {
                    Gymaui_App.Platforms.Android.ShellIconTintRemover.DisableIconTintingFromActivity();
                };
            }
        }

        private async void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
        {
            // Immediate attempt
            Gymaui_App.Platforms.Android.ShellIconTintRemover.DisableIconTintingFromActivity();
            // Delayed attempts to catch views that appear after navigation (e.g. More menu)
            await Task.Delay(100);
            Gymaui_App.Platforms.Android.ShellIconTintRemover.DisableIconTintingFromActivity();
            await Task.Delay(300);
            Gymaui_App.Platforms.Android.ShellIconTintRemover.DisableIconTintingFromActivity();
        }
#endif

        // Helper method to navigate to specific tab
        public static async Task NavigateToTab(string route)
        {
            try
            {
                await Shell.Current.GoToAsync($"///{route}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
                // Fallback: directly set CurrentItem
                if (Shell.Current is AppShell shell && !string.IsNullOrEmpty(route))
                {
                    var items = shell.Items.OfType<ShellContent>();
                    var targetItem = items.FirstOrDefault(item =>
                        item.Route?.Equals(route, StringComparison.OrdinalIgnoreCase) == true);

                    if (targetItem != null)
                    {
                        shell.CurrentItem = targetItem;
                    }
                }
            }
        }
    }
}
