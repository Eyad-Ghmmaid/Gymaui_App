# Calendar System - Testing & Integration Guide

## Quick Integration Test

### Test 1: Navigate to Calendar

1. Run the app
2. Look at the bottom tab bar
3. Verify you see ?? Calendar tab
4. Tap the Calendar tab
5. Expected: Calendar page loads with current month

### Test 2: Calendar Display

1. On Calendar page, verify:
   - Month and year are displayed (e.g., "December 2024")
   - Previous (?) and Next (?) buttons work
   - Day numbers are visible (1-31 or fewer)
   - Legend shows 4 colors with meanings
   - Days are arranged in 7-column grid (Mon-Sun)

### Test 3: Month Navigation

1. Click Next (?) button
2. Verify month/year changes
3. Click Previous (?) button
4. Verify month/year changes back
5. Test going forward 12 months and back

### Test 4: Exercise Completion in Workout

1. Create a plan with exercises
2. Set today as a training day
3. Go to ?? Workout tab
4. Verify exercises appear with ? button
5. Click ? button on an exercise
6. Verify button changes appearance (color/text)
7. Refresh or navigate away and back
8. Verify button state is saved

### Test 5: Calendar Status Update

1. After marking exercises complete (Test 4)
2. Go to ?? Calendar tab
3. Find today's date
4. Verify:
   - Cell shows "1/3" or similar (completed/total)
   - Cell is GREEN if all exercises done
   - Cell is RED if some exercises not done
   - Cell is BLUE if today is a rest day
   - Cell is GRAY if future date

### Test 6: Rest Days

1. Create a plan where certain days are REST DAYS
2. Go to calendar
3. Find those days in current month
4. Verify cells are BLUE (not green/red)
5. Verify exercise count shows 0/0

### Test 7: Future Days

1. Look at calendar
2. Find a date that's in the future
3. Verify cell is GRAY
4. Verify exercise count shows correctly
5. Verify status icon is "?"

### Test 8: Missed Training

1. Create plan with training day as Monday
2. Wait until Monday has passed
3. Don't mark any exercises as complete
4. View calendar for that month
5. Find Monday
6. Verify cell is RED
7. Verify shows "0/X" where X is exercise count

## Unit Testing Code

### Test: Calendar Service Status Determination

```csharp
[Test]
public async Task GetDayInfoAsync_FutureDay_ReturnsFuture()
{
    var service = new CalendarService(_databaseService);
    var futureDate = DateTime.Now.AddDays(10);
    
    var dayInfo = await service.GetDayInfoAsync(1, futureDate);
    
    Assert.AreEqual(DayStatus.Future, dayInfo.Status);
}

[Test]
public async Task GetDayInfoAsync_RestDay_ReturnsRestDay()
{
    var service = new CalendarService(_databaseService);
    var today = DateTime.Now;
    
    var dayInfo = await service.GetDayInfoAsync(1, today);
    
    if (dayInfo.TotalExercises == 0)
        Assert.AreEqual(DayStatus.RestDay, dayInfo.Status);
}

[Test]
public async Task MarkExerciseCompleted_ToggleTwice_ReturnsFalse()
{
    var service = new CalendarService(_databaseService);
    
    await service.MarkExerciseCompletedAsync(1, DateTime.Now, true);
    var first = await service.IsExerciseCompletedAsync(1, DateTime.Now);
    
    await service.MarkExerciseCompletedAsync(1, DateTime.Now, false);
    var second = await service.IsExerciseCompletedAsync(1, DateTime.Now);
    
    Assert.IsTrue(first);
    Assert.IsFalse(second);
}
```

## Database Verification Queries

### Check DailyProgress Table

```csharp
public async Task VerifyDailyProgressData()
{
    var db = new DatabaseService();
    await db.InitializeAsync();
    
    // Get all progress records
    var progressList = await db.GetDailyProgressAsync(planDayId: 1, DateTime.Now);
    
    if (progressList != null)
    {
        Console.WriteLine($"? Found progress for today");
        Console.WriteLine($"  Completed: {progressList.CompletedExerciseCount}");
        Console.WriteLine($"  Total: {progressList.TotalExerciseCount}");
        Console.WriteLine($"  IsComplete: {progressList.IsComplete}");
    }
}
```

### Check ExerciseCompletion Table

```csharp
public async Task VerifyExerciseCompletionData()
{
    var db = new DatabaseService();
    await db.InitializeAsync();
    
    var completions = await db.GetExerciseCompletionsForDateAsync(DateTime.Now);
    
    Console.WriteLine($"Found {completions.Count} completions for today:");
    foreach (var c in completions)
    {
        Console.WriteLine($"  - Exercise {c.PlanExerciseId}: {c.IsCompleted}");
    }
}
```

## Debugging Checklist

