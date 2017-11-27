using System;

namespace Xamarin.Forms.Platform.UWP
{
	public class KeyboardConverter : Windows.UI.Xaml.Data.IValueConverter
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