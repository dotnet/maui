#nullable disable
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
			string s = value?.ToString();

			if (s is null)
			{
				return null;
			}

#if NETSTANDARD2_0_OR_GREATER
			string strValue = s.Trim();
#else
			ReadOnlySpan<char> strValue = s.AsSpan().Trim();
#endif

			if (strValue.Equals("auto", StringComparison.OrdinalIgnoreCase))
			{
				return GridLength.Auto;
			}

			if (strValue.Equals("*", StringComparison.OrdinalIgnoreCase))
			{
				return new GridLength(1, GridUnitType.Star);
			}

#if NETSTANDARD2_0_OR_GREATER
			if (strValue.EndsWith("*", StringComparison.Ordinal) && double.TryParse(strValue.Substring(0, strValue.Length - 1), NumberStyles.Number, CultureInfo.InvariantCulture, out var length))
#else
			if (strValue.EndsWith("*", StringComparison.Ordinal) && double.TryParse(strValue[0..(strValue.Length - 1)], NumberStyles.Number, CultureInfo.InvariantCulture, out var length))
#endif
			{
				return new GridLength(length, GridUnitType.Star);
			}

			if (double.TryParse(strValue, NumberStyles.Number, CultureInfo.InvariantCulture, out length))
			{
				return new GridLength(length);
			}

			throw new FormatException();
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not GridLength length)
			{
				throw new NotSupportedException();
			}

			if (length.IsAuto)
			{
				return "auto";
			}

			if (length.IsStar)
			{
				return $"{length.Value.ToString(CultureInfo.InvariantCulture)}*";
			}

			return $"{length.Value.ToString(CultureInfo.InvariantCulture)}";
		}
	}
}
