using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Platform.WPF
{
	public sealed class ViewToRendererConverter : System.Windows.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			VisualElement visualElement = value as VisualElement;
			if (visualElement != null)
			{
				var frameworkElement = Platform.GetOrCreateRenderer(visualElement)?.GetNativeElement();

				if(frameworkElement != null)
				{
					frameworkElement.Loaded += (sender, args) =>
					{
						visualElement.Layout(new Rectangle(0, 0, frameworkElement.ActualWidth, frameworkElement.ActualHeight));
					};

					frameworkElement.SizeChanged += (sender, args) =>
					{
						visualElement.Layout(new Rectangle(0, 0, frameworkElement.ActualWidth, frameworkElement.ActualHeight));
					};

					return frameworkElement;
				}
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
