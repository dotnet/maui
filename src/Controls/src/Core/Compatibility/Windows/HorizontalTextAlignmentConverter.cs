#nullable disable
using System;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public partial class HorizontalTextAlignmentConverter : Microsoft.UI.Xaml.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var textAlign = (TextAlignment)value;
			return textAlign.ToPlatform();
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotSupportedException();
		}
	}
}