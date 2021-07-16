using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	public class PointCollectionConverter : StringTypeConverterBase
	{
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
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