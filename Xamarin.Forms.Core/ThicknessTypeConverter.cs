using System;
using System.Globalization;

namespace Xamarin.Forms
{
	[Xaml.ProvideCompiled("Xamarin.Forms.Core.XamlC.ThicknessTypeConverter")]
	[Xaml.TypeConversion(typeof(Thickness))]
	public class ThicknessTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
			{
				value = value.Trim();
				if (value.Contains(","))
				{ //Xaml
					var thickness = value.Split(',');
					switch (thickness.Length)
					{
						case 2:
							if (double.TryParse(thickness[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double h)
								&& double.TryParse(thickness[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double v))
								return new Thickness(h, v);
							break;
						case 4:
							if (double.TryParse(thickness[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double l)
								&& double.TryParse(thickness[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double t)
								&& double.TryParse(thickness[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double r)
								&& double.TryParse(thickness[3], NumberStyles.Number, CultureInfo.InvariantCulture, out double b))
								return new Thickness(l, t, r, b);
							break;
					}
				}
				else if (value.Contains(" "))
				{ //CSS
					var thickness = value.Split(' ');
					switch (thickness.Length)
					{
						case 2:
							if (double.TryParse(thickness[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double v)
								&& double.TryParse(thickness[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double h))
								return new Thickness(h, v);
							break;
						case 3:
							if (double.TryParse(thickness[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double t)
								&& double.TryParse(thickness[1], NumberStyles.Number, CultureInfo.InvariantCulture, out h)
								&& double.TryParse(thickness[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double b))
								return new Thickness(h, t, h, b);
							break;
						case 4:
							if (double.TryParse(thickness[0], NumberStyles.Number, CultureInfo.InvariantCulture, out t)
								&& double.TryParse(thickness[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double r)
								&& double.TryParse(thickness[2], NumberStyles.Number, CultureInfo.InvariantCulture, out b)
								&& double.TryParse(thickness[3], NumberStyles.Number, CultureInfo.InvariantCulture, out double l))
								return new Thickness(l, t, r, b);
							break;
					}
				}
				else
				{ //single uniform thickness
					if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out double l))
						return new Thickness(l);
				}
			}

			throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(Thickness)}");
		}
	}
}