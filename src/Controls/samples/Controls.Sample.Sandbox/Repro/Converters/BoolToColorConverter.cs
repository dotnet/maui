using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AllTheLists.Converters;

public class BoolToColorConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isEnabled = (bool)value;

        var allParams = ((string)parameter).Split((';'));

        return isEnabled ? Color.FromArgb(allParams[0]) : Color.FromArgb(allParams[1]);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        //throw new NotImplementedException();
        return null;
    }
}