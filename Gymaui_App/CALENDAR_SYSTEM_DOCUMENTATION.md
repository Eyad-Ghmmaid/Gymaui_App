# Training Calendar System - Implementation Guide

## Overview

The Training Calendar System provides a visual monthly calendar that tracks the progress of your workout plan without requiring dates to be tied to specific calendar dates. Instead, exercises are marked as completed when you finish them, and the calendar displays the status of each day based on the active weekly plan.

## Key Features

### 1. **Date-Independent Progress Tracking**
- Users work through their training days in any order, not necessarily on their corresponding calendar dates
- Each exercise has a checkbox button (?) that users can click to mark it as completed
- A training day is considered complete only when **all exercises** for that day are marked as done

### 2. **Calendar Status Color System**

The calendar uses four distinct colors to represent day statuses:

| Color | Status | Meaning |
|-------|--------|---------|
| ?? Green | Completed Training | All exercises for this day were completed |
| ?? Red | Missed Training | Training day has passed, but not all exercises are done |
| ?? Blue | Rest Day | No training scheduled for this day according to the plan |
| ? Gray | Future Day | Date hasn't arrived yet (not evaluated) |

### 3. **Automatic Day Classification**

The system automatically determines whether each calendar day is:
- **Training Day**: Based on the active weekly plan's configuration
- **Rest Day**: Days with no training scheduled in the plan
- **Past/Future**: Determined by comparing the date to today's date

## Architecture

### Models

#### `DailyProgress.cs`
Tracks the overall completion status for each day:
```csharp
public class DailyProgress
{
    public int Id { get; set; }
    public int PlanDayId { get; set; }           // Which day of the week
    public DateTime Date { get; set; }           // Which calendar date
    public int CompletedExerciseCount { get; set; }
    public int TotalExerciseCount { get; set; }
    public DateTime LastUpdated { get; set; }
    public bool IsComplete => CompletedExerciseCount == TotalExerciseCount;
}
```

#### `ExerciseCompletion.cs`
Tracks individual exercise completion:
```csharp
public class ExerciseCompletion
{
    public int Id { get; set; }
    public int PlanExerciseId { get; set; }      // Which exercise in the plan
    public DateTime Date { get; set; }           // Which date it was completed
    public bool IsCompleted { get; set; }        // Completion status
    public DateTime CompletedAt { get; set; }    // Timestamp
}
```

### Services

#### `CalendarService.cs`
Main service for calendar operations:

**Key Methods:**
- `GetMonthCalendarAsync(year, month)` - Get all day statuses for a month
- `GetDayInfoAsync(planId, date)` - Get detailed info for a specific day
- `MarkExerciseCompletedAsync(planExerciseId, date, isCompleted)` - Toggle exercise completion
- `IsExerciseCompletedAsync(planExerciseId, date)` - Check if exercise is completed
- `GetCompletionsForDateAsync(planDayId, date)` - Get all completions for a date

**Status Determination Logic:**
```
If date is in the future:
  ? Status = Future (gray)
Else if no PlanDay for this weekday OR not a training day:
  ? Status = RestDay (blue)
Else (it's a training day):
  If all exercises completed:
    ? Status = CompletedTraining (green)
  Else:
    ? Status = MissedTraining (red)
```

### Views

#### `CalendarPage.xaml / CalendarPage.xaml.cs`
Displays a monthly calendar grid with:
- Month navigation (previous/next buttons)
- 7-column grid (Mon-Sun)
- Each cell shows:
  - Day number
  - Completion ratio (X/Y exercises)
  - Status icon (?, ?, —, ?)
  - Color-coded background
- Legend showing color meanings

#### `ActiveWorkoutPage.xaml (Updated)`
Added exercise completion tracking:
- Green checkmark button (?) on each exercise
- Clicking toggles the exercise completion status
- Button color changes to indicate completion status
- Updates `DailyProgress` automatically

### Database Integration

**New Tables:**
- `DailyProgress` - Stores daily completion summaries
- `ExerciseCompletion` - Stores individual exercise completion records

**Updated DatabaseService Methods:**
```csharp
// DailyProgress CRUD
Task<DailyProgress?> GetDailyProgressAsync(int planDayId, DateTime date)
Task<List<DailyProgress>> GetDailyProgressInRangeAsync(...)
Task<int> AddDailyProgressAsync(DailyProgress progress)
Task<int> UpdateDailyProgressAsync(DailyProgress progress)

// ExerciseCompletion CRUD
Task<ExerciseCompletion?> GetExerciseCompletionAsync(int planExerciseId, DateTime date)
Task<List<ExerciseCompletion>> GetExerciseCompletionsForDateAsync(DateTime date)
Task<int> AddExerciseCompletionAsync(ExerciseCompletion completion)
Task<int> UpdateExerciseCompletionAsync(ExerciseCompletion completion)
```

## Usage Flow

### 1. **User Marks Exercise as Complete**
```
User clicks ? button on exercise in ActiveWorkoutPage
  ?
OnExerciseCompletionToggled() handler
  ?
CalendarService.MarkExerciseCompletedAsync() is called
  ?
ExerciseCompletion record is created/updated
  ?
DailyProgress is automatically updated
```

### 2. **User Views Calendar**
```
User navigates to Calendar tab
  ?
CalendarPage.OnAppearing()
  ?
CalendarService.GetMonthCalendarAsync(year, month)
  ?
For each day in the month:
  - Get PlanDay for that weekday
  - Get DailyProgress if date is past
  - Determine status
  ?
Calendar grid is populated with color-coded cells
```

### 3. **Status Updates in Real-Time**
```
Exercise marked complete today
  ?
DailyProgress updated with CompletedExerciseCount++
  ?
If all exercises done: Status becomes CompletedTraining (green)
  ?
Calendar updates on next refresh/reload
```

## Important Design Decisions

### 1. **Date Storage**
- All dates are stored as UTC midnight (date only, no time component)
- This ensures consistency across time zones

### 2. **Automatic Status Calculation**
- Status is calculated on-the-fly, not stored
- Only DailyProgress summary is cached
- Ensures always accurate, even if system time changes

### 3. **Weekly Plan Based**
- The calendar recognizes which days are training/rest based on the active plan's weekly structure
- If Monday is a training day in the plan, every Monday will be marked as training (unless completion changes it)

### 4. **PlanDay Integration**
- Each calendar day maps to a PlanDay (day of week: 0=Monday, 6=Sunday)
- Multiple calendar dates can share the same PlanDay (e.g., all Mondays)

## Navigation

The Calendar tab is integrated into the main app shell:
```
AppShell.xaml: Added CalendarPage as a tab (?? Calendar)
Tab Index: 4 (between Plans and Stats)
Route: "calendar"
```

## Future Enhancement Opportunities

1. **Weekly Statistics**
   - Show overall completion percentage for the week
   - Display streaks of completed training days

2. **Historical Analytics**
   - Filter past months to see completion trends
   - Export calendar data

3. **Notifications**
   - Remind users of missed training days
   - Congratulate on completed workouts

4. **Custom Completion Criteria**
   - Allow partial completion (e.g., 80% exercises = marked complete)
   - Weight exercises differently

5. **Sync Integration**
   - Cloud backup of calendar progress
   - Cross-device synchronization

## Technical Notes

- CalendarService is registered as Singleton in MauiProgram
- All database operations are async for UI responsiveness
- Color values use HEX format (#00AA00, #CC0000, #0066FF, #444444)
- Calendar uses a 7-column grid layout for optimal mobile display
