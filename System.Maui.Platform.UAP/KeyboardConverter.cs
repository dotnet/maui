using System;

namespace System.Maui.Platform.UWP
{
	public class KeyboardConverter : global::Windows.UI.Xaml.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var keyboard = value as Keyboard;
			if (keyboard == null)
				return null;

			return keyboard.ToInputScope();
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}