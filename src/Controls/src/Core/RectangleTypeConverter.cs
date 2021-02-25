using System;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	[Xaml.ProvideCompiled("Microsoft.Maui.Controls.XamlC.RectangleTypeConverter")]
	[Xaml.TypeConversion(typeof(Rectangle))]
	public class RectangleTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
			{
				string[] xywh = value.Split(',');
				if (xywh.Length == 4
					&& double.TryParse(xywh[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double x)
					&& double.TryParse(xywh[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double y)
					&& double.TryParse(xywh[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double w)
					&& double.TryParse(xywh[3], NumberStyles.Number, CultureInfo.InvariantCulture, out double h))
					return new Rectangle(x, y, w, h);
			}

			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(Rectangle)));
		}

		public override string ConvertToInvariantString(object value)
		{
			if (!(value is Rectangle r))
				throw new NotSupportedException();
			return $"{r.X.ToString(CultureInfo.InvariantCulture)}, {r.Y.ToString(CultureInfo.InvariantCulture)}, {r.Width.ToString(CultureInfo.InvariantCulture)}, {r.Height.ToString(CultureInfo.InvariantCulture)}";
		}
	}
}