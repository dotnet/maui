using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	public class TextAlignmentConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string);

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();
			if (strValue != null)
			{
				if (strValue.Equals("Start", StringComparison.OrdinalIgnoreCase) || strValue.Equals("left", StringComparison.OrdinalIgnoreCase))
					return TextAlignment.Start;
				if (strValue.Equals("top", StringComparison.OrdinalIgnoreCase))
					return TextAlignment.Start;
				if (strValue.Equals("right", StringComparison.OrdinalIgnoreCase))
					return TextAlignment.End;
				if (strValue.Equals("bottom", StringComparison.OrdinalIgnoreCase))
					return TextAlignment.End;
				if (strValue.Equals("center", StringComparison.OrdinalIgnoreCase))
					return TextAlignment.Center;
				if (strValue.Equals("middle", StringComparison.OrdinalIgnoreCase))
					return TextAlignment.Center;
				if (strValue.Equals("End", StringComparison.OrdinalIgnoreCase) || strValue.Equals("right", StringComparison.OrdinalIgnoreCase))
					return TextAlignment.End;
				if (strValue.Equals("Center", StringComparison.OrdinalIgnoreCase) || strValue.Equals("center", StringComparison.OrdinalIgnoreCase))
					return TextAlignment.Center;

				if (Enum.TryParse(strValue, out TextAlignment direction))
					return direction;
			}
			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", strValue, typeof(TextAlignment)));
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not TextAlignment ta)
				throw new NotSupportedException();
			if (ta == TextAlignment.Start)
				return nameof(TextAlignment.Start);
			if (ta == TextAlignment.Center)
				return nameof(TextAlignment.Center);
			if (ta == TextAlignment.End)
				return nameof(TextAlignment.End);
			throw new NotSupportedException();
		}
	}
}
