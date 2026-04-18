# Calendar System - Developer Quick Reference

## Quick Start for Using the Calendar System

### Initialize the System

```csharp
// In MauiProgram.cs (already done)
builder.Services.AddSingleton<CalendarService>();
builder.Services.AddSingleton<DatabaseService>();
```

### Marking Exercises as Complete

```csharp
// Inject CalendarService
private readonly CalendarService _calendarService;

// Mark an exercise as completed for today
await _calendarService.MarkExerciseCompletedAsync(
    planExerciseId: 5,
    date: DateTime.Now,
    isCompleted: true
);
```

### Getting Calendar Information

```csharp
// Get all days in a month with their statuses
var calendar = await _calendarService.GetMonthCalendarAsync(2024, 12);
foreach (var day in calendar)
{
    Console.WriteLine($"{day.Date}: {day.Status} ({day.CompletedExercises}/{day.TotalExercises})");
}

// Get specific day info
var dayInfo = await _calendarService.GetDayInfoAsync(planId: 1, date: DateTime.Now);
Console.WriteLine($"Status: {dayInfo.Status}");
Console.WriteLine($"Completed: {dayInfo.CompletedExercises}/{dayInfo.TotalExercises}");
```

### Checking Exercise Completion

```csharp
// Check if an exercise was completed on a date
bool isCompleted = await _calendarService.IsExerciseCompletedAsync(
    planExerciseId: 5,
    date: DateTime.Now
);

// Get all completions for a specific date
var completions = await _calendarService.GetCompletionsForDateAsync(
    planDayId: 1,
    date: DateTime.Now
);
```

## Status Enum Reference

```csharp
public enum DayStatus
{
    CompletedTraining,  // All exercises done (green)
    MissedTraining,     // Training day but not all done (red)
    RestDay,            // No training scheduled (blue)
    Future              // Date hasn't arrived yet (gray)
}
```

## Database Schema

### DailyProgress Table

| Column | Type | Purpose |
|--------|------|---------|
| Id | int | Primary key |
| PlanDayId | int | FK to PlanDay (day of week) |
| Date | DateTime | Calendar date (UTC midnight) |
| CompletedExerciseCount | int | How many exercises done |
| TotalExerciseCount | int | Total exercises for the day |
| LastUpdated | DateTime | When this was last updated |

### ExerciseCompletion Table

| Column | Type | Purpose |
|--------|------|---------|
| Id | int | Primary key |
| PlanExerciseId | int | FK to PlanExercise |
| Date | DateTime | Which date (UTC midnight) |
| IsCompleted | bool | Completion status |
| CompletedAt | DateTime | When it was marked |

## Querying Examples

### Find all completed training days this month

```csharp
var month = DateTime.Now;
var calendar = await _calendarService.GetMonthCalendarAsync(month.Year, month.Month);
var completedDays = calendar.Where(d => d.Status == DayStatus.CompletedTraining);

foreach (var day in completedDays)
{
    Console.WriteLine($"? {day.Date:MMM dd}");
}
```

### Find all missed training days

```csharp
var missedDays = calendar.Where(d => d.Status == DayStatus.MissedTraining);
var count = missedDays.Count();
Console.WriteLine($"Missed {count} training days this month");
```

### Calculate completion percentage

```csharp
var allTrainingDays = calendar.Where(d => d.Status != DayStatus.Future && 
                                          (d.Status == DayStatus.CompletedTraining ||
                                           d.Status == DayStatus.MissedTraining));
var completed = calendar.Count(d => d.Status == DayStatus.CompletedTraining);
var percentage = (completed * 100.0) / allTrainingDays.Count();
Console.WriteLine($"Completion: {percentage:F1}%");
```

## CalendarDayInfo Structure

```csharp
public class CalendarDayInfo
{
    public DateTime Date { get; set; }              // Calendar date
    public DayStatus Status { get; set; }           // Green/Red/Blue/Gray
    public int CompletedExercises { get; set; }     // X in X/Y
    public int TotalExercises { get; set; }         // Y in X/Y
    public PlanDay? PlanDay { get; set; }           // The plan day object
}
```

