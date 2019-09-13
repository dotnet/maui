using System;

namespace Xamarin.Forms
{
	[Xaml.TypeConversion(typeof(IItemsLayout))]
	public class ItemsLayoutTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value == "HorizontalList")
			{
				return LinearItemsLayout.Horizontal;
			}

			if (value == "VerticalList")
			{
				return LinearItemsLayout.Vertical;
			}

			throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(IItemsLayout)}");
		}
	}
}