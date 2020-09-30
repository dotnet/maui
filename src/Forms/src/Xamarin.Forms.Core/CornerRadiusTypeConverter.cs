using System;
using System.Globalization;

namespace Xamarin.Forms
{
	[Xaml.TypeConversion(typeof(CornerRadius))]
	public class CornerRadiusTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
			{
				value = value.Trim();
				if (value.Contains(","))
				{ //Xaml
					var cornerRadius = value.Split(',');
					if (cornerRadius.Length == 4
						&& double.TryParse(cornerRadius[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double tl)
						&& double.TryParse(cornerRadius[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double tr)
						&& double.TryParse(cornerRadius[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double bl)
						&& double.TryParse(cornerRadius[3], NumberStyles.Number, CultureInfo.InvariantCulture, out double br))
						return new CornerRadius(tl, tr, bl, br);
					if (cornerRadius.Length > 1
						&& cornerRadius.Length < 4
						&& double.TryParse(cornerRadius[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double l))
						return new CornerRadius(l);
				}
				else if (value.Trim().Contains(" "))
				{ //CSS
					var cornerRadius = value.Split(' ');
					if (cornerRadius.Length == 2
						&& double.TryParse(cornerRadius[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double t)
						&& double.TryParse(cornerRadius[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double b))
						return new CornerRadius(t, b, b, t);
					if (cornerRadius.Length == 3
						&& double.TryParse(cornerRadius[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double tl)
						&& double.TryParse(cornerRadius[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double trbl)
						&& double.TryParse(cornerRadius[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double br))
						return new CornerRadius(tl, trbl, trbl, br);
					if (cornerRadius.Length == 4
						&& double.TryParse(cornerRadius[0], NumberStyles.Number, CultureInfo.InvariantCulture, out tl)
						&& double.TryParse(cornerRadius[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double tr)
						&& double.TryParse(cornerRadius[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double bl)
						&& double.TryParse(cornerRadius[3], NumberStyles.Number, CultureInfo.InvariantCulture, out br))
						return new CornerRadius(tl, tr, bl, br);
				}
				else
				{ //single uniform CornerRadius
					if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out double l))
						return new CornerRadius(l);
				}
			}

			throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(CornerRadius)}");
		}
	}
}