using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/CarouselLayoutTypeConverter.xml" path="Type[@FullName='Microsoft.Maui.Controls.CarouselLayoutTypeConverter']/Docs" />
	public class CarouselLayoutTypeConverter : TypeConverter
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/CarouselLayoutTypeConverter.xml" path="//Member[@MemberName='CanConvertFrom']/Docs" />
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		/// <include file="../../../docs/Microsoft.Maui.Controls/CarouselLayoutTypeConverter.xml" path="//Member[@MemberName='CanConvertTo']/Docs" />
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string);

		/// <include file="../../../docs/Microsoft.Maui.Controls/CarouselLayoutTypeConverter.xml" path="//Member[@MemberName='ConvertFrom']/Docs" />
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();

			if (strValue == "HorizontalList")
				return LinearItemsLayout.CarouselDefault;

			if (strValue == "VerticalList")
				return LinearItemsLayout.CarouselVertical;

			throw new InvalidOperationException($"Cannot convert \"{strValue}\" into {typeof(LinearItemsLayout)}");
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/CarouselLayoutTypeConverter.xml" path="//Member[@MemberName='ConvertTo']/Docs" />
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not LinearItemsLayout lil)
				throw new NotSupportedException();

			if (lil == LinearItemsLayout.CarouselDefault)
				return "HorizontalList";

			if (lil == LinearItemsLayout.CarouselVertical)
				return "VerticalList";

			throw new NotSupportedException();
		}
	}
}