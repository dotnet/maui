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

namespace Microsoft.Maui.Controls.ControlGallery
{
	public class GenericValueConverter : IValueConverter
	{
		Func<object, object> _convert;
		Func<object, object> _back;
		public GenericValueConverter(Func<object, object> convert, Func<object, object> back = null)
		{
			_convert = convert;
			_back = back;
		}

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return _convert(value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return _back(value);
		}
	}
}