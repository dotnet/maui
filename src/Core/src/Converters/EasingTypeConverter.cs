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

			if (TryParseFunction(strValue, out var function))
			{
				return function;
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
				_ when Compare(strValue, nameof(Hold)) => Hold,
				_ => throw new InvalidOperationException($"Cannot convert \"{strValue}\" into {typeof(Easing)}")
			};

			static bool Compare(string left, string right) =>
				left.Equals(right, StringComparison.OrdinalIgnoreCase);
		}

		// Function forms: Spring(), Spring(bounce), CubicBezier(x1, y1, x2, y2), Steps(count) and Steps(count, start|end).
		static bool TryParseFunction(string value, out Easing? easing)
		{
			easing = null;

			var text = value.Trim();
			if (text.StartsWith(nameof(Easing) + ".", StringComparison.OrdinalIgnoreCase))
				text = text.Substring(nameof(Easing).Length + 1);

#if !NETSTANDARD2_0
			var open = text.IndexOf('(', StringComparison.Ordinal);
#else
			var open = text.IndexOf("(", StringComparison.Ordinal);
#endif
			if (open <= 0 || text[text.Length - 1] != ')')
				return false;

			var name = text.Substring(0, open).Trim();
			var argumentText = text.Substring(open + 1, text.Length - open - 2).Trim();
			var arguments = argumentText.Length == 0 ? Array.Empty<string>() : argumentText.Split(',');

			if (IsName(name, nameof(Spring)) && arguments.Length <= 1)
				easing = arguments.Length == 0 ? Spring() : Spring(ParseNumber(arguments[0]));
			else if (IsName(name, nameof(CubicBezier)) && arguments.Length == 4)
				easing = CubicBezier(ParseNumber(arguments[0]), ParseNumber(arguments[1]), ParseNumber(arguments[2]), ParseNumber(arguments[3]));
			else if (IsName(name, nameof(Steps)) && arguments.Length is 1 or 2)
				easing = Steps(int.Parse(arguments[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture), arguments.Length == 2 && ParseJumpAtStart(arguments[1]));
			else
				throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(Easing)}");

			return true;

			static bool IsName(string left, string right) => left.Equals(right, StringComparison.OrdinalIgnoreCase);

			static double ParseNumber(string text) => double.Parse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);

			static bool ParseJumpAtStart(string text)
			{
				var position = text.Trim();
				if (IsName(position, "start") || IsName(position, "jump-start"))
					return true;
				if (IsName(position, "end") || IsName(position, "jump-end"))
					return false;
				throw new FormatException($"Expected 'start' or 'end', but got \"{position}\".");
			}
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
				_ when easing.Equals(Hold) => nameof(Hold),
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
			"SpringOut",
			"Hold"
			});
	}
}