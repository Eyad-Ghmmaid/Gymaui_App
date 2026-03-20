using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SQLite;
using Gymaui_App.Models;
using System.Text.Json;

namespace Gymaui_App.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _db;

        public async Task InitializeAsync()
        {
            if (_db != null)
                return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "gymapp.db3");
            _db = new SQLiteAsyncConnection(dbPath);

            await _db.CreateTableAsync<Exercise>();
            await _db.CreateTableAsync<WorkoutSession>();
            await _db.CreateTableAsync<ExerciseLog>();
            await _db.CreateTableAsync<Models.Plan>();
            await _db.CreateTableAsync<Models.PlanDay>();
            await _db.CreateTableAsync<Models.PlanExercise>();
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
            var plans = await GetPlansAsync();
            foreach (var p in plans)
            {
                if (p.IsActive && p.Id != planId)
                {
                    p.IsActive = false;
                    await UpdatePlanAsync(p);
                }
            }

            var target = await GetPlanAsync(planId);
            if (target != null && !target.IsActive)
            {
                target.IsActive = true;
                await UpdatePlanAsync(target);
            }
        }

        // PlanDay / PlanExercise basic helpers
        public Task<int> AddPlanDayAsync(Models.PlanDay day)
            => _db!.InsertAsync(day);

        public Task<List<Models.PlanDay>> GetDaysForPlanAsync(int planId)
            => _db!.Table<Models.PlanDay>().Where(d => d.PlanId == planId).OrderBy(d => d.Order).ToListAsync();

        public Task<Models.PlanDay?> GetPlanDayByDayOfWeekAsync(int planId, int dayOfWeek)
            => _db!.Table<Models.PlanDay>().Where(d => d.PlanId == planId && d.DayOfWeek == dayOfWeek).FirstOrDefaultAsync();

        public Task<Models.PlanDay?> GetPlanDayByPlanAndIndexAsync(int planId, int dayIndex)
            => _db!.Table<Models.PlanDay>().Where(d => d.PlanId == planId && d.Order == dayIndex).FirstOrDefaultAsync();

        public Task<int> DeletePlanDayAsync(Models.PlanDay day)
            => _db!.DeleteAsync(day);

        public Task<int> UpdatePlanDayAsync(Models.PlanDay day)
            => _db!.UpdateAsync(day);

        /// <summary>
        /// Gets the training day for today from the active plan
        /// </summary>
        public async Task<Models.PlanDay?> GetTodaysPlanDayAsync()
        {
            var activePlan = await GetActivePlanAsync();
            if (activePlan == null)
                return null;

            var today = (int)DateTime.Now.DayOfWeek;
            // Convert from .NET (0=Sunday) to our system (0=Monday)
            var dayOfWeek = (today + 6) % 7;

            return await GetPlanDayByDayOfWeekAsync(activePlan.Id, dayOfWeek);
        }

        public Task<int> AddPlanExerciseAsync(Models.PlanExercise pe)
            => _db!.InsertAsync(pe);

        public Task<List<Models.PlanExercise>> GetExercisesForDayAsync(int planDayId)
            => _db!.Table<Models.PlanExercise>().Where(e => e.PlanDayId == planDayId).ToListAsync();

        public Task<int> DeletePlanExerciseAsync(Models.PlanExercise pe)
            => _db!.DeleteAsync(pe);

        public async Task DeletePlanAndChildrenAsync(int planId)
        {
            var days = await GetDaysForPlanAsync(planId);
            foreach (var d in days)
            {
                var exercises = await GetExercisesForDayAsync(d.Id);
                foreach (var pe in exercises)
                {
                    await DeletePlanExerciseAsync(pe);
                }
                await DeletePlanDayAsync(d);
            }

            var plan = await GetPlanAsync(planId);
            if (plan != null)
                await DeletePlanAsync(plan);
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

        // ExerciseLog CRUD
        public Task<int> AddExerciseLogAsync(ExerciseLog log)
            => _db!.InsertAsync(log);

        public Task<int> AddExerciseLogsAsync(IEnumerable<ExerciseLog> logs)
            => _db!.InsertAllAsync(logs);

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

        public static void DeleteDatabaseFile()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "gymapp.db3");
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
                System.Diagnostics.Debug.WriteLine($"Database file deleted: {dbPath}");
            }
        }
    }
}
