using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls.Shapes
{
	public class StrokeShapeTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			   => sourceType == typeof(string);

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();
			string[] corners = strValue.Split(new char[] { ' ', ',' });

			if (corners.Length > 0)
			{
				var cornerRadius = strValue.Split(',');
				if (cornerRadius.Length == 4
					&& double.TryParse(cornerRadius[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double tl)
					&& double.TryParse(cornerRadius[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double tr)
					&& double.TryParse(cornerRadius[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double bl)
					&& double.TryParse(cornerRadius[3], NumberStyles.Number, CultureInfo.InvariantCulture, out double br))
					return new RoundRectangle { CornerRadius = new CornerRadius(tl, tr, bl, br) };
				if (cornerRadius.Length > 1
					&& cornerRadius.Length < 4
					&& double.TryParse(cornerRadius[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double l))
					return new RoundRectangle { CornerRadius = new CornerRadius(l) };
			}

			throw new InvalidOperationException($"Cannot convert \"{strValue}\" into {typeof(RoundRectangle)}");
		}
	}
}