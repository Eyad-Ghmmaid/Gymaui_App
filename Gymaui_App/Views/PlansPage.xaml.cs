using System;
using System.Linq;
using Gymaui_App.Models;
using Gymaui_App.Services;
using Gymaui_App.Utilities;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Gymaui_App.Views
{
    public partial class PlansPage : ContentPage
    {
        private readonly DatabaseService _db;

        public PlansPage(DatabaseService db)
        {
            InitializeComponent();
            _db = db ?? throw new ArgumentNullException(nameof(db));
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

        private async void OnEditPlanClicked(object sender, EventArgs e)
        {
            if (sender is Button b && b.CommandParameter is int id)
            {
                await Shell.Current.GoToAsync($"{nameof(PlanEditorPage)}?planId={id}");
            }
        }

        private async void OnSetActiveClicked(object sender, EventArgs e)
        {
            if (sender is Button b && b.CommandParameter is int id)
            {
                await _db.SetActivePlanAsync(id);
                await LoadPlansAsync();
            }
        }

        private async void OnDeletePlanClicked(object sender, EventArgs e)
        {
            if (sender is Button b && b.CommandParameter is int id)
            {
                var confirm = await DialogHelper.DisplayAlert("Löschen", "Plan löschen?", "Ja", "Nein");
                if (!confirm) return;

                await _db.DeletePlanAndChildrenAsync(id);
                await LoadPlansAsync();
            }
        }
    }
}
