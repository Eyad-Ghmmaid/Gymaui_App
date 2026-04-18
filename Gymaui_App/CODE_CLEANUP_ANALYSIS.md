# Code Cleanup Analysis Report

## HIGH PRIORITY ISSUES

### 1. AddExercisePage.xaml.cs - CRITICAL
- **Issue**: Duplicate `using System.IO;` on lines 5-6
- **Issue**: Creating DatabaseService directly instead of DI (`new DatabaseService()`)
- **Issue**: Not implementing IDisposable for resources
- **Action**: Remove duplicate using, refactor to use DI

### 2. ActiveWorkoutPage.xaml.cs - CRITICAL
- **Issue**: Helper method `_database_service_get_exercise_guard` with ugly naming convention
- **Issue**: Creating DatabaseService directly
- **Issue**: Not properly disposing resources
- **Action**: Remove helper, use proper service injection

### 3. ExerciseViewModel.cs - MEDIUM
- **Issue**: Helper method `_database_service_delete_guard` with bad naming
- **Issue**: Using sync Command instead of AsyncRelayCommand
- **Issue**: No IsBusy guards on commands
- **Action**: Modernize commands, proper error handling

### 4. CreatePlanPage.xaml.cs - MEDIUM
- **Issue**: Multiple LoadExercisesForDays() calls causing reload loops
- **Issue**: Hardcoded colors instead of resource references
- **Action**: Optimize loops, use ResourceDictionary

### 5. NavigationService.cs - LOW
- **Issue**: Unused INavigationService implementation, only used for event
- **Action**: Verify if really needed

## MEDIUM PRIORITY ISSUES

### 6. DatabaseService.cs - DATABASE OPTIMIZATION
- **Issue**: Missing database indexes on frequently queried columns
- **Issue**: SetActivePlanAsync() loads all plans and updates them individually (N+1 problem)
- **Issue**: No caching mechanism for frequently accessed data
- **Action**: Add indexes, optimize queries, consider caching

### 7. Memory Leaks & Event Handlers
- **Issue**: Event handlers in OnAppearing() not unregistered in OnDisappearing()
- **Issue**: Static reference AppShell.PendingWorkoutSessionId
- **Issue**: NavigationService.OnNavigateToTab event never unsubscribed
- **Action**: Implement proper cleanup

### 8. XAML Styles & Resources
- **Issue**: Hardcoded colors throughout XAML files (need to check)
- **Issue**: Potential duplicate styles
- **Action**: Audit and consolidate

## LOW PRIORITY ISSUES

### 9. Unused Code
- **Issue**: DialogHelper.cs might not be used everywhere
- **Action**: Verify usage

### 10. Code Style
- **Issue**: Inconsistent exception handling (some catch all, some re-throw)
- **Issue**: Missing validation in some methods
- **Action**: Standardize error handling

## SUMMARY OF CHANGES NEEDED

| Category | Count | Files |
|----------|-------|-------|
| Dependency Injection Fixes | 2 | AddExercisePage, ActiveWorkoutPage |
| Command Modernization | 1 | ExerciseViewModel |
| Database Optimization | Multiple | DatabaseService |
| Event/Memory Cleanup | Multiple | Various Pages |
| Code Quality | 3 | Various |

---
**Generated**: Auto-analysis
**Status**: Ready for implementation
