using System;
using System.Globalization;

namespace Xamarin.Forms.Controls
{
	public class ThicknessConverter : IValueConverter
	{
		ThicknessTypeConverter _converter = new ThicknessTypeConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Thickness thickness)
				return $"{thickness.Left}, {thickness.Top}, {thickness.Right}, {thickness.Bottom}";

			return "0, 0, 0, 0";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Thickness thickness)
				return thickness;

			try
			{
				if (value is string str)
					return _converter.ConvertFromInvariantString(str);
			}
			catch
			{
				// no-op
			}

			return default(Thickness);
		}
	}
}
