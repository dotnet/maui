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
				double l, t, r, b;
				string[] thickness = value.Split(',');
				switch (thickness.Length)
				{
					case 1:
						if (double.TryParse(thickness[0], NumberStyles.Number, CultureInfo.InvariantCulture, out l))
							return new Thickness(l);
						break;
					case 2:
						if (double.TryParse(thickness[0], NumberStyles.Number, CultureInfo.InvariantCulture, out l) && double.TryParse(thickness[1], NumberStyles.Number, CultureInfo.InvariantCulture, out t))
							return new Thickness(l, t);
						break;
					case 4:
						if (double.TryParse(thickness[0], NumberStyles.Number, CultureInfo.InvariantCulture, out l) && double.TryParse(thickness[1], NumberStyles.Number, CultureInfo.InvariantCulture, out t) &&
							double.TryParse(thickness[2], NumberStyles.Number, CultureInfo.InvariantCulture, out r) && double.TryParse(thickness[3], NumberStyles.Number, CultureInfo.InvariantCulture, out b))
							return new Thickness(l, t, r, b);
						break;
				}
			}

			throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(Thickness)}");
		}
	}
}