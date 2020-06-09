using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Xamarin.Forms.Platform.WPF.Controls;
using Xamarin.Forms.Platform.WPF.Enums;
using IOPath = System.IO.Path;
using WGeometry = System.Windows.Media.Geometry;

namespace Xamarin.Forms.Platform.WPF.Converters
{
	public class IconConveter : System.Windows.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is FileImageSource imageSource)
			{
				if (Enum.TryParse(imageSource.File, true, out Symbol symbol))
					return new FormsSymbolIcon() { Symbol = symbol };
				else if (TryParseGeometry(imageSource.File, out WGeometry geometry))
					return new FormsPathIcon() { Data = geometry };
				else if (IOPath.GetExtension(imageSource.File) != null)
					return new FormsBitmapIcon() { UriSource = new Uri(imageSource.File, UriKind.RelativeOrAbsolute) };
			}
			else if (value is FontImageSource fontsource)
			{
				return new FormsFontIcon() { Source = fontsource };
			}

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		private bool TryParseGeometry(string value, out WGeometry geometry)
		{
			geometry = null;
			try
			{
				geometry = WGeometry.Parse(value);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
