using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Microsoft.Maui.Graphics
{
	public class ColorTypeConverter : TypeConverter
	{
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object fromValue)
		{
			return Color.Parse(fromValue?.ToString());
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (!(value is Color color) || color == null)
				throw new NotSupportedException();

			return color.ToHex();
		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string);
	}
}
