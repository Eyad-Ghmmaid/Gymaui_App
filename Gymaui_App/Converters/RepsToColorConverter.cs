using System.Globalization;

namespace Gymaui_App.Converters
{
    // MultiValueConverter: values[0] = ActualReps (string), values[1] = TargetReps (int)
    public class RepsToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (values == null || values.Length < 2)
                    return Colors.Black;

                var actualObj = values[0];
                var targetObj = values[1];

                int target = 0;
                if (targetObj is int ti)
                    target = ti;
                else if (targetObj is string ts && int.TryParse(ts, out var t2))
                    target = t2;

                int actual = 0;
                if (actualObj is int ai)
                    actual = ai;
                else if (actualObj is string s && int.TryParse(s, out var a2))
                    actual = a2;

                if (actual <= 0)
                    return Colors.Black;

                if (actual >= target)
                    return Colors.Green;

                return Colors.Red;
            }
            catch
            {
                return Colors.Black;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
