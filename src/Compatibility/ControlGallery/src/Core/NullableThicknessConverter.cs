//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using System.Globalization;
using Microsoft.Maui.Converters;

namespace Microsoft.Maui.Controls.ControlGallery
{
	public class NullableThicknessConverter : IValueConverter
	{
		ThicknessTypeConverter _converter = new ThicknessTypeConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Thickness thickness)
				return $"{thickness.Left}, {thickness.Top}, {thickness.Right}, {thickness.Bottom}";

			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is null)
				return null;

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

			return null;
		}
	}
}