## Common Patterns

### Pattern 1: Update UI After Marking Exercise Complete

```csharp
private async void OnExerciseCompleteButtonClicked(int planExerciseId)
{
    await _calendarService.MarkExerciseCompletedAsync(
        planExerciseId, 
        DateTime.Now, 
        isCompleted: true
    );
    
    // Refresh calendar/UI
    var dayInfo = await _calendarService.GetDayInfoAsync(_currentPlanId, DateTime.Now);
    UpdateDayDisplay(dayInfo);
}
```

### Pattern 2: Show Daily Progress

```csharp
public async Task RefreshDailyStatus()
{
    var dayInfo = await _calendarService.GetDayInfoAsync(_activePlanId, DateTime.Now);
    
    ProgressLabel.Text = $"{dayInfo.CompletedExercises}/{dayInfo.TotalExercises} done";
    
    if (dayInfo.Status == DayStatus.CompletedTraining)
    {
        ProgressLabel.TextColor = Colors.Green;
        ProgressLabel.Text += " ?";
    }
    else if (dayInfo.Status == DayStatus.MissedTraining)
    {
        ProgressLabel.TextColor = Colors.Red;
        ProgressLabel.Text += " ?";
    }
}
```

### Pattern 3: Initialize Calendar on First Load

```csharp
protected override async void OnAppearing()
{
    base.OnAppearing();
    
    try
    {
        await _databaseService.InitializeAsync();
        
        var calendar = await _calendarService.GetMonthCalendarAsync(
            DateTime.Now.Year, 
            DateTime.Now.Month
        );
        
        DisplayCalendar(calendar);
    }
    catch (Exception ex)
    {
        await DisplayAlert("Error", $"Failed to load calendar: {ex.Message}", "OK");
    }
}
```

## Important: Date Handling

?? **Always use UTC dates:**

```csharp
// CORRECT: Use .Date to remove time component
var dateOnly = DateTime.Now.Date;  // ? Good

// INCORRECT: DateTime with time component
var withTime = DateTime.Now;       // ? Bad

// When querying:
var dayInfo = await _calendarService.GetDayInfoAsync(planId, dateOnly);
```

## Debugging Tips

### Check if exercise is being marked

```csharp
var isMarked = await _calendarService.IsExerciseCompletedAsync(5, DateTime.Now);
Debug.WriteLine($"Exercise marked: {isMarked}");
```

### Verify daily progress

```csharp
var databaseService = new DatabaseService();
await databaseService.InitializeAsync();
var progress = await databaseService.GetDailyProgressAsync(planDayId: 1, DateTime.Now);
if (progress != null)
{
    Debug.WriteLine($"Progress: {progress.CompletedExerciseCount}/{progress.TotalExerciseCount}");
}
```

### List all completions for a date

```csharp
var completions = await _databaseService.GetExerciseCompletionsForDateAsync(DateTime.Now);
Debug.WriteLine($"Found {completions.Count} completions today");
foreach (var c in completions)
{
    Debug.WriteLine($"  - Exercise {c.PlanExerciseId}: {c.IsCompleted}");
}
```

## Performance Considerations

- `GetMonthCalendarAsync()` loops through ~30 days - acceptable for UI
- `GetDayInfoAsync()` makes 2-3 database queries - OK for single day
- Cache month results if refreshing frequently
- Use `AsNoTracking()` in LINQ queries where possible

## Thread Safety

- All methods are async and thread-safe
- Use `await` consistently
- Don't mix sync and async code

## Error Handling

```csharp
try
{
    await _calendarService.MarkExerciseCompletedAsync(id, date, true);
}
catch (ArgumentNullException ex)
{
    // Database service not initialized
    await DisplayAlert("Error", "Service not ready", "OK");
}
catch (Exception ex)
{
    // Other errors
    Debug.WriteLine($"Error: {ex.Message}");
}
```
