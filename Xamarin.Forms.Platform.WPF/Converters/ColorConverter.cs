using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Xamarin.Forms.Platform.WPF
{
	public sealed class ColorConverter : System.Windows.Data.IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			FrameworkElement framework = values[0] as FrameworkElement;
			DependencyProperty dp = parameter as DependencyProperty;

			if (values.Count() > 1 && framework != null && values[1] is Color && dp != null)
			{
				return framework.UpdateDependencyColor(dp, (Color)values[1]);
			}
			return Color.Transparent.ToBrush();
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