### If Calendar doesn't show up:
- [ ] CalendarPage.xaml exists and compiles
- [ ] CalendarPage.xaml.cs exists and compiles
- [ ] CalendarPage registered in MauiProgram.cs
- [ ] CalendarService registered in MauiProgram.cs
- [ ] AppShell.xaml has CalendarPage route
- [ ] TabBar includes CalendarTab

### If calendar shows but no data:
- [ ] Database initialized (InitializeAsync called)
- [ ] DailyProgress table created in database
- [ ] ExerciseCompletion table created in database
- [ ] Active plan exists
- [ ] Plan has days configured
- [ ] Days have exercises assigned

### If exercise checkmark button doesn't work:
- [ ] ActiveWorkoutPage.xaml has Button with Click handler
- [ ] OnExerciseCompletionToggled method exists
- [ ] CalendarService.MarkExerciseCompletedAsync called
- [ ] No exceptions in debug output
- [ ] PlanDay loaded correctly before button click

### If colors don't update:
- [ ] GetStatusColor method returns correct Color
- [ ] DayStatus enum value correct
- [ ] Color hex values correct (#00AA00, #CC0000, etc.)
- [ ] CalendarPage reloads after data change

## Debug Logging

Add this to any page to enable detailed logging:

```csharp
private void DebugLog(string message)
{
    System.Diagnostics.Debug.WriteLine($"[CALENDAR DEBUG] {DateTime.Now:HH:mm:ss} - {message}");
}

// Usage in methods:
protected override async void OnAppearing()
{
    base.OnAppearing();
    DebugLog("CalendarPage OnAppearing called");
    
    try
    {
        await _databaseService.InitializeAsync();
        DebugLog("Database initialized");
        
        var calendar = await _calendarService.GetMonthCalendarAsync(DateTime.Now.Year, DateTime.Now.Month);
        DebugLog($"Calendar loaded with {calendar.Count} days");
        
        LoadCalendarAsync();
        DebugLog("Calendar loaded in UI");
    }
    catch (Exception ex)
    {
        DebugLog($"ERROR: {ex.Message}");
    }
}
```

## Performance Benchmarks

### Expected Performance

- **Calendar Month Load**: < 500ms
- **Single Day Info Query**: < 100ms  
- **Mark Exercise Complete**: < 200ms
- **Exercise Completion Check**: < 50ms

### If Performance Issues:

1. **Slow Calendar Load**
   - Check: Number of database rows
   - Solution: Consider pagination or caching

2. **Slow Exercise Marking**
   - Check: Database indexes on Date column
   - Solution: Add database indexes if needed

3. **Slow Status Calculation**
   - Check: N+1 query problem
   - Solution: Batch load related data

## Common Issues & Solutions

### Issue: Calendar cells are all gray
**Cause**: All days detected as "future"
**Solution**: Check system date, verify DateTime.Now is correct

### Issue: Red cell shows 0/0 exercises
**Cause**: No exercises assigned to that plan day
**Solution**: Verify exercises added to the plan for that weekday

### Issue: Button doesn't toggle on click
**Cause**: CurrentPlanDay is null
**Solution**: Ensure plan day is loaded before marking exercises

### Issue: Exercise completion not persisting
**Cause**: Database transaction not committed
**Solution**: Verify await CalendarService.MarkExerciseCompletedAsync completes

### Issue: Wrong exercises shown for day
**Cause**: DayOfWeek conversion error
**Solution**: Verify PlanDay.DayOfWeek matches System.DayOfWeek (0=Monday)

## Stress Testing

### Test 1: 1000 Exercises
1. Create plan with many exercises per day
2. Mark all complete
3. Verify calendar updates correctly
4. Check for memory leaks

### Test 2: 12 Month Navigation
1. Click Next 12 times
2. Click Previous 12 times
3. Verify no memory leak or lag

### Test 3: Rapid Toggles
1. Rapidly click exercise checkmark 10+ times
2. Verify state stays consistent
3. Verify no database corruption

## Version Compatibility

- **Target Framework**: .NET 9.0
- **MAUI Version**: Uses latest in project
- **Minimum Supported**: .NET 9.0
- **Database**: SQLite (local)

## Rollback Plan

If issues occur, to rollback:

1. Remove CalendarPage.xaml and CalendarPage.xaml.cs files
2. Remove CalendarService.cs from Services
3. Remove Calendar tab from AppShell.xaml
4. Remove CalendarService registration from MauiProgram.cs
5. Keep database schema (backward compatible)
6. Rebuild and test

## Success Criteria

? All tests pass from "Testing Checklist"
? Colors display correctly
? Exercise completion toggles work
? Calendar month navigation works
? No build errors or warnings
? No runtime exceptions in debug output
? Performance meets benchmarks
? UI is responsive

---

**Test Date**: [Your Date]
**Tester**: [Your Name]
**Status**: ? Pass / ? Fail
