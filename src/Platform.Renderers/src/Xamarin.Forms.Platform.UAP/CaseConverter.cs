using System;

namespace Xamarin.Forms.Platform.UWP
{
	public class CaseConverter : Windows.UI.Xaml.Data.IValueConverter
	{
		public bool ConvertToUpper { get; set; }

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value == null)
				return null;

			var v = (string)value;
			return ConvertToUpper ? v.ToUpper() : v.ToLower();
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotSupportedException();
		}
	}
}