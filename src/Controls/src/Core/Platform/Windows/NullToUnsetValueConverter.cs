#nullable enable
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Microsoft.Maui.Controls.Platform
{
	internal sealed partial class NullToUnsetValueConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language) =>
			value ?? DependencyProperty.UnsetValue;

		public object ConvertBack(object value, Type targetType, object parameter, string language) =>
			throw new NotSupportedException();
	}
}
