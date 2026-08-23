#nullable enable
using System;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Controls.Platform
{
	internal sealed partial class NullToUnsetValueConverter : Microsoft.UI.Xaml.Data.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language) =>
			value ?? DependencyProperty.UnsetValue;

		public object ConvertBack(object value, Type targetType, object parameter, string language) =>
			throw new NotSupportedException();
	}
}
