using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Class that the XAML parser uses to convert strings to <see cref="Accelerator" /> objects.
	/// </summary>
	/// <remarks>
	/// The given string value may contain a combination of "CTRL", "CMD", "ALT", "SHIFT", "FN", or "WIN",
	/// in any combination of upper or lower case letters, as well as any available keys on the platform.
	/// The returned <see cref="Accelerator" /> has its <see cref="Accelerator.Modifiers" /> array filled with the specified modifiers,
	/// and its <see cref="Accelerator.Keys" /> array filled with the remaining keys.</remarks>
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

			return Accelerator.FromString(strValue);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not Accelerator acc)
				throw new NotSupportedException();

			return acc.ToString();
		}
	}
}
