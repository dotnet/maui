#nullable disable
using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/CarouselLayoutTypeConverter.xml" path="Type[@FullName='Microsoft.Maui.Controls.CarouselLayoutTypeConverter']/Docs/*" />
	public class CarouselLayoutTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string);

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();

			if (strValue == "HorizontalList")

/* Unmerged change from project 'Controls.Core(net8.0)'
Before:
				return LinearItemsLayout.CarouselDefault;
After:
			{
				return LinearItemsLayout.CarouselDefault;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-maccatalyst)'
Before:
				return LinearItemsLayout.CarouselDefault;
After:
			{
				return LinearItemsLayout.CarouselDefault;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-android)'
Before:
				return LinearItemsLayout.CarouselDefault;
After:
			{
				return LinearItemsLayout.CarouselDefault;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.19041.0)'
Before:
				return LinearItemsLayout.CarouselDefault;
After:
			{
				return LinearItemsLayout.CarouselDefault;
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.20348.0)'
Before:
				return LinearItemsLayout.CarouselDefault;
After:
			{
				return LinearItemsLayout.CarouselDefault;
			}
*/
			{
			{
				return LinearItemsLayout.CarouselDefault;

/* Unmerged change from project 'Controls.Core(net8.0)'
Added:
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-maccatalyst)'
Added:
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-android)'
Added:
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.19041.0)'
Added:
			}
*/

/* Unmerged change from project 'Controls.Core(net8.0-windows10.0.20348.0)'
Added:
			}
*/
			}

			if (strValue == "VerticalList")
			{
				return LinearItemsLayout.CarouselVertical;
			}

			throw new InvalidOperationException($"Cannot convert \"{strValue}\" into {typeof(LinearItemsLayout)}");
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not LinearItemsLayout lil)
			{
				throw new NotSupportedException();
			}

			if (lil == LinearItemsLayout.CarouselDefault)
			{
				return "HorizontalList";
			}

			if (lil == LinearItemsLayout.CarouselVertical)
			{
				return "VerticalList";
			}

			throw new NotSupportedException();
		}
	}
}
