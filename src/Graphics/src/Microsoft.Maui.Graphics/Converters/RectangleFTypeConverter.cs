using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Graphics.Converters
{
	public class RectangleFTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string);

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (RectangleF.TryParse(value?.ToString(), out var r))
				return r;

			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(RectangleF)));
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (!(value is RectangleF r))
				throw new NotSupportedException();
			return $"{r.X.ToString(CultureInfo.InvariantCulture)}, {r.Y.ToString(CultureInfo.InvariantCulture)}, {r.Width.ToString(CultureInfo.InvariantCulture)}, {r.Height.ToString(CultureInfo.InvariantCulture)}";
		}
	}
}
