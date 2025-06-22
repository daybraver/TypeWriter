using System.Globalization;
using System.Windows.Data;

namespace TypeWriter.Converters
{
    public class Accent2BoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var value2 = (Accent)value;
            var param2 = (Accent)parameter;
            return value2 == param2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
