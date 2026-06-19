using Gymaui_App.Utilities;
using System.Globalization;

namespace Gymaui_App.Converters
{
    public class ChipBackgroundConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string muscleGroup = value as string ?? string.Empty;
            string selectedMuscleGroup = parameter as string ?? "Alle";

            if (muscleGroup == selectedMuscleGroup)
                return Color.FromArgb("#E8FF47");

            return Color.FromArgb("#242424");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ChipTextColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string muscleGroup = value as string ?? string.Empty;
            string selectedMuscleGroup = parameter as string ?? "Alle";

            if (muscleGroup == selectedMuscleGroup)
                return Color.FromArgb("#000000");

            return Color.FromArgb("#8A8A8A");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ExerciseDetailsConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Models.Exercise exercise)
            {
                var parts = new List<string>();

                if (!string.IsNullOrEmpty(exercise.MuscleGroup))
                    parts.Add(exercise.MuscleGroup);

                return string.Join("  •  ", parts);
            }
            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CountConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is System.Collections.ICollection collection)
                return $"({collection.Count})";

            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MuscleGroupIconConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string muscleGroup = value as string ?? string.Empty;
            string fileName = MuscleGroups.GetIcon(muscleGroup);
            return ImageSource.FromFile(fileName);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MuscleGroupBorderConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Will be set dynamically in code-behind
            return Color.FromArgb("#2A2A2A");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MuscleGroupTextConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Will be set dynamically in code-behind
            return Color.FromArgb("#FFFFFF");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

