using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using RealTalk_AI.Resources;

namespace RealTalk_AI.Utils
{
    public class LocalizationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string key)
            {
                return LocalizationResourceManager.Instance[key];
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
