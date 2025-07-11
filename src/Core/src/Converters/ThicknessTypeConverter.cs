using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Converters
{

	/// <inheritdoc/>
	public class ThicknessTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
			=> sourceType == typeof(string)
				|| sourceType == typeof(Size)
				|| PrimitiveTypeConversions.IsImplicitlyConvertibleToDouble(sourceType);

		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
			=> destinationType == typeof(string);

		public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
		{
			if (PrimitiveTypeConversions.TryGetDouble(value, out double d))
			{
				return (Thickness)d;
			}
			else if (value is Size s)
			{
				return (Thickness)s;
			}

			// IMPORTANT! Update ThicknessTypeDesignConverter.IsValid if making changes here
			var strValue = value?.ToString();

			if (strValue != null)
			{
				strValue = strValue.Trim();

				if (strValue.ContainsChar(','))
				{ //Xaml
					var thickness = strValue.Split(',');
					switch (thickness.Length)
					{
						case 2:
							if (double.TryParse(thickness[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double h)
								&& double.TryParse(thickness[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double v))
							{
								return new Thickness(h, v);
							}

							break;
						case 4:
							if (double.TryParse(thickness[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double l)
								&& double.TryParse(thickness[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double t)
								&& double.TryParse(thickness[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double r)
								&& double.TryParse(thickness[3], NumberStyles.Number, CultureInfo.InvariantCulture, out double b))
							{
								return new Thickness(l, t, r, b);
							}

							break;
					}
				}
				else if (strValue.ContainsChar(' '))
				{ //CSS
					var thickness = strValue.Split(' ');
					switch (thickness.Length)
					{
						case 2:
							if (double.TryParse(thickness[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double v)
								&& double.TryParse(thickness[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double h))
							{
								return new Thickness(h, v);
							}

							break;
						case 3:
							if (double.TryParse(thickness[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double t)
								&& double.TryParse(thickness[1], NumberStyles.Number, CultureInfo.InvariantCulture, out h)
								&& double.TryParse(thickness[2], NumberStyles.Number, CultureInfo.InvariantCulture, out double b))
							{
								return new Thickness(h, t, h, b);
							}

							break;
						case 4:
							if (double.TryParse(thickness[0], NumberStyles.Number, CultureInfo.InvariantCulture, out t)
								&& double.TryParse(thickness[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double r)
								&& double.TryParse(thickness[2], NumberStyles.Number, CultureInfo.InvariantCulture, out b)
								&& double.TryParse(thickness[3], NumberStyles.Number, CultureInfo.InvariantCulture, out double l))
							{
								return new Thickness(l, t, r, b);
							}

							break;
					}
				}
				else
				{ //single uniform thickness
					if (double.TryParse(strValue, NumberStyles.Number, CultureInfo.InvariantCulture, out double l))
					{
						return new Thickness(l);
					}
				}
			}

			throw new InvalidOperationException($"Cannot convert \"{strValue}\" into {typeof(Thickness)}");
		}

		public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
		{
			if (value is not Thickness t)
			{
				throw new NotSupportedException();
			}

			return $"{t.Left.ToString(CultureInfo.InvariantCulture)}, {t.Top.ToString(CultureInfo.InvariantCulture)}, " +
				$"{t.Right.ToString(CultureInfo.InvariantCulture)}, {t.Bottom.ToString(CultureInfo.InvariantCulture)}";
		}
	}
}