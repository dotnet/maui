using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls
{
	[ProvideCompiled("Microsoft.Maui.Controls.XamlC.GridLengthTypeConverter")]
	public class GridLengthTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> true;

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();

			if (strValue == null)
				return null;

			strValue = strValue.Trim();
			if (string.Compare(strValue, "auto", StringComparison.OrdinalIgnoreCase) == 0)
				return GridLength.Auto;
			if (string.Compare(strValue, "*", StringComparison.OrdinalIgnoreCase) == 0)
				return new GridLength(1, GridUnitType.Star);
			if (strValue.EndsWith("*", StringComparison.Ordinal) && double.TryParse(strValue.Substring(0, strValue.Length - 1), NumberStyles.Number, CultureInfo.InvariantCulture, out var length))
				return new GridLength(length, GridUnitType.Star);
			if (double.TryParse(strValue, NumberStyles.Number, CultureInfo.InvariantCulture, out length))
				return new GridLength(length);

			throw new FormatException();
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not GridLength length)
				throw new NotSupportedException();
			if (length.IsAuto)
				return "auto";
			if (length.IsStar)
				return $"{length.Value.ToString(CultureInfo.InvariantCulture)}*";
			return $"{length.Value.ToString(CultureInfo.InvariantCulture)}";
		}
	}
}
