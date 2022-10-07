using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Gallery.SurfingApp
{
    public class ToUpperConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value.ToString();

            if (string.IsNullOrEmpty(str))
                return string.Empty;

            return str.ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
