using System;

namespace Microsoft.Maui.Controls
{
	[Xaml.TypeConversion(typeof(LinearItemsLayout))]
	public class CarouselLayoutTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value == "HorizontalList")
				return LinearItemsLayout.CarouselDefault;

			if (value == "VerticalList")
				return LinearItemsLayout.CarouselVertical;

			throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(LinearItemsLayout)}");
		}

		public override string ConvertToInvariantString(object value)
		{
			if (!(value is LinearItemsLayout lil))
				throw new NotSupportedException();

			if (lil == LinearItemsLayout.CarouselDefault)
				return "HorizontalList";

			if (lil == LinearItemsLayout.CarouselVertical)
				return "VerticalList";

			throw new NotSupportedException();
		}
	}
}