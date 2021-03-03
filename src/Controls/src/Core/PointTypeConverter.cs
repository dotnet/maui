using System;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	[Xaml.TypeConversion(typeof(Point))]
	public class PointTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
			{
				string[] xy = value.Split(',');
				if (xy.Length == 2 && double.TryParse(xy[0], NumberStyles.Number, CultureInfo.InvariantCulture, out var x) && double.TryParse(xy[1], NumberStyles.Number, CultureInfo.InvariantCulture, out var y))
					return new Point(x, y);
			}

			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(Point)));
		}

		public override string ConvertToInvariantString(object value)
		{
			if (!(value is Point p))
				throw new NotSupportedException();
			return $"{p.X.ToString(CultureInfo.InvariantCulture)}, {p.Y.ToString(CultureInfo.InvariantCulture)}";
		}
	}
}