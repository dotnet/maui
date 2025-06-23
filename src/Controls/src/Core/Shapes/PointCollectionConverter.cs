#nullable disable
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Converters;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PointCollectionConverter.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.PointCollectionConverter']/Docs/*" />
	public class PointCollectionConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string) || sourceType == typeof(Point[]);

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string);

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is Point[] pointArray)
			{
				return (PointCollection)pointArray;
			}

			var strValue = value?.ToString();
			string[] points = strValue.Split(new char[] { ' ', ',' });
			var pointCollection = new PointCollection();
			double x = 0;
			bool hasX = false;

			foreach (string point in points)
			{
				if (string.IsNullOrWhiteSpace(point))
					continue;

				if (double.TryParse(point, NumberStyles.Number, CultureInfo.InvariantCulture, out double number))
				{
					if (!hasX)
					{
						x = number;
						hasX = true;
					}
					else
					{
						pointCollection.Add(new Point(x, number));
						hasX = false;
					}
				}
				else
					throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", point, typeof(double)));
			}

			if (hasX)
				throw new InvalidOperationException(string.Format("Cannot convert string into PointCollection"));

			return pointCollection;
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not PointCollection pc)
				throw new NotSupportedException();

			var converter = new PointTypeConverter();
			return string.Join(", ", pc.Select(p => converter.ConvertToInvariantString(p)));
		}
	}
}
