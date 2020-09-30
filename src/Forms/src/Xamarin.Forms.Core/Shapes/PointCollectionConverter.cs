using System;
using System.Globalization;

namespace Xamarin.Forms.Shapes
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
	}
}