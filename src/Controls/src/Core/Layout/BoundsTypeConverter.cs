using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/BoundsTypeConverter.xml" path="Type[@FullName='Microsoft.Maui.Controls.BoundsTypeConverter']/Docs/*" />
	[Xaml.ProvideCompiled("Microsoft.Maui.Controls.XamlC.BoundsTypeConverter")]
	public sealed class BoundsTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string);

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();

			if (strValue != null)
			{
				double x = -1, y = -1, w = -1, h = -1;
				string[] xywh = strValue.Split(',');
				bool hasX, hasY, hasW, hasH;

				hasX = (xywh.Length == 2 || xywh.Length == 4) && double.TryParse(xywh[0], NumberStyles.Number, CultureInfo.InvariantCulture, out x);
				hasY = (xywh.Length == 2 || xywh.Length == 4) && double.TryParse(xywh[1], NumberStyles.Number, CultureInfo.InvariantCulture, out y);
				hasW = xywh.Length == 4 && double.TryParse(xywh[2], NumberStyles.Number, CultureInfo.InvariantCulture, out w);
				hasH = xywh.Length == 4 && double.TryParse(xywh[3], NumberStyles.Number, CultureInfo.InvariantCulture, out h);

				if (!hasW && xywh.Length == 4 && string.Compare("AutoSize", xywh[2].Trim(), StringComparison.OrdinalIgnoreCase) == 0)
				{
					hasW = true;
					w = AbsoluteLayout.AutoSize;
				}

				if (!hasH && xywh.Length == 4 && string.Compare("AutoSize", xywh[3].Trim(), StringComparison.OrdinalIgnoreCase) == 0)
				{
					hasH = true;
					h = AbsoluteLayout.AutoSize;
				}

				if (hasX && hasY && xywh.Length == 2)
					return new Rect(x, y, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);
				if (hasX && hasY && hasW && hasH && xywh.Length == 4)
					return new Rect(x, y, w, h);
			}

			throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(Rect)}");
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not Rect rect)
				throw new NotSupportedException();
			return $"{rect.X.ToString(CultureInfo.InvariantCulture)}, {rect.Y.ToString(CultureInfo.InvariantCulture)}, {(rect.Width == AbsoluteLayout.AutoSize ? nameof(AbsoluteLayout.AutoSize) : rect.Width.ToString(CultureInfo.InvariantCulture))}, {(rect.Height == AbsoluteLayout.AutoSize ? nameof(AbsoluteLayout.AutoSize) : rect.Height.ToString(CultureInfo.InvariantCulture))}";
		}
	}
}
