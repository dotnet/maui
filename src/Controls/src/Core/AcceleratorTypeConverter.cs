#nullable disable
using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Class that the XAML parser uses to convert strings to <see cref="KeyboardAccelerator" /> objects.
	/// </summary>
	/// <remarks>
	/// The given string value may contain a combination of "CTRL", "CMD", "ALT", "SHIFT", "FN", or "WIN",
	/// in any combination of upper or lower case letters, as well as any available keys on the platform.
	/// The returned <see cref="KeyboardAccelerator" /> has its <see cref="KeyboardAccelerator.Modifiers" /> array filled with the specified modifiers,
	/// and its <see cref="KeyboardAccelerator.Key" /> string as the remaining key.</remarks>
	public class AcceleratorTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string);

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();

			if (string.IsNullOrEmpty(strValue))
				return null;

			return KeyboardAccelerator.FromString(strValue);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not KeyboardAccelerator acc)
				throw new NotSupportedException();

			return acc.ToString();
		}
	}
}
