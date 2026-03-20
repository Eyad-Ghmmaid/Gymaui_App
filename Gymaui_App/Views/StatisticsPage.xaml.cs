using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Gymaui_App.Models;
using Gymaui_App.Services;

namespace Gymaui_App.Views
{
    [QueryProperty(nameof(ExerciseId), "ExerciseId")]
    public partial class StatisticsPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private readonly WeightChartDrawable _drawable = new WeightChartDrawable();
        private Exercise? _exercise;
        private string? _exerciseId;
        private List<Exercise> _allExercises = new List<Exercise>();

        public string? ExerciseId
        {
            get => _exerciseId;
            set
            {
                _exerciseId = value;
            }
        }

        // parameterless ctor for XAML instantiation (fallback)
        public StatisticsPage() : this(new DatabaseService())
        {
        }

        // DI-friendly ctor
        public StatisticsPage(DatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            InitializeComponent();
            ChartView.Drawable = _drawable;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                await _databaseService.InitializeAsync();
                await LoadAllExercisesAsync();

                // if Exercise ID was passed via query parameter, select it
                if (!string.IsNullOrWhiteSpace(ExerciseId) && int.TryParse(ExerciseId, out var id))
                {
                    _exercise = await _databaseService.GetExerciseAsync(id);
                    if (_exercise != null)
                    {
                        var index = _allExercises.FindIndex(e => e.Id == _exercise.Id);
                        if (index >= 0)
                        {
                            ExercisePicker!.SelectedIndex = index;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnAppearing: {ex.Message}");
            }
        }

        private async Task LoadAllExercisesAsync()
        {
            try
            {
                _allExercises = await _databaseService.GetExercisesAsync();
                if (ExercisePicker != null)
                {
                    ExercisePicker.ItemsSource = _allExercises.Select(e => e.Name).ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading exercises: {ex.Message}");
            }
        }

        private async void OnExerciseSelected(object? sender, EventArgs e)
        {
            try
            {
                if (ExercisePicker != null && ExercisePicker.SelectedIndex >= 0 && ExercisePicker.SelectedIndex < _allExercises.Count)
                {
                    _exercise = _allExercises[ExercisePicker.SelectedIndex];
                    if (ExerciseNameLabel != null)
                    {
                        ExerciseNameLabel.Text = _exercise.Name;
                    }
                    await LoadAndRenderAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error selecting exercise: {ex.Message}");
            }
        }

        private async Task LoadAndRenderAsync()
        {
            try
            {
                if (_exercise == null)
                    return;

                await _databaseService.InitializeAsync();
                var logs = await _databaseService.GetLogsForExerciseAsync(_exercise.Id);

                if (logs.Count == 0)
                {
                    if (NoDataLabel != null) NoDataLabel.IsVisible = true;
                    if (MaxWeightLabel != null) MaxWeightLabel.Text = "-";
                    if (AvgWeightLabel != null) AvgWeightLabel.Text = "-";
                    if (EntryCountLabel != null) EntryCountLabel.Text = "0";
                    if (LastDateLabel != null) LastDateLabel.Text = "-";
                    _drawable.Data = new List<WeightPoint>();
                    ChartView.Invalidate();
                    return;
                }

                if (NoDataLabel != null) NoDataLabel.IsVisible = false;

                // order by time
                var ordered = logs.OrderBy(l => l.Timestamp).ToList();

                // Calculate statistics
                var maxWeight = ordered.Max(l => l.Weight);
                var avgWeight = ordered.Average(l => l.Weight);
                var lastDate = ordered.Last().Timestamp;

                if (MaxWeightLabel != null) MaxWeightLabel.Text = $"{maxWeight:F1} kg";
                if (AvgWeightLabel != null) AvgWeightLabel.Text = $"{avgWeight:F1} kg";
                if (EntryCountLabel != null) EntryCountLabel.Text = ordered.Count.ToString();
                if (LastDateLabel != null) LastDateLabel.Text = lastDate.ToString("dd.MM.yyyy");

                // Prepare chart data
                _drawable.Data = ordered.Select(l => new WeightPoint { Time = l.Timestamp, Weight = l.Weight }).ToList();

                // request redraw
                ChartView.Invalidate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LoadAndRenderAsync: {ex.Message}");
            }
        }

        private async void OnRefreshClicked(object? sender, EventArgs e)
        {
            await LoadAndRenderAsync();
        }
    }

    // simple DTO for drawing
    internal class WeightPoint
    {
        public DateTime Time { get; set; }
        public double Weight { get; set; }
    }

    // drawable that paints a simple line chart
    internal class WeightChartDrawable : IDrawable
    {
        public List<WeightPoint> Data { get; set; } = new List<WeightPoint>();

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.SaveState();

            // Dark background to match app theme
            canvas.FillColor = Color.FromArgb("#0D0D0D");
            canvas.FillRectangle(dirtyRect);

            var paddingLeft = 40f;
            var paddingRight = 16f;
            var paddingTop = 16f;
            var paddingBottom = 30f;

            var plotX = paddingLeft;
            var plotY = paddingTop;
            var plotWidth = Math.Max(10f, dirtyRect.Width - paddingLeft - paddingRight);
            var plotHeight = Math.Max(10f, dirtyRect.Height - paddingTop - paddingBottom);

            // draw axes
            canvas.StrokeColor = Color.FromArgb("#2A2A2A");
            canvas.StrokeSize = 1;
            // x axis
            canvas.DrawLine(plotX, plotY + plotHeight, plotX + plotWidth, plotY + plotHeight);
            // y axis
            canvas.DrawLine(plotX, plotY, plotX, plotY + plotHeight);

            if (Data == null || Data.Count == 0)
            {
                // no data text
                canvas.FontColor = Color.FromArgb("#8A8A8A");
                canvas.FontSize = 14;
                canvas.DrawString("Keine Daten", 0, 0, dirtyRect.Width, dirtyRect.Height, HorizontalAlignment.Center, VerticalAlignment.Center);
                canvas.RestoreState();
                return;
            }

            // compute ranges
            var times = Data.Select(d => d.Time).ToList();
            var weights = Data.Select(d => d.Weight).ToList();

            var minTime = times.Min();
            var maxTime = times.Max();
            var minWeight = Math.Min(0, (int)Math.Floor(weights.Min()));
            var maxWeight = Math.Max(1, (int)Math.Ceiling(weights.Max()));

            var timeRange = (maxTime - minTime).TotalSeconds;
            if (timeRange <= 0) timeRange = 1; // avoid div by zero

            var weightRange = maxWeight - minWeight;
            if (weightRange <= 0) weightRange = 1;

            // draw horizontal grid lines and weight labels (4 lines)
            canvas.FontColor = Color.FromArgb("#8A8A8A");
            canvas.FontSize = 12;
            int gridLines = 4;
            for (int i = 0; i <= gridLines; i++)
            {
                float y = plotY + (float)(plotHeight - (i / (double)gridLines) * plotHeight);
                canvas.StrokeColor = Color.FromArgb("#2A2A2A");
                canvas.StrokeSize = 1;
                canvas.DrawLine(plotX, y, plotX + plotWidth, y);

                var weightLabel = (minWeight + (i / (double)gridLines) * weightRange).ToString("0");
                canvas.FontColor = Color.FromArgb("#8A8A8A");
                canvas.DrawString(weightLabel, 4, y - 8, 32, 16, HorizontalAlignment.Left, VerticalAlignment.Center);
            }

            // compute points in pixel coordinates
            var pts = new List<PointF>();
            foreach (var d in Data)
            {
                var tSec = (d.Time - minTime).TotalSeconds;
                var xFrac = tSec / timeRange;
                var x = plotX + (float)(xFrac * plotWidth);

                var wFrac = (d.Weight - minWeight) / (double)weightRange;
                var y = plotY + (float)((1.0 - wFrac) * plotHeight);

                pts.Add(new PointF(x, y));
            }

            // draw line
            canvas.StrokeColor = Color.FromArgb("#E8FF47");
            canvas.StrokeSize = 2;
            for (int i = 0; i < pts.Count - 1; i++)
            {
                canvas.DrawLine(pts[i].X, pts[i].Y, pts[i + 1].X, pts[i + 1].Y);
            }

            // draw points
            canvas.FillColor = Color.FromArgb("#E8FF47");
            canvas.StrokeColor = Color.FromArgb("#E8FF47");
            foreach (var p in pts)
            {
                canvas.FillCircle(p.X, p.Y, 4);
                canvas.DrawCircle(p.X, p.Y, 4);
            }

            // draw time labels: first and last
            canvas.FontColor = Color.FromArgb("#8A8A8A");
            canvas.FontSize = 10;
            var firstLabel = minTime.ToString("yyyy-MM-dd");
            var lastLabel = maxTime.ToString("yyyy-MM-dd");
            canvas.DrawString(firstLabel, plotX, plotY + plotHeight + 4, 120, 20, HorizontalAlignment.Left, VerticalAlignment.Top);
            canvas.DrawString(lastLabel, plotX + plotWidth - 120, plotY + plotHeight + 4, 120, 20, HorizontalAlignment.Right, VerticalAlignment.Top);

            canvas.RestoreState();
        }
    }
}
