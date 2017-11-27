using System;
using Windows.UI.Xaml;

namespace Xamarin.Forms.Platform.UWP
{
	public class CollapseWhenEmptyConverter : Windows.UI.Xaml.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var length = 0;

			var s = value as string;
			if (s != null)
				length = s.Length;

			if (value is int)
				length = (int)value;

			return length > 0 ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotSupportedException();
		}
	}
}