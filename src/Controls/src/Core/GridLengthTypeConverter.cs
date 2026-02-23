using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls
{
	[ProvideCompiled("Microsoft.Maui.Controls.XamlC.GridLengthTypeConverter")]
	[Obsolete("Microsoft.Maui.Controls.GridLengthTypeConverter is obsolete. Use Microsoft.Maui.Converters.GridLengthConverter instead.")]
	public class GridLengthTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
			=> true;

		public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
		{
			var strValue = value as string ?? value?.ToString();

			if (strValue is null)
				return null;

			return ParseStringToGridLength(strValue);
		}

#if NET6_0_OR_GREATER
		internal static GridLength ParseStringToGridLength(ReadOnlySpan<char> value)
#else
		internal static GridLength ParseStringToGridLength(string value)
#endif
		{
			value = value.Trim();

			if (value.Length != 0)
			{
				if (value.Length == 4 && value.Equals("auto", StringComparison.OrdinalIgnoreCase))
					return GridLength.Auto;

				if (value.Length == 1 && value[0] == '*')
					return GridLength.Star;

#if NET6_0_OR_GREATER
				var lastChar = value[^1];
#else
				var lastChar = value[value.Length - 1];
#endif
				if (lastChar == '*')
				{
#if NET6_0_OR_GREATER
					var prefix = value[..^1];
#else
					var prefix = value.Substring(0, value.Length - 1);
#endif

					if (double.TryParse(prefix, NumberStyles.Number, CultureInfo.InvariantCulture, out var starLength))
						return new GridLength(starLength, GridUnitType.Star);
				}

				if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var absoluteLength))
					return new GridLength(absoluteLength);
			}

			throw new FormatException($"Invalid GridLength format: {value.ToString()}");
		}

		public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
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
