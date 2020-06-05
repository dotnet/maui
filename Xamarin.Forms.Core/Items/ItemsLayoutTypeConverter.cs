using System;
using System.Globalization;

namespace Xamarin.Forms
{
	[Xaml.TypeConversion(typeof(IItemsLayout))]
	public class ItemsLayoutTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			ItemsLayoutOrientation? orientation = default(ItemsLayoutOrientation?);
			int identifierLength = 0;

			if (value == "VerticalList")
			{
				return LinearItemsLayout.Vertical;
			}
			else if (value == "HorizontalList")
			{
				return LinearItemsLayout.Horizontal;
			}
			else if (value.StartsWith("VerticalGrid", StringComparison.Ordinal))
			{
				orientation = ItemsLayoutOrientation.Vertical;
				identifierLength = "VerticalGrid".Length;
			}
			else if (value.StartsWith("HorizontalGrid", StringComparison.Ordinal))
			{
				orientation = ItemsLayoutOrientation.Horizontal;
				identifierLength = "HorizontalGrid".Length;
			}

			if (orientation.HasValue)
			{
				if (value.Length == identifierLength)
				{
					return new GridItemsLayout(orientation.Value);
				}
				else if (value.Length > identifierLength + 1 && value[identifierLength] == ',')
				{
					var argument = value.Substring(identifierLength + 1);
					var span = int.Parse(argument, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture);
					return new GridItemsLayout(span, orientation.Value);
				}
			}

			throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(IItemsLayout)}");
		}
	}
}