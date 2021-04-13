using System;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	public class PointCollectionConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			string[] points = value.Split(new char[] { ' ', ',' });
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

		public override string ConvertToInvariantString(object value)
		{
			if (!(value is PointCollection pc))
				throw new NotSupportedException();

			var converter = new PointTypeConverter();
			return string.Join(", ", pc.Select(p => converter.ConvertToInvariantString(p)));
		}
	}
}