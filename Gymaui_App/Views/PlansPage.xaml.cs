using Gymaui_App.Services;
using Gymaui_App.Models;
using Gymaui_App.Utilities;

namespace Gymaui_App.Views
{
    public partial class PlansPage : ContentPage
    {
        private readonly DatabaseService _db;

        public PlansPage(DatabaseService db)
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
                await LoadPlansAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading plans: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task LoadPlansAsync()
        {
            try
            {
                var plans = await _db.GetPlansAsync();
                PlansCollection.ItemsSource = plans.OrderByDescending(p => p.Created).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadPlansAsync: {ex.Message}");
            }
        }

        private async void OnCreatePlanClicked(object sender, EventArgs e)
        {
            // Navigate to the multi-step plan creation flow
            await Shell.Current.GoToAsync("createplan");
        }

        private async void OnEditPlanSwipe(object? sender, EventArgs e)
        {
            if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is Plan plan)
            {
                await Shell.Current.GoToAsync($"{nameof(PlanEditorPage)}?planId={plan.Id}");
            }
        }

        private async void OnSetActiveSwipe(object? sender, EventArgs e)
        {
            if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is Plan plan)
            {
                await _db.SetActivePlanAsync(plan.Id);
                await LoadPlansAsync();
            }
        }

        private async void OnDeletePlanSwipe(object? sender, EventArgs e)
        {
            if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is Plan plan)
            {
                var confirm = await DialogHelper.DisplayAlert("Löschen", "Plan löschen?", "Ja", "Nein");
                if (!confirm) return;

                await _db.DeletePlanAndChildrenAsync(plan.Id);
                await LoadPlansAsync();
            }
        }
    }
}
