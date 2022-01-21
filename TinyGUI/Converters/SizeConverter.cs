using System;
using System.Globalization;
using System.Windows.Data;

namespace TinyGUI.Converters
{
    public class SizeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is string name && values[1] is long oriFileSize)
            {
                return $"{name} ({ProgressConverter.Converter(oriFileSize * 1.0)})";
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}