using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Graphics.Converters
{
	public class RectFTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string)
#if ANDROID
				|| destinationType == typeof(global::Android.Graphics.Rect)
				|| destinationType == typeof(global::Android.Graphics.RectF)
#elif IOS || MACCATALYST
				|| destinationType == typeof(CoreGraphics.CGRect)
#endif
			;

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (RectF.TryParse(value?.ToString(), out var r))
				return r;

			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(RectF)));
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (!(value is RectF r))
				throw new NotSupportedException();

#if ANDROID
			if (destinationType == typeof(global::Android.Graphics.Rect))
				return (global::Android.Graphics.Rect)r;
			if (destinationType == typeof(global::Android.Graphics.RectF))
				return (global::Android.Graphics.RectF)r;
#elif IOS || MACCATALYST
			if (destinationType == typeof(CoreGraphics.CGRect))
				return (CoreGraphics.CGRect)r;
#endif

			return $"{r.X.ToString(CultureInfo.InvariantCulture)}, {r.Y.ToString(CultureInfo.InvariantCulture)}, {r.Width.ToString(CultureInfo.InvariantCulture)}, {r.Height.ToString(CultureInfo.InvariantCulture)}";
		}
	}
}
