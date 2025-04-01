#nullable disable
using System;
using WVisibility = Microsoft.UI.Xaml.Visibility;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public partial class CollapseWhenEmptyConverter : Microsoft.UI.Xaml.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			var length = 0;

			var s = value as string;
			if (s != null)
				length = s.Length;

			if (value is int)
				length = (int)value;

			return length > 0 ? WVisibility.Visible : WVisibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotSupportedException();
		}
	}
}