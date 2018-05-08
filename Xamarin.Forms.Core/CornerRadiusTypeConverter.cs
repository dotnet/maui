using System;
using System.Globalization;

namespace Xamarin.Forms
{
	[Xaml.TypeConversion (typeof (CornerRadius))]
	public class CornerRadiusTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString (string value)
		{
			if (value != null) {
				value = value.Trim ();
				if (value.Contains (",")) { //Xaml
					var cornerRadius = value.Split(',');
					switch (cornerRadius.Length) {
						case 4:
							if (double.TryParse (cornerRadius[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double topLeft)
								&& double.TryParse (cornerRadius[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double topRight)
								&& double.TryParse (cornerRadius[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double bottomLeft)
								&& double.TryParse (cornerRadius[3], NumberStyles.Number, CultureInfo.InvariantCulture, out double bottomRight))
								return new CornerRadius (topLeft, topRight, bottomLeft, bottomRight);
							break;
					}
				} else if (value.Contains (" ")) { //CSS
					var cornerRadius = value.Split(' ');
					switch (cornerRadius.Length) {
						case 2:
							if (double.TryParse (cornerRadius[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double t)
								&& double.TryParse (cornerRadius[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double b))
								return new CornerRadius (t, b, b, t);
							break;
						case 3:
							if (double.TryParse (cornerRadius[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double tl)
								&& double.TryParse (cornerRadius[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double trbl)
								&& double.TryParse (cornerRadius[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double br))
								return new CornerRadius (tl, trbl, trbl, br);
							break;
						case 4:
							if (double.TryParse (cornerRadius[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double topLeft)
								&& double.TryParse (cornerRadius[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double topRight)
								&& double.TryParse (cornerRadius[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double bottomLeft)
								&& double.TryParse (cornerRadius[3], NumberStyles.Number, CultureInfo.InvariantCulture, out double bottomRight))
								return new CornerRadius (topLeft, topRight, bottomLeft, bottomRight);
							break;
					}
				} else { //single uniform CornerRadius
					if (double.TryParse (value, NumberStyles.Number, CultureInfo.InvariantCulture, out double l))
						return new CornerRadius (l);
				}
			}

			throw new InvalidOperationException ($"Cannot convert \"{value}\" into {typeof (CornerRadius)}");
		}
	}
}