#nullable disable
using System;
using System.Globalization;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public partial class CaseConverter : Microsoft.UI.Xaml.Data.IValueConverter
	{
		public bool ConvertToUpper { get; set; }

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (value == null)
				return null;

			var v = (string)value;
			return ConvertToUpper ? v.ToUpper(CultureInfo.CurrentCulture) : v.ToLower(CultureInfo.CurrentCulture);
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotSupportedException();
		}
	}
}