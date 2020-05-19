using System;
using global::Windows.UI.Xaml;

namespace System.Maui.Platform.UWP
{
	public sealed class BoolToVisibilityConverter : global::Windows.UI.Xaml.Data.IValueConverter
	{
		public bool FalseIsVisible { get; set; }

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var v = (bool)value;
			if (FalseIsVisible)
				v = !v;

			return v ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}