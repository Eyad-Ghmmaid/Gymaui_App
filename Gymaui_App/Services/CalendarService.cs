using Gymaui_App.Models;

namespace Gymaui_App.Services
{
    public enum DayStatus
    {
        CompletedTraining,
        MissedTraining,
        RestDay,
        Future
    }

    public class CalendarDayInfo
    {
        public DateTime Date { get; set; }
        public DayStatus Status { get; set; }
        public int CompletedExercises { get; set; }
        public int TotalExercises { get; set; }
        public PlanDay? PlanDay { get; set; }
    }

    public class CalendarService
    {
        private readonly DatabaseService _databaseService;

        public CalendarService(DatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        }

        /// <summary>
        /// Gets the status of all days in a given month for the active plan.
        /// Uses dynamic schedule shifting: missed training days stay in the plan
        /// and shift all subsequent days forward.
        /// </summary>
        public async Task<List<CalendarDayInfo>> GetMonthCalendarAsync(int year, int month)
        {
            await _databaseService.InitializeAsync();

            var activePlan = await _databaseService.GetActivePlanAsync();
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var today = DateTime.Now.Date;
            var result = new List<CalendarDayInfo>(daysInMonth);

            if (activePlan == null)
            {
                for (int day = 1; day <= daysInMonth; day++)
                {
                    var date = new DateTime(year, month, day);
                    result.Add(new CalendarDayInfo
                    {
                        Date = date,
                        Status = date > today ? DayStatus.Future : DayStatus.RestDay,
                        CompletedExercises = 0,
                        TotalExercises = 0
                    });
                }
                return result;
            }

            var planStartDate = activePlan.Created.Date;

            // Pre-load exercise counts per plan day
            var allDays = await _databaseService.GetDaysForPlanAsync(activePlan.Id);
            var exerciseCountByPlanDayId = new Dictionary<int, int>();
            foreach (var planDay in allDays.Where(d => d.IsTrainingDay))
            {
                var exercises = await _databaseService.GetExercisesForDayAsync(planDay.Id);
                exerciseCountByPlanDayId[planDay.Id] = exercises.Count;
            }

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(year, month, day);
                var dayInfo = new CalendarDayInfo { Date = date };

                if (date > today)
                {
                    dayInfo.Status = DayStatus.Future;
                    result.Add(dayInfo);
                    continue;
                }

                if (date < planStartDate)
                {
                    dayInfo.Status = DayStatus.RestDay;
                    result.Add(dayInfo);
                    continue;
                }

                // Use dynamic shifting to determine the plan day for this date
                var planDay = await _databaseService.GetDynamicPlanDayForDateAsync(activePlan, date);

                if (planDay == null || !planDay.IsTrainingDay)
                {
                    dayInfo.Status = DayStatus.RestDay;
                    dayInfo.PlanDay = planDay;
                    result.Add(dayInfo);
                    continue;
                }

                // Training day - check progress
                dayInfo.PlanDay = planDay;
                var exerciseCount = exerciseCountByPlanDayId.GetValueOrDefault(planDay.Id, 0);
                dayInfo.TotalExercises = exerciseCount;

                var progress = await _databaseService.GetDailyProgressAsync(planDay.Id, date);
                if (progress != null)
                {
                    dayInfo.CompletedExercises = progress.CompletedExerciseCount;
                    dayInfo.Status = (progress.CompletedExerciseCount >= exerciseCount && exerciseCount > 0)
                        ? DayStatus.CompletedTraining
                        : DayStatus.RestDay; // Not completed yet but not "missed" - dynamic shifting handles this
                }
                else
                {
                    dayInfo.CompletedExercises = 0;
                    // With dynamic shifting, an incomplete training day is not "missed",
                    // it's just pending (will be shown as the current day)
                    dayInfo.Status = (date < today) ? DayStatus.RestDay : DayStatus.Future;
                }

                result.Add(dayInfo);
            }

            return result;
        }

        /// <summary>
        /// Marks an exercise as completed for a given date
        /// </summary>
        public async Task MarkExerciseCompletedAsync(int planExerciseId, DateTime date, bool isCompleted)
        {
            await _databaseService.InitializeAsync();

            var dateOnly = date.Date;
            var completion = await _databaseService.GetExerciseCompletionAsync(planExerciseId, dateOnly);

            if (completion == null)
            {
                completion = new ExerciseCompletion
                {
                    PlanExerciseId = planExerciseId,
                    Date = dateOnly,
                    IsCompleted = isCompleted,
                    CompletedAt = DateTime.UtcNow
                };
                await _databaseService.AddExerciseCompletionAsync(completion);
            }
            else
            {
                completion.IsCompleted = isCompleted;
                completion.CompletedAt = DateTime.UtcNow;
                await _databaseService.UpdateExerciseCompletionAsync(completion);
            }

            // Update daily progress
            var planExercise = await _databaseService.GetPlanExerciseAsync(planExerciseId);
            if (planExercise != null)
            {
                await UpdateDailyProgressByPlanDayAsync(planExercise.PlanDayId, dateOnly);
            }
        }

        /// <summary>
        /// Gets whether an exercise is marked as completed for a date
        /// </summary>
        public async Task<bool> IsExerciseCompletedAsync(int planExerciseId, DateTime date)
        {
            var completion = await _databaseService.GetExerciseCompletionAsync(planExerciseId, date.Date);
            return completion?.IsCompleted ?? false;
        }

