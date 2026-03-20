using System;
using System.Linq;
using Gymaui_App.Models;
using Gymaui_App.Services;
using Microsoft.Maui.Controls;

namespace Gymaui_App.Views
{
    [QueryProperty(nameof(PlanId), "planId")]
    public partial class PlanEditorPage : ContentPage
    {
        private readonly DatabaseService _db;

        public int PlanId { get; set; }

        private Plan? _plan;

        private Label PlanNameLabel;

        public PlanEditorPage(DatabaseService db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            InitializeComponent();

            PlanNameLabel = new Label { FontAttributes = FontAttributes.Bold, FontSize = 18, TextColor = Color.FromArgb("#FFFFFF") };

            // create grid with buttons
            var grid = new Grid { ColumnDefinitions = { new ColumnDefinition(), new ColumnDefinition() }, RowSpacing = 8, ColumnSpacing = 8 };

            Button CreateDayButton(int index)
            {
                var btn = new Button 
                { 
                    Text = $"Tag {index}", 
                    CommandParameter = index,
                    BackgroundColor = Color.FromArgb("#E8FF47"),
                    TextColor = Color.FromArgb("#000000"),
                    FontAttributes = FontAttributes.Bold,
                    CornerRadius = 8,
                    Padding = 12
                };
                btn.Clicked += OnDayClicked;
                return btn;
            }

            grid.Add(CreateDayButton(1));
            grid.Add(CreateDayButton(2), 1, 0);
            grid.Add(CreateDayButton(3), 0, 1);
            grid.Add(CreateDayButton(4), 1, 1);
            grid.Add(CreateDayButton(5), 0, 2);
            grid.Add(CreateDayButton(6), 1, 2);
            var btn7 = CreateDayButton(7);
            grid.Add(btn7, 0, 3);
            Grid.SetColumnSpan(btn7, 2);

            var stack = new StackLayout { Padding = 12, Spacing = 12, BackgroundColor = Color.FromArgb("#0D0D0D") };
            stack.Children.Add(PlanNameLabel);
            stack.Children.Add(grid);

            ContentGrid.Children.Add(stack);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _db.InitializeAsync();

            if (PlanId == 0) return;

            _plan = await _db.GetPlanAsync(PlanId);
            if (_plan == null) return;

            PlanNameLabel.Text = _plan.Name;

            // Load existing days for the plan
            var days = await _db.GetDaysForPlanAsync(PlanId);
            
            // For backward compatibility with old plans, create days if they don't exist
            if (days.Count == 0)
            {
                var dayNames = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
                for (int i = 0; i < 7; i++)
                {
                    var day = new PlanDay 
                    { 
                        PlanId = PlanId, 
                        DayOfWeek = i, 
                        Name = dayNames[i],
                        IsTrainingDay = i < 5, // Mon-Fri are training days by default
                        Order = i
                    };
                    await _db.AddPlanDayAsync(day);
                }
                days = await _db.GetDaysForPlanAsync(PlanId);
            }
        }

        private async void OnDayClicked(object sender, EventArgs e)
        {
            if (sender is Button b && b.CommandParameter is int index)
            {
                // navigate to DayEditor with planId and dayIndex
                await Shell.Current.GoToAsync($"{nameof(DayEditorPage)}?planId={PlanId}&dayIndex={index}");
            }
        }
    }
}

