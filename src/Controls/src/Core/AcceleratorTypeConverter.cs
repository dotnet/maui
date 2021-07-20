using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
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
