using Gymaui_App.Views;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;

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
            Routing.RegisterRoute("createplan", typeof(CreatePlanPage));
        }

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
