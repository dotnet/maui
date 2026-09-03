using System;
using System.Globalization;

namespace Microsoft.Maui.Controls.Compatibility.Platform.WPF
{
	public sealed class ViewToRendererConverter : System.Windows.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var visualElement = value as VisualElement;
			if (visualElement == null)
			{
				return null;
			}

			var frameworkElement = Platform.GetOrCreateRenderer(visualElement)?.GetNativeElement();
			return frameworkElement;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
