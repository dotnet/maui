#nullable disable
using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsLayoutTypeConverter.xml" path="Type[@FullName='Microsoft.Maui.Controls.ItemsLayoutTypeConverter']/Docs/*" />
	public class ItemsLayoutTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string);

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();
			if (strValue == null)
				throw new ArgumentNullException(nameof(strValue));

			ItemsLayoutOrientation? orientation = default(ItemsLayoutOrientation?);
			int identifierLength = 0;

			if (strValue == "VerticalList")
				return LinearItemsLayout.CreateVerticalDefault();
			else if (strValue == "HorizontalList")
				return LinearItemsLayout.CreateHorizontalDefault();
			else if (strValue.StartsWith("VerticalGrid", StringComparison.Ordinal))
			{
				orientation = ItemsLayoutOrientation.Vertical;
				identifierLength = "VerticalGrid".Length;
			}
			else if (strValue.StartsWith("HorizontalGrid", StringComparison.Ordinal))
			{
				orientation = ItemsLayoutOrientation.Horizontal;
				identifierLength = "HorizontalGrid".Length;
			}

			if (orientation.HasValue)
			{
				if (strValue.Length == identifierLength)
					return new GridItemsLayout(orientation.Value);
				else if (strValue.Length > identifierLength + 1 && strValue[identifierLength] == ',')
				{
					var argument = strValue.Substring(identifierLength + 1);
					var span = int.Parse(argument, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture);
					return new GridItemsLayout(span, orientation.Value);
				}
			}

			throw new InvalidOperationException($"Cannot convert \"{strValue}\" into {typeof(IItemsLayout)}");
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is LinearItemsLayout && value == LinearItemsLayout.Vertical)
				return "VerticalList";
			if (value is LinearItemsLayout && value == LinearItemsLayout.Horizontal)
				return "HorizontalList";
			if (value is GridItemsLayout gil)
				return $"{gil.Orientation}Grid,{gil.Span}";
			throw new NotSupportedException();
		}
	}
}
