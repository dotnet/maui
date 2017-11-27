using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Xamarin.Forms.Platform.UWP
{
	public class MasterBackgroundConverter : Windows.UI.Xaml.Data.IValueConverter
	{
		// Obtained by comparing the Mail apps master section background to the detail background
		const double Shift = 0.03;

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			SolidColorBrush brush = null;

			var element = value as FrameworkElement;
			if (element != null)
			{
				while (brush == null && element != null)
				{
					DependencyProperty property = GetBackgroundProperty(element);
					if (property != null)
					{
						value = element.GetValue(property);
						brush = value as SolidColorBrush;
						if (brush != null && brush.Color == Colors.Transparent)
							brush = null;
					}

					element = VisualTreeHelper.GetParent(element) as FrameworkElement;
				}
			}

			brush = value as SolidColorBrush;
			if (brush != null)
			{
				Windows.UI.Color wcolor = brush.Color;
				Color color = Color.FromRgba(wcolor.R, wcolor.G, wcolor.B, wcolor.A);

				double delta = Shift;
				if (color.Luminosity > .6)
					delta = -Shift;

				color = color.AddLuminosity(delta);

				return new SolidColorBrush(color.ToWindowsColor());
			}

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}

		static DependencyProperty GetBackgroundProperty(FrameworkElement element)
		{
			if (element is Control)
				return Control.BackgroundProperty;
			if (element is Panel)
				return Panel.BackgroundProperty;

			return null;
		}
	}
}