using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls.Design
{
	public class BoundsDesignTypeConverter : StringConverter
	{
		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			// MUST MATCH BoundsTypeConverter.ConvertFrom
			string strValue = value?.ToString();
			if (string.IsNullOrEmpty(strValue))
				return false;

			string[] xywh = strValue.Split(',');
			bool hasX, hasY, hasW, hasH;

			hasX = (xywh.Length == 2 || xywh.Length == 4) && double.TryParse(xywh[0], NumberStyles.Number, CultureInfo.InvariantCulture, out _);
			hasY = (xywh.Length == 2 || xywh.Length == 4) && double.TryParse(xywh[1], NumberStyles.Number, CultureInfo.InvariantCulture, out _);
			hasW = xywh.Length == 4 && double.TryParse(xywh[2], NumberStyles.Number, CultureInfo.InvariantCulture, out _);
			hasH = xywh.Length == 4 && double.TryParse(xywh[3], NumberStyles.Number, CultureInfo.InvariantCulture, out _);
			if (!hasW && xywh.Length == 4 && string.Compare("AutoSize", xywh[2].Trim(), StringComparison.OrdinalIgnoreCase) == 0)
			{
				hasW = true;
			}

			if (!hasH && xywh.Length == 4 && string.Compare("AutoSize", xywh[3].Trim(), StringComparison.OrdinalIgnoreCase) == 0)
			{
				hasH = true;
			}

			if (hasX && hasY && xywh.Length == 2)
				return true;
			if (hasX && hasY && hasW && hasH && xywh.Length == 4)
				return true;

			return false;
		}
	}
}
