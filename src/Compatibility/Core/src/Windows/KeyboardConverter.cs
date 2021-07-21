using System;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class KeyboardConverter : Microsoft.UI.Xaml.Data.IValueConverter
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