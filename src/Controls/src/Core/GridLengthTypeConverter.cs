using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls
{
	[ProvideCompiled("Microsoft.Maui.Controls.XamlC.GridLengthTypeConverter")]
	public class GridLengthTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
			=> true;

		public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object? value)
		{
			var strValue = value?.ToString();

			if (strValue == null)
				return null;

			return ParseStringToGridLength(strValue);
		}

		internal static GridLength ParseStringToGridLength(ReadOnlySpan<char> value)
		{
			value = value.Trim();

			if (value.Length != 0)
			{
				if (value.Length == 4 && value.Equals("auto", StringComparison.OrdinalIgnoreCase))
					return GridLength.Auto;

				if (value[^1] == '*')
				{
					if (value.Length == 1)
						return GridLength.Star;

					if (double.TryParse(value[..^1], NumberStyles.Number, CultureInfo.InvariantCulture, out var starLength))
						return new GridLength(starLength, GridUnitType.Star);
				}

				if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var absoluteLength))
					return new GridLength(absoluteLength);
			}

			throw new FormatException($"Invalid GridLength format: {value.ToString()}");
		}

		public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
		{
			if (value is not GridLength length)
				throw new NotSupportedException();

			return ConvertToString(length);
		}

		internal static string ConvertToString(GridLength length)
		{
			if (length.IsAuto)
				return "auto";
			if (length.IsStar)
				return $"{length.Value.ToString(CultureInfo.InvariantCulture)}*";
			return length.Value.ToString(CultureInfo.InvariantCulture);
		}
	}
}
