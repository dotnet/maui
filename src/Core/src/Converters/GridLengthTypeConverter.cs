using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Converters;

public sealed class GridLengthTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type? sourceType)
			=> sourceType == typeof(double) || sourceType == typeof(string);

		public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object? value)
			=> value switch
			{
				double d => (GridLength)d,
				string strValue => strValue.Trim().ToLowerInvariant() switch
				{
					"auto" => GridLength.Auto,
					"*" => new GridLength(1, GridUnitType.Star),
#pragma warning disable CA1846, CA1865
					_ when strValue.EndsWith("*", StringComparison.Ordinal) && double.TryParse(strValue.Substring(0, strValue.Length - 1), NumberStyles.Number, CultureInfo.InvariantCulture, out var length) => new GridLength(length, GridUnitType.Star),
#pragma warning restore CA1846, CA1865
					_ when double.TryParse(strValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var length) => new GridLength(length),
					_ => throw new FormatException(),
				},
				_ => throw new NotSupportedException(),
			};

		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType == typeof(string);
		public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type? destinationType)
		{
			if (destinationType == typeof(string) && value is GridLength length)
			{
				if (length.IsAuto)
					return "auto";
				if (length.IsStar)
					return $"{length.Value.ToString(CultureInfo.InvariantCulture)}*";
				return $"{length.Value.ToString(CultureInfo.InvariantCulture)}";
			}
			throw new NotSupportedException($"Cannot convert {value?.GetType()} to {destinationType}");

		}
	}