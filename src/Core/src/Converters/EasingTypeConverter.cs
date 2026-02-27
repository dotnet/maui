using System;
using System.ComponentModel;
using System.Globalization;
using static Microsoft.Maui.Easing;

namespace Microsoft.Maui.Converters
{
	/// <inheritdoc/>
	public class EasingTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
			=> sourceType == typeof(string) || sourceType == typeof(Func<double, double>);

		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
			=> destinationType == typeof(string);

		public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
		{
			if (value is null)
			{
				return null;
			}

			var strValue = value as string ?? value.ToString();

			if (string.IsNullOrWhiteSpace(strValue))
			{
				return null;
			}

			var parts = strValue.Split('.');
			if (parts.Length == 2 && Compare(parts[0], nameof(Easing)))
				strValue = parts[1];

			return strValue switch
			{
				_ when Compare(strValue, nameof(Linear)) => Linear,
				_ when Compare(strValue, nameof(SinIn)) => SinIn,
				_ when Compare(strValue, nameof(SinOut)) => SinOut,
				_ when Compare(strValue, nameof(SinInOut)) => SinInOut,
				_ when Compare(strValue, nameof(CubicIn)) => CubicIn,
				_ when Compare(strValue, nameof(CubicOut)) => CubicOut,
				_ when Compare(strValue, nameof(CubicInOut)) => CubicInOut,
				_ when Compare(strValue, nameof(BounceIn)) => BounceIn,
				_ when Compare(strValue, nameof(BounceOut)) => BounceOut,
				_ when Compare(strValue, nameof(SpringIn)) => SpringIn,
				_ when Compare(strValue, nameof(SpringOut)) => SpringOut,
				_ => throw new InvalidOperationException($"Cannot convert \"{strValue}\" into {typeof(Easing)}")
			};

			static bool Compare(string left, string right) =>
				left.Equals(right, StringComparison.OrdinalIgnoreCase);
		}

		public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
		{
			if (value is not Easing easing)
			{
				throw new NotSupportedException();
			}

			return easing switch
			{
				_ when easing.Equals(Linear) => nameof(Linear),
				_ when easing.Equals(SinIn) => nameof(SinIn),
				_ when easing.Equals(SinOut) => nameof(SinOut),
				_ when easing.Equals(SinInOut) => nameof(SinInOut),
				_ when easing.Equals(CubicIn) => nameof(CubicIn),
				_ when easing.Equals(CubicOut) => nameof(CubicOut),
				_ when easing.Equals(CubicInOut) => nameof(CubicInOut),
				_ when easing.Equals(BounceIn) => nameof(BounceIn),
				_ when easing.Equals(BounceOut) => nameof(BounceOut),
				_ when easing.Equals(SpringIn) => nameof(SpringIn),
				_ when easing.Equals(SpringOut) => nameof(SpringOut),
				_ => throw new NotSupportedException(),
			};
		}

		public override bool GetStandardValuesSupported(ITypeDescriptorContext? context)
			=> true;

		public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context)
			=> false;

		public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext? context)
			=> new(new[] {
			"Linear",
			"SinOut",
			"SinIn",
			"SinInOut",
			"CubicIn",
			"CubicOut",
			"CubicInOut",
			"BounceOut",
			"BounceIn",
			"SpringIn",
			"SpringOut"
			});
	}
}