        /// <summary>
        /// Gets all completions for a specific date using a single batch query
        /// </summary>
        public async Task<Dictionary<int, bool>> GetCompletionsForDateAsync(int planDayId, DateTime date)
        {
            var exercises = await _databaseService.GetExercisesForDayAsync(planDayId);
            var completions = await _databaseService.GetExerciseCompletionsForDateAsync(date.Date);
            var completedIds = new HashSet<int>(completions.Select(c => c.PlanExerciseId));

            return exercises.ToDictionary(e => e.Id, e => completedIds.Contains(e.Id));
        }

        /// <summary>
        /// Calculates the current training streak (consecutive training days completed).
        /// Uses dynamic shifting - counts backwards from today, skipping rest days.
        /// </summary>
        public async Task<int> GetCurrentStreakAsync()
        {
            var activePlan = await _databaseService.GetActivePlanAsync();
            if (activePlan == null)
                return 0;

            var planStartDate = activePlan.Created.Date;
            var today = DateTime.Now.Date;
            int streak = 0;

            for (var date = today; date >= planStartDate; date = date.AddDays(-1))
            {
                var planDay = await _databaseService.GetDynamicPlanDayForDateAsync(activePlan, date);

                if (planDay == null || !planDay.IsTrainingDay)
                    continue; // skip rest days

                if (date > today)
                    continue;

                var progress = await _databaseService.GetDailyProgressAsync(planDay.Id, date);
                if (progress != null && progress.IsComplete)
                {
                    streak++;
                }
                else if (date < today)
                {
                    break; // streak broken
                }
            }

            return streak;
        }

        /// <summary>
        /// Gets the weekly completion summary for the current week (Mon-Sun).
        /// Uses dynamic schedule shifting.
        /// Returns a list of 7 booleans (Mon=0 to Sun=6), true if training was completed.
        /// Null means no training scheduled (rest day).
        /// </summary>
        public async Task<List<bool?>> GetWeeklyProgressAsync()
        {
            var activePlan = await _databaseService.GetActivePlanAsync();
            var result = new List<bool?>(7);
            var today = DateTime.Now.Date;
            var monday = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));

            if (activePlan == null)
            {
                for (int i = 0; i < 7; i++)
                    result.Add(null);
                return result;
            }

            var planStartDate = activePlan.Created.Date;

            for (int i = 0; i < 7; i++)
            {
                var date = monday.AddDays(i);

                if (date < planStartDate || date > today)
                {
                    result.Add(null);
                    continue;
                }

                var planDay = await _databaseService.GetDynamicPlanDayForDateAsync(activePlan, date);

                if (planDay == null || !planDay.IsTrainingDay)
                {
                    result.Add(null); // rest day
                    continue;
                }

                var progress = await _databaseService.GetDailyProgressAsync(planDay.Id, date);
                result.Add(progress != null && progress.IsComplete);
            }

            return result;
        }

        /// <summary>
        /// Gets the total number of completed training days for the active plan.
        /// </summary>
        public async Task<int> GetTotalCompletedWorkoutsAsync()
        {
            var sessions = await _databaseService.GetWorkoutSessionsAsync();
            return sessions.Count;
        }

        /// <summary>
        /// Gets the number of remaining (not yet completed) exercises for today's training day.
        /// Returns (remaining, total) or (0, 0) if today is a rest day or no plan is active.
        /// </summary>
        public async Task<(int Remaining, int Total)> GetRemainingExercisesTodayAsync()
        {
            var planDay = await _databaseService.GetTodaysPlanDayAsync();
            if (planDay == null)
                return (0, 0);

            var exercises = await _databaseService.GetExercisesForDayAsync(planDay.Id);
            var totalExercises = exercises.Count;

            if (totalExercises == 0)
                return (0, 0);

            var today = DateTime.Now.Date;
            var completions = await _databaseService.GetExerciseCompletionsForDateAsync(today);
            var completedIds = new HashSet<int>(completions.Select(c => c.PlanExerciseId));
            var completedCount = exercises.Count(e => completedIds.Contains(e.Id));

            return (totalExercises - completedCount, totalExercises);
        }

        private async Task UpdateDailyProgressByPlanDayAsync(int planDayId, DateTime date)
        {
            var dateOnly = date.Date;

            var progress = await _databaseService.GetDailyProgressAsync(planDayId, dateOnly);
            var exercises = await _databaseService.GetExercisesForDayAsync(planDayId);
            var completions = await _databaseService.GetExerciseCompletionsForDateAsync(dateOnly);
            var completedIds = new HashSet<int>(completions.Select(c => c.PlanExerciseId));
            var completedCount = exercises.Count(e => completedIds.Contains(e.Id));

            if (progress == null)
            {
                progress = new DailyProgress
                {
                    PlanDayId = planDayId,
                    Date = dateOnly,
                    CompletedExerciseCount = completedCount,
                    TotalExerciseCount = exercises.Count,
                    LastUpdated = DateTime.UtcNow
                };
                await _databaseService.AddDailyProgressAsync(progress);
            }
            else
            {
                progress.CompletedExerciseCount = completedCount;
                progress.TotalExerciseCount = exercises.Count;
                progress.LastUpdated = DateTime.UtcNow;
                await _databaseService.UpdateDailyProgressAsync(progress);
            }
        }
    }
}
