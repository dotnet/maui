using System;

namespace Xamarin.Forms
{
	[Xaml.TypeConversion(typeof(LinearItemsLayout))]
	public class CarouselLayoutTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value == "HorizontalList")
			{
				return LinearItemsLayout.CarouselDefault;
			}

			if (value == "VerticalList")
			{
				return LinearItemsLayout.CarouselVertical;
			}

			throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(IItemsLayout)}");
		}
	}
}