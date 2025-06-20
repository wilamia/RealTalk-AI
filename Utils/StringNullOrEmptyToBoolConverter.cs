using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace RealTalk_AI.Utils
{
    public class StringNullOrEmptyToBoolConverter : IValueConverter
    {
        public bool Invert { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = !string.IsNullOrEmpty(value as string);

            if (parameter?.ToString() == "invert")
                result = !result;

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
