using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Graphics.Converters
{
	public class SizeTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string)
				|| destinationType == typeof(SizeF)
#if IOS || MACCATALYST
				|| destinationType == typeof(CoreGraphics.CGSize)
				|| destinationType == typeof(CoreGraphics.CGPoint)
#endif
				;

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (Size.TryParse(value?.ToString(), out var s))
				return s;

			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(Size)));
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (!(value is Size size))
				throw new NotSupportedException();

			if (destinationType == typeof(SizeF))
				return (SizeF)size;
#if IOS || MACCATALYST
			if (destinationType == typeof(CoreGraphics.CGSize))
				return (CoreGraphics.CGSize)size;
			if (destinationType == typeof(CoreGraphics.CGPoint))
				return (CoreGraphics.CGPoint)size;
#endif
			return $"{size.Width.ToString(CultureInfo.InvariantCulture)}, {size.Height.ToString(CultureInfo.InvariantCulture)}";
		}
	}
}
