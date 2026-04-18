using Gymaui_App.Models;
using Gymaui_App.Services;
using Gymaui_App.Utilities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Gymaui_App.ViewModels
{
    /// <summary>
    /// Represents a group of exercises by muscle group
    /// </summary>
    public class ExerciseGroup : List<Exercise>
    {
        public string MuscleGroup { get; set; }
        public string Icon => MuscleGroups.GetIcon(MuscleGroup);

        public ExerciseGroup(string muscleGroup, List<Exercise> exercises) : base(exercises)
        {
            MuscleGroup = muscleGroup;
        }
    }

    public class ExerciseListViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private List<Exercise> _allExercises = new();

        // UI Collections
        public ObservableCollection<ExerciseGroup> GroupedExercises { get; } = new();
        public ObservableCollection<Exercise> FilteredExercises { get; } = new();

        // Filter state
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value)
                    return;
                _searchText = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        private string _selectedMuscleGroup = "Alle";
        public string SelectedMuscleGroup
        {
            get => _selectedMuscleGroup;
            set
            {
                if (_selectedMuscleGroup == value)
                    return;
                _selectedMuscleGroup = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        // Available muscle groups for chips
        public List<string> AvailableMuscleGroups { get; } = new() { "Alle" };

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value)
                    return;
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        private string _emptyStateMessage = string.Empty;
        public string EmptyStateMessage
        {
            get => _emptyStateMessage;
            set
            {
                if (_emptyStateMessage == value)
                    return;
                _emptyStateMessage = value;
                OnPropertyChanged();
            }
        }

        private bool _showEmptyState;
        public bool ShowEmptyState
        {
            get => _showEmptyState;
            set
            {
                if (_showEmptyState == value)
                    return;
                _showEmptyState = value;
                OnPropertyChanged();
            }
        }

        private bool _isGroupedView;
        public bool IsGroupedView
        {
            get => _isGroupedView;
            set
            {
                if (_isGroupedView == value)
                    return;
                _isGroupedView = value;
                OnPropertyChanged();
            }
        }

        public ExerciseListViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            AvailableMuscleGroups.AddRange(MuscleGroups.All);
        }

        public async Task LoadExercisesAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                await _databaseService.InitializeAsync();
                _allExercises = await _databaseService.GetExercisesAsync();
                ApplyFilters();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ApplyFilters()
        {
            var filtered = _allExercises.ToList();

            // Apply muscle group filter
            if (SelectedMuscleGroup != "Alle")
            {
                filtered = filtered
                    .Where(e => e.MuscleGroup == SelectedMuscleGroup)
                    .ToList();
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered
                    .Where(e =>
                        e.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        e.MuscleGroup.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Determine view type: grouped or flat
            bool showGrouped = SelectedMuscleGroup == "Alle" && string.IsNullOrWhiteSpace(SearchText);
            IsGroupedView = showGrouped;

            if (showGrouped)
                UpdateGroupedView(filtered);
            else
                UpdateFlatView(filtered);
        }

        private void UpdateGroupedView(List<Exercise> exercises)
        {
            GroupedExercises.Clear();
            FilteredExercises.Clear();

            if (exercises.Count == 0)
            {
                ShowEmptyState = true;
                EmptyStateMessage = "Noch keine Übungen vorhanden";
                return;
            }

            ShowEmptyState = false;

            // Group by muscle group
            var groupedData = exercises
                .GroupBy(e => e.MuscleGroup)
                .OrderBy(g => MuscleGroups.All.IndexOf(g.Key))
                .ToList();

            foreach (var group in groupedData)
            {
                var exerciseGroup = new ExerciseGroup(
                    group.Key,
                    group.OrderBy(e => e.Name).ToList()
                );
                GroupedExercises.Add(exerciseGroup);
            }

            // Batch-populate FilteredExercises
            var sorted = exercises.OrderBy(e => e.Name).ToList();
            foreach (var exercise in sorted)
            {
                FilteredExercises.Add(exercise);
            }
        }

        private void UpdateFlatView(List<Exercise> exercises)
        {
            GroupedExercises.Clear();
            FilteredExercises.Clear();

            if (exercises.Count == 0)
            {
                ShowEmptyState = true;
                if (!string.IsNullOrWhiteSpace(SearchText))
                    EmptyStateMessage = $"Keine Übungen für '{SearchText}' gefunden";
                else if (SelectedMuscleGroup != "Alle")
                    EmptyStateMessage = $"Noch keine {SelectedMuscleGroup}-Übungen";
                else
                    EmptyStateMessage = "Noch keine Übungen vorhanden";
                return;
            }

            ShowEmptyState = false;

            // Batch-populate
            var sorted = exercises.OrderBy(e => e.Name).ToList();
            foreach (var exercise in sorted)
            {
                FilteredExercises.Add(exercise);
            }
        }

        public async Task DeleteExerciseAsync(Exercise exercise)
        {
            if (exercise == null || IsBusy)
                return;

            try
            {
                IsBusy = true;
                await _databaseService.DeleteExerciseAsync(exercise);
                _allExercises.Remove(exercise);
                ApplyFilters();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
