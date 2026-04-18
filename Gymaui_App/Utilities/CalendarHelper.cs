using Gymaui_App.Services;
using Microsoft.Maui.Controls.Shapes;

namespace Gymaui_App.Utilities
{
    /// <summary>
    /// Shared helper for building calendar UI cells, avoiding code duplication
    /// between StartPage and CalendarPage.
    /// </summary>
    public static class CalendarHelper
    {
        public static Border CreateDayCell(CalendarDayInfo day)
        {
            var statusColor = GetStatusColor(day.Status);
            var statusIcon = GetStatusIcon(day.Status);

            var dayLabel = new Label
            {
                Text = day.Date.Day.ToString(),
                TextColor = Colors.White,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center
            };

            var exercisesLabel = new Label
            {
                Text = $"{day.CompletedExercises}/{day.TotalExercises}",
                TextColor = Color.FromArgb("#E8FF47"),
                FontSize = 11,
                HorizontalTextAlignment = TextAlignment.Center
            };

            var iconLabel = new Label
            {
                Text = statusIcon,
                TextColor = Colors.White,
                FontSize = 12,
                HorizontalTextAlignment = TextAlignment.Center
            };

            var stackLayout = new VerticalStackLayout
            {
                Spacing = 2,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Children = { dayLabel, exercisesLabel, iconLabel }
            };

            var grid = new Grid
            {
                Padding = 8,
                HeightRequest = 60,
                Children = { stackLayout }
            };

            var border = new Border
            {
                Padding = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                Stroke = Color.FromArgb("#2A2A2A"),
                StrokeThickness = 1,
                Background = new SolidColorBrush(statusColor),
                Content = grid
            };

            return border;
        }

        public static void PopulateCalendarGrid(Grid calendarGrid, List<CalendarDayInfo> days, DateTime currentDate)
        {
            calendarGrid.Clear();

            var firstDay = new DateTime(currentDate.Year, currentDate.Month, 1);
            var dayOfWeek = (int)firstDay.DayOfWeek;
            var startDayOfWeek = (dayOfWeek + 6) % 7; // Monday=0

            int currentRow = 0;
            int currentCol = startDayOfWeek;

            foreach (var day in days)
            {
                var dayCell = CreateDayCell(day);
                calendarGrid.Add(dayCell, currentCol, currentRow);

                currentCol++;
                if (currentCol > 6)
                {
                    currentCol = 0;
                    currentRow++;
                }
            }
        }

        public static Color GetStatusColor(DayStatus status)
        {
            return status switch
            {
                DayStatus.CompletedTraining => Color.FromArgb("#00AA00"),
                DayStatus.MissedTraining => Color.FromArgb("#CC0000"),
                DayStatus.RestDay => Color.FromArgb("#0066FF"),
                DayStatus.Future => Color.FromArgb("#444444"),
                _ => Color.FromArgb("#1A1A1A")
            };
        }

        public static string GetStatusIcon(DayStatus status)
        {
            return status switch
            {
                DayStatus.CompletedTraining => "✔",
                DayStatus.MissedTraining => "✖",
                DayStatus.RestDay => "·",
                DayStatus.Future => "·",
                _ => ""
            };
        }
    }
}
