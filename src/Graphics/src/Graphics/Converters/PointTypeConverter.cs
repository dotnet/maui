using System;
using System.ComponentModel;
using System.Globalization;
using System.Numerics;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Graphics.Converters
{
	public class PointTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string) || sourceType == typeof(Vector2);

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string)
				|| destinationType == typeof(PointF)
#if ANDROID
				|| destinationType == typeof(global::Android.Graphics.Point)
				|| destinationType == typeof(global::Android.Graphics.PointF)
#elif IOS || MACCATALYST
				|| destinationType == typeof(CoreGraphics.CGSize)
				|| destinationType == typeof(CoreGraphics.CGPoint)
#endif
				;

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is Vector2 vec)
				return (Point)vec;

			if (Point.TryParse(value?.ToString(), out var p))
				return p;

			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(Point)));
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (!(value is Point p))
				throw new NotSupportedException();

			if (destinationType == typeof(PointF))
				return (PointF)p;
#if ANDROID
			if (destinationType == typeof(global::Android.Graphics.Point))
				return (global::Android.Graphics.Point)p;
			if (destinationType == typeof(global::Android.Graphics.PointF))
				return (global::Android.Graphics.PointF)p;
#elif IOS || MACCATALYST
			if (destinationType == typeof(CoreGraphics.CGSize))
				return (CoreGraphics.CGSize)p;
			if (destinationType == typeof(CoreGraphics.CGPoint))
				return (CoreGraphics.CGPoint)p;
#endif
			return $"{p.X.ToString(CultureInfo.InvariantCulture)}, {p.Y.ToString(CultureInfo.InvariantCulture)}";
		}
	}
}
