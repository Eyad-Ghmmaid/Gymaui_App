using Gymaui_App.Models;
using Gymaui_App.Utilities;
using SQLite;
using System.Text.Json;

namespace Gymaui_App.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _db;
        private const int DB_VERSION = 5;

        public async Task InitializeAsync()
        {
            if (_db != null)
                return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "gymapp.db3");
            _db = new SQLiteAsyncConnection(dbPath);

            // Check if migration is needed
            int savedVersion = Preferences.Get("db_version", 0);

            if (savedVersion < DB_VERSION)
            {
                // Drop old tables for clean migration
                try
                {
                    await _db.DropTableAsync<ExerciseLog>();
                    await _db.DropTableAsync<WorkoutSession>();
                    await _db.DropTableAsync<Models.ExerciseCompletion>();
                    await _db.DropTableAsync<Models.DailyProgress>();
                    await _db.DropTableAsync<Models.PlanExercise>();
                    await _db.DropTableAsync<Models.PlanDay>();
                    await _db.DropTableAsync<Models.Plan>();
                    await _db.DropTableAsync<Exercise>();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error dropping tables: {ex.Message}");
                }

                System.Diagnostics.Debug.WriteLine($"Database migration: v{savedVersion} -> v{DB_VERSION}");
            }

            // Create tables
            await _db.CreateTableAsync<Exercise>();
            await _db.CreateTableAsync<WorkoutSession>();
            await _db.CreateTableAsync<ExerciseLog>();
            await _db.CreateTableAsync<Models.Plan>();
            await _db.CreateTableAsync<Models.PlanDay>();
            await _db.CreateTableAsync<Models.PlanExercise>();
            await _db.CreateTableAsync<Models.DailyProgress>();
            await _db.CreateTableAsync<Models.ExerciseCompletion>();

            // Create indices for frequently queried columns
            await _db.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_exerciselog_exerciseid ON ExerciseLogs(ExerciseId)");
            await _db.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_exerciselog_sessionid ON ExerciseLogs(WorkoutSessionId)");
            await _db.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_planday_planid ON PlanDays(PlanId)");
            await _db.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_planexercise_plandayid ON PlanExercises(PlanDayId)");
            await _db.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_dailyprogress_date ON DailyProgress(PlanDayId, Date)");
            await _db.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_exercisecompletion_date ON ExerciseCompletion(PlanExerciseId, Date)");
            await _db.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_workoutsession_date ON WorkoutSessions(Date)");

            // Seed data if needed
            if (savedVersion < DB_VERSION)
            {
                await SeedDefaultExercisesAsync();
                await SeedDefaultPlanAsync();
                Preferences.Set("db_version", DB_VERSION);
            }
        }

        // Exercise CRUD
        public Task<int> AddExerciseAsync(Exercise exercise)
            => _db!.InsertAsync(exercise);

        public Task<List<Exercise>> GetExercisesAsync()
            => _db!.Table<Exercise>().ToListAsync();

        public Task<Exercise?> GetExerciseAsync(int id)
            => _db!.Table<Exercise>().Where(e => e.Id == id).FirstOrDefaultAsync();

        public Task<int> UpdateExerciseAsync(Exercise exercise)
            => _db!.UpdateAsync(exercise);

        public Task<int> DeleteExerciseAsync(Exercise exercise)
            => _db!.DeleteAsync(exercise);

        /// <summary>
        /// Loads multiple exercises by their IDs in a single query (avoids N+1).
        /// Returns exercises in the order of the provided IDs.
        /// </summary>
        public async Task<List<Exercise>> GetExercisesByIdsAsync(IEnumerable<int> ids)
        {
            var idList = ids.ToList();
            if (idList.Count == 0)
                return new List<Exercise>();

            var allExercises = await _db!.Table<Exercise>()
                .Where(e => idList.Contains(e.Id))
                .ToListAsync();

            // Preserve the requested order
            var lookup = allExercises.ToDictionary(e => e.Id);
            return idList
                .Where(id => lookup.ContainsKey(id))
                .Select(id => lookup[id])
                .ToList();
        }

        // Plan CRUD
        public Task<int> AddPlanAsync(Models.Plan plan)
            => _db!.InsertAsync(plan);

        public Task<List<Models.Plan>> GetPlansAsync()
            => _db!.Table<Models.Plan>().ToListAsync();

        public Task<Models.Plan?> GetPlanAsync(int id)
            => _db!.Table<Models.Plan>().Where(p => p.Id == id).FirstOrDefaultAsync();

        public Task<Models.Plan?> GetActivePlanAsync()
            => _db!.Table<Models.Plan>().Where(p => p.IsActive).FirstOrDefaultAsync();

        public Task<int> UpdatePlanAsync(Models.Plan plan)
            => _db!.UpdateAsync(plan);

        public Task<int> DeletePlanAsync(Models.Plan plan)
            => _db!.DeleteAsync(plan);

        public async Task SetActivePlanAsync(int planId)
        {
            // Deactivate all other plans in one query
            await _db!.ExecuteAsync("UPDATE Plans SET IsActive = 0 WHERE Id != ?", planId);
            // Activate the target plan
            await _db!.ExecuteAsync("UPDATE Plans SET IsActive = 1 WHERE Id = ?", planId);
        }

        // PlanDay / PlanExercise basic helpers
        public Task<int> AddPlanDayAsync(Models.PlanDay day)
            => _db!.InsertAsync(day);

        public Task<List<Models.PlanDay>> GetDaysForPlanAsync(int planId)
            => _db!.Table<Models.PlanDay>().Where(d => d.PlanId == planId).OrderBy(d => d.Order).ToListAsync();

        public Task<Models.PlanDay?> GetPlanDayAsync(int id)
            => _db!.Table<Models.PlanDay>().Where(d => d.Id == id).FirstOrDefaultAsync();

        public Task<Models.PlanDay?> GetPlanDayByDayOfWeekAsync(int planId, int dayOfWeek)
            => _db!.Table<Models.PlanDay>().Where(d => d.PlanId == planId && d.DayOfWeek == dayOfWeek).FirstOrDefaultAsync();

        public Task<Models.PlanDay?> GetPlanDayByPlanAndIndexAsync(int planId, int dayIndex)
            => _db!.Table<Models.PlanDay>().Where(d => d.PlanId == planId && d.Order == dayIndex).FirstOrDefaultAsync();

        public Task<int> DeletePlanDayAsync(Models.PlanDay day)
            => _db!.DeleteAsync(day);

        public Task<int> UpdatePlanDayAsync(Models.PlanDay day)
            => _db!.UpdateAsync(day);

        /// <summary>
        /// Gets today's training day using dynamic schedule shifting.
        /// If a training day was missed (not completed), it becomes the current day
        /// instead of advancing to the next one. Rest days always advance automatically.
        /// Returns null if today is a rest day or no active plan exists.
        /// </summary>
        public async Task<Models.PlanDay?> GetTodaysPlanDayAsync()
        {
            var activePlan = await GetActivePlanAsync();
            if (activePlan == null)
                return null;

            var today = DateTime.Now.Date;
            var planDay = await GetDynamicPlanDayForDateAsync(activePlan, today);

            // Return only if it's a training day
            if (planDay != null && planDay.IsTrainingDay)
                return planDay;

            return null;
        }

        /// <summary>
        /// Determines which plan day is assigned to a given date using dynamic shifting.
        /// Walks forward from the plan start date, pausing on incomplete training days.
        /// </summary>
        public async Task<Models.PlanDay?> GetDynamicPlanDayForDateAsync(Models.Plan plan, DateTime date)
        {
            var targetDate = date.Date;
            var planStartDate = plan.Created.Date;

            if (targetDate < planStartDate)
                return null;

            var allDays = await GetDaysForPlanAsync(plan.Id);
            if (allDays.Count == 0)
                return null;

            var orderedDays = allDays.OrderBy(d => d.Order).ToList();
            var totalDaysInCycle = orderedDays.Count;

            // Pre-load all completed daily progress records from plan start to target date
            var allProgress = new Dictionary<(int PlanDayId, DateTime Date), Models.DailyProgress>();
            foreach (var day in orderedDays.Where(d => d.IsTrainingDay))
            {
                var progressList = await GetDailyProgressInRangeAsync(day.Id, planStartDate, targetDate);
                foreach (var p in progressList)
                {
                    allProgress[(p.PlanDayId, p.Date.Date)] = p;
                }
            }

            // Walk from plan start to target date
            int currentDayIndex = 0; // index into orderedDays
            var currentDate = planStartDate;

            while (currentDate <= targetDate)
            {
                var planDay = orderedDays[currentDayIndex % totalDaysInCycle];

                if (currentDate == targetDate)
                    return planDay;

                if (!planDay.IsTrainingDay)
                {
                    // Rest days always advance
                    currentDayIndex++;
                }
                else
                {
                    // Training day: only advance if completed on this date
                    var key = (planDay.Id, currentDate);
                    if (allProgress.TryGetValue(key, out var progress) && progress.IsComplete)
                    {
                        currentDayIndex++;
                    }
                    // else: stay on the same training day (shift forward)
                }

                currentDate = currentDate.AddDays(1);
            }

            return null;
        }

        /// <summary>
        /// Gets all daily progress records in a date range for a given plan day.
        /// </summary>
        public Task<List<Models.DailyProgress>> GetAllDailyProgressAsync()
        {
            return _db!.Table<Models.DailyProgress>().ToListAsync();
        }

        public Task<int> AddPlanExerciseAsync(Models.PlanExercise pe)
            => _db!.InsertAsync(pe);

        public Task<List<Models.PlanExercise>> GetExercisesForDayAsync(int planDayId)
            => _db!.Table<Models.PlanExercise>().Where(e => e.PlanDayId == planDayId).ToListAsync();

        public Task<Models.PlanExercise?> GetPlanExerciseAsync(int id)
            => _db!.Table<Models.PlanExercise>().Where(e => e.Id == id).FirstOrDefaultAsync();

        public Task<int> DeletePlanExerciseAsync(Models.PlanExercise pe)
            => _db!.DeleteAsync(pe);

        public async Task DeletePlanAndChildrenAsync(int planId)
        {
            // Delete all child exercises for all days of this plan in one query
            await _db!.ExecuteAsync(
                "DELETE FROM PlanExercises WHERE PlanDayId IN (SELECT Id FROM PlanDays WHERE PlanId = ?)", planId);
            // Delete all days for this plan
            await _db!.ExecuteAsync("DELETE FROM PlanDays WHERE PlanId = ?", planId);
            // Delete the plan itself
            await _db!.ExecuteAsync("DELETE FROM Plans WHERE Id = ?", planId);
        }

        // WorkoutSession CRUD
        public async Task<int> AddWorkoutSessionAsync(WorkoutSession session)
        {
            // Ensure ExercisesJson is up to date
            session.ExercisesJson = JsonSerializer.Serialize(session.Exercises);
            return await _db!.InsertAsync(session);
        }

        public Task<List<WorkoutSession>> GetWorkoutSessionsAsync()
            => _db!.Table<WorkoutSession>().ToListAsync();

        public Task<WorkoutSession?> GetWorkoutSessionAsync(int id)
            => _db!.Table<WorkoutSession>().Where(s => s.Id == id).FirstOrDefaultAsync();

        public async Task<int> UpdateWorkoutSessionAsync(WorkoutSession session)
        {
            session.ExercisesJson = JsonSerializer.Serialize(session.Exercises);
            return await _db!.UpdateAsync(session);
        }

        public Task<int> DeleteWorkoutSessionAsync(WorkoutSession session)
            => _db!.DeleteAsync(session);

        /// <summary>
        /// Finds an existing workout session for today (by UTC date).
        /// Returns null if none exists.
        /// </summary>
        public async Task<WorkoutSession?> GetTodaysSessionAsync()
        {
            var todayUtc = DateTime.UtcNow.Date;
            var tomorrowUtc = todayUtc.AddDays(1);
            var sessions = await _db!.Table<WorkoutSession>()
                .Where(s => s.Date >= todayUtc && s.Date < tomorrowUtc)
                .ToListAsync();
            // Return the first session that actually has exercises
            return sessions
                .Where(s => !string.IsNullOrEmpty(s.ExercisesJson) && s.ExercisesJson != "[]")
                .OrderByDescending(s => s.Date)
                .FirstOrDefault();
        }

        // ExerciseLog CRUD
        public Task<int> AddExerciseLogAsync(ExerciseLog log)
            => _db!.InsertAsync(log);

        public Task<int> AddExerciseLogsAsync(IEnumerable<ExerciseLog> logs)
            => _db!.InsertAllAsync(logs);

        public Task<int> UpdateExerciseLogAsync(ExerciseLog log)
            => _db!.UpdateAsync(log);

        public Task<int> DeleteExerciseLogAsync(ExerciseLog log)
            => _db!.DeleteAsync(log);

        public Task<List<ExerciseLog>> GetLogsForWorkoutSessionAsync(int workoutSessionId)
            => _db!.Table<ExerciseLog>().Where(l => l.WorkoutSessionId == workoutSessionId).ToListAsync();

        public Task<List<ExerciseLog>> GetLogsForExerciseAsync(int exerciseId)
            => _db!.Table<ExerciseLog>().Where(l => l.ExerciseId == exerciseId).ToListAsync();

        // Database Management
        public async Task ClearAllDataAsync()
        {
            try
            {
                await _db!.DeleteAllAsync<ExerciseLog>();
                await _db!.DeleteAllAsync<WorkoutSession>();
                await _db!.DeleteAllAsync<Models.ExerciseCompletion>();
                await _db!.DeleteAllAsync<Models.DailyProgress>();
                await _db!.DeleteAllAsync<Exercise>();
                await _db!.DeleteAllAsync<Models.PlanExercise>();
                await _db!.DeleteAllAsync<Models.PlanDay>();
                await _db!.DeleteAllAsync<Models.Plan>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing database: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Resets the entire app data: clears all tables, resets preferences, and re-seeds default exercises.
        /// </summary>
        public async Task ResetAllDataAsync()
        {
            try
            {
                await ClearAllDataAsync();
                await SeedDefaultExercisesAsync();
                Preferences.Clear();
                Preferences.Set("db_version", DB_VERSION);
                System.Diagnostics.Debug.WriteLine("All data has been reset successfully.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error resetting all data: {ex.Message}");
                throw;
            }
        }

        public static void DeleteDatabaseFile()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "gymapp.db3");
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
                System.Diagnostics.Debug.WriteLine($"Database file deleted: {dbPath}");
            }
        }

        // DailyProgress CRUD
        public Task<int> AddDailyProgressAsync(Models.DailyProgress progress)
            => _db!.InsertAsync(progress);

        public Task<Models.DailyProgress?> GetDailyProgressAsync(int planDayId, DateTime date)
        {
            var dateOnly = date.Date;
            return _db!.Table<Models.DailyProgress>()
                .Where(p => p.PlanDayId == planDayId && p.Date == dateOnly)
                .FirstOrDefaultAsync();
        }

        public Task<List<Models.DailyProgress>> GetDailyProgressForDateAsync(DateTime date)
        {
            var dateOnly = date.Date;
            return _db!.Table<Models.DailyProgress>()
                .Where(p => p.Date == dateOnly)
                .ToListAsync();
        }

        public Task<List<Models.DailyProgress>> GetDailyProgressInRangeAsync(int planDayId, DateTime startDate, DateTime endDate)
        {
            var start = startDate.Date;
            var end = endDate.Date;
            return _db!.Table<Models.DailyProgress>()
                .Where(p => p.PlanDayId == planDayId && p.Date >= start && p.Date <= end)
                .OrderByDescending(p => p.Date)
                .ToListAsync();
        }

        public Task<int> UpdateDailyProgressAsync(Models.DailyProgress progress)
            => _db!.UpdateAsync(progress);

        public Task<int> DeleteDailyProgressAsync(Models.DailyProgress progress)
            => _db!.DeleteAsync(progress);

        // ExerciseCompletion CRUD
        public Task<int> AddExerciseCompletionAsync(Models.ExerciseCompletion completion)
            => _db!.InsertAsync(completion);

        public Task<Models.ExerciseCompletion?> GetExerciseCompletionAsync(int planExerciseId, DateTime date)
        {
            var dateOnly = date.Date;
            return _db!.Table<Models.ExerciseCompletion>()
                .Where(c => c.PlanExerciseId == planExerciseId && c.Date == dateOnly)
                .FirstOrDefaultAsync();
        }

        public Task<List<Models.ExerciseCompletion>> GetExerciseCompletionsForDateAsync(DateTime date)
        {
            var dateOnly = date.Date;
            return _db!.Table<Models.ExerciseCompletion>()
                .Where(c => c.Date == dateOnly && c.IsCompleted)
                .ToListAsync();
        }

        public Task<int> UpdateExerciseCompletionAsync(Models.ExerciseCompletion completion)
            => _db!.UpdateAsync(completion);

        public Task<int> DeleteExerciseCompletionAsync(Models.ExerciseCompletion completion)
            => _db!.DeleteAsync(completion);

        // Seed Data
        private async Task SeedDefaultExercisesAsync()
        {
            try
            {
                var defaultExercises = new List<Exercise>
                {
                    // BRUST
                    new() { Name = "Barbell Bench Press", MuscleGroup = MuscleGroups.Brust, YouTubeUrl = "https://www.youtube.com/watch?v=Blpub1haOj8", TargetSets = 4, TargetReps = 8 },
                    new() { Name = "Incline Bench Press", MuscleGroup = MuscleGroups.Brust, YouTubeUrl = "https://www.youtube.com/watch?v=R8kRvIzUbUc", TargetSets = 3, TargetReps = 10 },
                    new() { Name = "Incline Dumbbell Press", MuscleGroup = MuscleGroups.Brust, YouTubeUrl = "https://www.youtube.com/watch?v=8iPEnn-ltC8", TargetSets = 3, TargetReps = 10 },
                    new() { Name = "Chest Press Machine", MuscleGroup = MuscleGroups.Brust, YouTubeUrl = "https://www.youtube.com/watch?v=sW0BRP315U8", TargetSets = 3, TargetReps = 12 },
                    new() { Name = "Cable Fly", MuscleGroup = MuscleGroups.Brust, YouTubeUrl = "https://www.youtube.com/watch?v=eozdVDA78K0", TargetSets = 3, TargetReps = 12 },
                    new() { Name = "Pec Deck (Butterfly)", MuscleGroup = MuscleGroups.Brust, YouTubeUrl = "https://www.youtube.com/watch?v=vSU49q3sXuw", TargetSets = 3, TargetReps = 12 },
                    new() { Name = "Dips", MuscleGroup = MuscleGroups.Brust, YouTubeUrl = "https://www.youtube.com/watch?v=2z8JmcrW-As", TargetSets = 3, TargetReps = 10 },
                    new() { Name = "Push-Up", MuscleGroup = MuscleGroups.Brust, YouTubeUrl = "https://www.youtube.com/watch?v=IODxDxX7oi4", TargetSets = 3, TargetReps = 15 },
                    new() { Name = "Decline Bench Press", MuscleGroup = MuscleGroups.Brust, YouTubeUrl = "https://www.youtube.com/watch?v=LfyQBUKR8SE", TargetSets = 3, TargetReps = 10 },
                    new() { Name = "Cable Crossover", MuscleGroup = MuscleGroups.Brust, YouTubeUrl = "https://www.youtube.com/watch?v=taI4XduLpTk", TargetSets = 3, TargetReps = 12 },

                    // RÜCKEN
                    new() { Name = "Pull-Up", MuscleGroup = MuscleGroups.Rücken, YouTubeUrl = "https://www.youtube.com/watch?v=eGo4IYlbE5g", TargetSets = 3, TargetReps = 10 },
                    new() { Name = "Lat Pulldown", MuscleGroup = MuscleGroups.Rücken, YouTubeUrl = "https://www.youtube.com/watch?v=UjVMfCG3H0s", TargetSets = 4, TargetReps = 10 },
                    new() { Name = "Barbell Row", MuscleGroup = MuscleGroups.Rücken, YouTubeUrl = "https://www.youtube.com/watch?v=vT2GjY_Umpw", TargetSets = 4, TargetReps = 8 },
                    new() { Name = "Seated Cable Row", MuscleGroup = MuscleGroups.Rücken, YouTubeUrl = "https://www.youtube.com/watch?v=87cYGLj-yE4", TargetSets = 3, TargetReps = 12 },
                    new() { Name = "Machine Row", MuscleGroup = MuscleGroups.Rücken, YouTubeUrl = "https://www.youtube.com/watch?v=DELI3AMBNuA", TargetSets = 3, TargetReps = 12 },
                    new() { Name = "T-Bar Row", MuscleGroup = MuscleGroups.Rücken, YouTubeUrl = "https://www.youtube.com/watch?v=j3Igk5nyZE4", TargetSets = 4, TargetReps = 8 },
                    new() { Name = "One Arm Dumbbell Row", MuscleGroup = MuscleGroups.Rücken, YouTubeUrl = "https://www.youtube.com/watch?v=pYcpY20QaE8", TargetSets = 3, TargetReps = 10 },
                    new() { Name = "Straight Arm Pulldown", MuscleGroup = MuscleGroups.Rücken, YouTubeUrl = "https://www.youtube.com/watch?v=AJ3j4i9mH4c", TargetSets = 3, TargetReps = 12 },
                    new() { Name = "Chest Supported Row", MuscleGroup = MuscleGroups.Rücken, YouTubeUrl = "https://www.youtube.com/watch?v=GZbfZ033f74", TargetSets = 3, TargetReps = 12 },

                    // SCHULTERN
                    new() { Name = "Overhead Press", MuscleGroup = MuscleGroups.Schultern, YouTubeUrl = "https://www.youtube.com/watch?v=7lwrBIu70tY", TargetSets = 4, TargetReps = 8 },
                    new() { Name = "Lateral Raise", MuscleGroup = MuscleGroups.Schultern, YouTubeUrl = "https://www.youtube.com/watch?v=H_JDx09iftw", TargetSets = 3, TargetReps = 12 },
                    new() { Name = "Rear Delt Fly", MuscleGroup = MuscleGroups.Schultern, YouTubeUrl = "https://www.youtube.com/watch?v=bB1m4rcTnoE", TargetSets = 3, TargetReps = 12 },
                    new() { Name = "Face Pull", MuscleGroup = MuscleGroups.Schultern, YouTubeUrl = "https://www.youtube.com/watch?v=rep-qVOkqgk", TargetSets = 3, TargetReps = 15 },
                    new() { Name = "Shrugs", MuscleGroup = MuscleGroups.Schultern, YouTubeUrl = "https://www.youtube.com/watch?v=cJRVVxmytaM", TargetSets = 3, TargetReps = 12 },
                    new() { Name = "Arnold Press", MuscleGroup = MuscleGroups.Schultern, YouTubeUrl = "https://www.youtube.com/watch?v=vj2w851ZHRM", TargetSets = 3, TargetReps = 10 },
                    new() { Name = "Front Raise", MuscleGroup = MuscleGroups.Schultern, YouTubeUrl = "https://www.youtube.com/watch?v=-t7fuZ0KhDA", TargetSets = 3, TargetReps = 12 },
                    new() { Name = "Cable Lateral Raise", MuscleGroup = MuscleGroups.Schultern, YouTubeUrl = "https://www.youtube.com/watch?v=3VcKaXpzqRo", TargetSets = 3, TargetReps = 12 },

                    // BIZEPS
                    new() { Name = "Barbell Curl", MuscleGroup = MuscleGroups.Bizeps, YouTubeUrl = "https://www.youtube.com/watch?v=kwG2ipFRgfo", TargetSets = 3, TargetReps = 10 },
                    new() { Name = "Dumbbell Curl", MuscleGroup = MuscleGroups.Bizeps, YouTubeUrl = "https://www.youtube.com/watch?v=Inv3C3Fh-jQ", TargetSets = 3, TargetReps = 10 },
                    new() { Name = "Hammer Curl", MuscleGroup = MuscleGroups.Bizeps, YouTubeUrl = "https://www.youtube.com/watch?v=Wfvzm8narGk", TargetSets = 3, TargetReps = 10 },
                    new() { Name = "Preacher Curl", MuscleGroup = MuscleGroups.Bizeps, YouTubeUrl = "https://www.youtube.com/watch?v=fIWP-FRFNU0", TargetSets = 3, TargetReps = 10 },
                    new() { Name = "Concentration Curl", MuscleGroup = MuscleGroups.Bizeps, YouTubeUrl = "https://www.youtube.com/watch?v=0AUGkch3tzc", TargetSets = 3, TargetReps = 12 },
                    new() { Name = "Cable Curl", MuscleGroup = MuscleGroups.Bizeps, YouTubeUrl = "https://www.youtube.com/watch?v=av7-8igSXTs", TargetSets = 3, TargetReps = 12 },

                    // TRIZEPS
                    new() { Name = "Triceps Pushdown", MuscleGroup = MuscleGroups.Trizeps, YouTubeUrl = "https://www.youtube.com/watch?v=4I8ZBLj6ViM", TargetSets = 3, TargetReps = 12 },
                    new() { Name = "Overhead Triceps Extension", MuscleGroup = MuscleGroups.Trizeps, YouTubeUrl = "https://www.youtube.com/watch?v=LNsLtJd47gA", TargetSets = 3, TargetReps = 12 },
                    new() { Name = "Skullcrusher", MuscleGroup = MuscleGroups.Trizeps, YouTubeUrl = "https://www.youtube.com/watch?v=d_KZxkY_0cM", TargetSets = 3, TargetReps = 10 },
                    new() { Name = "Close Grip Bench Press", MuscleGroup = MuscleGroups.Trizeps, YouTubeUrl = "https://www.youtube.com/watch?v=nEF0bv2FW94", TargetSets = 3, TargetReps = 10 },
                    new() { Name = "Triceps Dip Machine", MuscleGroup = MuscleGroups.Trizeps, YouTubeUrl = "https://www.youtube.com/watch?v=6kALZikXxLc", TargetSets = 3, TargetReps = 12 },

                    // BEINE
                    new() { Name = "Barbell Squat", MuscleGroup = MuscleGroups.Beine, YouTubeUrl = "https://www.youtube.com/watch?v=Dy28eq2PjcM", TargetSets = 4, TargetReps = 8 },
                    new() { Name = "Romanian Deadlift", MuscleGroup = MuscleGroups.Beine, YouTubeUrl = "https://www.youtube.com/watch?v=2SHsk9AzdjA", TargetSets = 4, TargetReps = 8 },
                    new() { Name = "Leg Press", MuscleGroup = MuscleGroups.Beine, YouTubeUrl = "https://www.youtube.com/watch?v=PyBoLzA6tgs", TargetSets = 4, TargetReps = 10 },
                    new() { Name = "Leg Curl", MuscleGroup = MuscleGroups.Beine, YouTubeUrl = "https://www.youtube.com/watch?v=W4z57p5BB0o", TargetSets = 3, TargetReps = 12 },
                    new() { Name = "Leg Extension", MuscleGroup = MuscleGroups.Beine, YouTubeUrl = "https://www.youtube.com/watch?v=Fm1ZaCe4Syc", TargetSets = 3, TargetReps = 12 },
                    new() { Name = "Lunges", MuscleGroup = MuscleGroups.Beine, YouTubeUrl = "https://www.youtube.com/watch?v=QOVaHwm-Q6U", TargetSets = 3, TargetReps = 10 },
                    new() { Name = "Bulgarian Split Squat", MuscleGroup = MuscleGroups.Beine, YouTubeUrl = "https://www.youtube.com/watch?v=2C-uNgKwPLE", TargetSets = 3, TargetReps = 10 },
                    new() { Name = "Hip Thrust", MuscleGroup = MuscleGroups.Beine, YouTubeUrl = "https://www.youtube.com/watch?v=LM8XHLYJoYs", TargetSets = 4, TargetReps = 10 },
                    new() { Name = "Hack Squat Machine", MuscleGroup = MuscleGroups.Beine, YouTubeUrl = "https://www.youtube.com/watch?v=0tn5K9NlCfo", TargetSets = 4, TargetReps = 10 },
                    new() { Name = "Standing Calf Raise", MuscleGroup = MuscleGroups.Beine, YouTubeUrl = "https://www.youtube.com/watch?v=0QRosgBfK50", TargetSets = 4, TargetReps = 15 },
                    new() { Name = "Seated Calf Raise", MuscleGroup = MuscleGroups.Beine, YouTubeUrl = "https://www.youtube.com/watch?v=YMmgqO8Jo-k", TargetSets = 4, TargetReps = 15 },

                    // CORE
                    new() { Name = "Hanging Leg Raise", MuscleGroup = MuscleGroups.Core, YouTubeUrl = "https://www.youtube.com/watch?v=JB2oyawG9KI", TargetSets = 3, TargetReps = 12 },
                    new() { Name = "Plank", MuscleGroup = MuscleGroups.Core, YouTubeUrl = "https://www.youtube.com/watch?v=pSHjTRCQxIw", TargetSets = 3, TargetReps = 60 },
                    new() { Name = "Cable Crunch", MuscleGroup = MuscleGroups.Core, YouTubeUrl = "https://www.youtube.com/watch?v=2fjejk9g9fQ", TargetSets = 3, TargetReps = 15 },
                    new() { Name = "Russian Twist", MuscleGroup = MuscleGroups.Core, YouTubeUrl = "https://www.youtube.com/watch?v=wkD8rjkodUI", TargetSets = 3, TargetReps = 20 },
                };

                await _db!.InsertAllAsync(defaultExercises);
                System.Diagnostics.Debug.WriteLine($"Seeded {defaultExercises.Count} default exercises");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error seeding exercises: {ex.Message}");
            }
        }

        private async Task SeedDefaultPlanAsync()
        {
            try
            {
                // Create the default plan and set it as active
                var plan = new Models.Plan
                {
                    Name = "4-5 Tage Split",
                    Created = DateTime.UtcNow,
                    IsActive = true
                };
                await _db!.InsertAsync(plan);

                // Helper: resolve exercise ID by name (falls back to 0 if not found)
                async Task<int> ExId(string name)
                {
                    var ex = await _db.Table<Exercise>().Where(e => e.Name == name).FirstOrDefaultAsync();
                    return ex?.Id ?? 0;
                }

                // ?? Tag 1 – Brust & Trizeps (DayOfWeek = 0, Order = 0) ??????????????
                var day1 = new Models.PlanDay { PlanId = plan.Id, Name = "Tag 1 – Brust & Trizeps", DayOfWeek = 0, Order = 0, IsTrainingDay = true };
                await _db.InsertAsync(day1);
                var day1Exercises = new (string Name, int Sets, int Reps)[] {
                    ("Barbell Bench Press", 4, 8),
                    ("Incline Dumbbell Press", 3, 10),
                    ("Cable Fly", 3, 12),
                    ("Dips", 3, 10),
                    ("Triceps Pushdown", 3, 12),
                    ("Overhead Triceps Extension", 3, 10),
                };
                for (int i = 0; i < day1Exercises.Length; i++)
                {
                    var (name, sets, reps) = day1Exercises[i];
                    var exId = await ExId(name);
                    if (exId > 0)
                        await _db.InsertAsync(new Models.PlanExercise { PlanDayId = day1.Id, ExerciseId = exId, Order = i });
                }

                // ?? Tag 2 – Rücken & Bizeps (DayOfWeek = 1, Order = 1) ??????????????
                var day2 = new Models.PlanDay { PlanId = plan.Id, Name = "Tag 2 – Rücken & Bizeps", DayOfWeek = 1, Order = 1, IsTrainingDay = true };
                await _db.InsertAsync(day2);
                var day2Exercises = new (string Name, int Sets, int Reps)[] {
                    ("Pull-Up", 4, 10),
                    ("Barbell Row", 4, 10),
                    ("Lat Pulldown", 3, 10),
                    ("Seated Cable Row", 3, 10),
                    ("Barbell Curl", 3, 10),
                    ("Hammer Curl", 3, 12),
                };
                for (int i = 0; i < day2Exercises.Length; i++)
                {
                    var (name, sets, reps) = day2Exercises[i];
                    var exId = await ExId(name);
                    if (exId > 0)
                        await _db.InsertAsync(new Models.PlanExercise { PlanDayId = day2.Id, ExerciseId = exId, Order = i });
                }

                // ?? Tag 3 – Schultern & Core (DayOfWeek = 2, Order = 2) ?????????????
                var day3 = new Models.PlanDay { PlanId = plan.Id, Name = "Tag 3 – Schultern & Core", DayOfWeek = 2, Order = 2, IsTrainingDay = true };
                await _db.InsertAsync(day3);
                var day3Exercises = new (string Name, int Sets, int Reps)[] {
                    ("Overhead Press", 4, 8),
                    ("Lateral Raise", 4, 15),
                    ("Rear Delt Fly", 3, 15),
                    ("Face Pull", 3, 15),
                    ("Shrugs", 3, 12),
                    ("Hanging Leg Raise", 3, 12),
                    ("Plank", 3, 45),
                };
                for (int i = 0; i < day3Exercises.Length; i++)
                {
                    var (name, sets, reps) = day3Exercises[i];
                    var exId = await ExId(name);
                    if (exId > 0)
                        await _db.InsertAsync(new Models.PlanExercise { PlanDayId = day3.Id, ExerciseId = exId, Order = i });
                }

                // ?? Tag 4 – Beine (DayOfWeek = 3, Order = 3) ????????????????????????
                var day4 = new Models.PlanDay { PlanId = plan.Id, Name = "Tag 4 – Beine", DayOfWeek = 3, Order = 3, IsTrainingDay = true };
                await _db.InsertAsync(day4);
                var day4Exercises = new (string Name, int Sets, int Reps)[] {
                    ("Barbell Squat", 4, 8),
                    ("Romanian Deadlift", 3, 10),
                    ("Leg Press", 3, 10),
                    ("Leg Curl", 3, 12),
                    ("Leg Extension", 3, 12),
                    ("Standing Calf Raise", 4, 15),
                };
                for (int i = 0; i < day4Exercises.Length; i++)
                {
                    var (name, sets, reps) = day4Exercises[i];
                    var exId = await ExId(name);
                    if (exId > 0)
                        await _db.InsertAsync(new Models.PlanExercise { PlanDayId = day4.Id, ExerciseId = exId, Order = i });
                }

                // ?? Tag 5 – Arme & Brust Pump (DayOfWeek = 4, Order = 4) ????????????
                var day5 = new Models.PlanDay { PlanId = plan.Id, Name = "Tag 5 – Arme & Brust Pump", DayOfWeek = 4, Order = 4, IsTrainingDay = true };
                await _db.InsertAsync(day5);
                var day5Exercises = new (string Name, int Sets, int Reps)[] {
                    ("Incline Bench Press", 3, 8),
                    ("Cable Fly", 3, 12),
                    ("Preacher Curl", 3, 10),
                    ("Skullcrusher", 3, 10),
                    ("Lateral Raise", 3, 15),
                };
                for (int i = 0; i < day5Exercises.Length; i++)
                {
                    var (name, sets, reps) = day5Exercises[i];
                    var exId = await ExId(name);
                    if (exId > 0)
                        await _db.InsertAsync(new Models.PlanExercise { PlanDayId = day5.Id, ExerciseId = exId, Order = i });
                }

                // ?? Ruhetag 1 (DayOfWeek = 5, Order = 5) ????????????????????????????
                var rest1 = new Models.PlanDay { PlanId = plan.Id, Name = "Ruhetag", DayOfWeek = 5, Order = 5, IsTrainingDay = false };
                await _db.InsertAsync(rest1);

                // ?? Ruhetag 2 (DayOfWeek = 6, Order = 6) ????????????????????????????
                var rest2 = new Models.PlanDay { PlanId = plan.Id, Name = "Ruhetag", DayOfWeek = 6, Order = 6, IsTrainingDay = false };
                await _db.InsertAsync(rest2);

                System.Diagnostics.Debug.WriteLine("Seeded default plan: 4-5 Tage Split");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error seeding default plan: {ex.Message}");
            }
        }
    }
}
