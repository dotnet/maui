using System;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	internal interface IWrappedValue
	{
		object Value { get; }
		Type Type { get; }
	}

	internal sealed class WrappedValueValueConverter : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is IWrappedValue wrappedValue && targetType == wrappedValue.Type)
				return wrappedValue.Value;

			return null;
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
			=> null;
	}
}
