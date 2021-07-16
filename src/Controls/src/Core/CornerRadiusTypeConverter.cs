using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	[Xaml.TypeConversion(typeof(CornerRadius))]
	public class CornerRadiusTypeConverter : StringTypeConverterBase
	{
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();
			if (strValue != null)
			{
				value = strValue.Trim();
				if (strValue.Contains(","))
				{ //Xaml
					var cornerRadius = strValue.Split(',');
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
				else if (strValue.Trim().Contains(" "))
				{ //CSS
					var cornerRadius = strValue.Split(' ');
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
					if (double.TryParse(strValue, NumberStyles.Number, CultureInfo.InvariantCulture, out double l))
						return new CornerRadius(l);
				}
			}

			throw new InvalidOperationException($"Cannot convert \"{strValue}\" into {typeof(CornerRadius)}");
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not CornerRadius cr)
				throw new NotSupportedException();
			return $"{cr.TopLeft.ToString(CultureInfo.InvariantCulture)}, {cr.TopRight.ToString(CultureInfo.InvariantCulture)}, {cr.BottomLeft.ToString(CultureInfo.InvariantCulture)}, {cr.BottomRight.ToString(CultureInfo.InvariantCulture)}";

		}
	}
}