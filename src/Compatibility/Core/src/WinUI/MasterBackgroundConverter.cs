using System;
using Windows.UI;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class MasterBackgroundConverter : Microsoft.UI.Xaml.Data.IValueConverter
	{
		// Obtained by comparing the Mail apps master section background to the detail background
		const double Shift = 0.03;

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			WSolidColorBrush brush = null;

			var element = value as FrameworkElement;
			if (element != null)
			{
				while (brush == null && element != null)
				{
					DependencyProperty property = GetBackgroundProperty(element);
					if (property != null)
					{
						value = element.GetValue(property);
						brush = value as WSolidColorBrush;
						if (brush != null && brush.Color == Colors.Transparent)
							brush = null;
					}

					element = VisualTreeHelper.GetParent(element) as FrameworkElement;
				}
			}

			brush = value as WSolidColorBrush;
			if (brush != null)
			{
				Maui.Graphics.Color color = brush.ToColor();

				double delta = Shift;
				if (color.GetLuminosity() > .6)
					delta = -Shift;

				color = color.AddLuminosity((float)delta);

				return new WSolidColorBrush(color.ToWindowsColor());
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