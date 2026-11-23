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

			var parsed = EasingSyntax.Parse(strValue);
			return parsed.Kind switch
			{
				EasingSyntax.Kind.Named => FromName(parsed.Name!),
				EasingSyntax.Kind.CubicBezier => CubicBezier(parsed.A, parsed.B, parsed.C, parsed.D),
				EasingSyntax.Kind.Spring => Spring(parsed.A),
				EasingSyntax.Kind.Steps => Steps((int)parsed.A, parsed.Flag),
				_ => throw new InvalidOperationException($"Cannot convert \"{strValue}\" into {typeof(Easing)}. {parsed.Error}")
			};

			static Easing FromName(string name) => name switch
			{
				nameof(Linear) => Linear,
				nameof(SinIn) => SinIn,
				nameof(SinOut) => SinOut,
				nameof(SinInOut) => SinInOut,
				nameof(CubicIn) => CubicIn,
				nameof(CubicOut) => CubicOut,
				nameof(CubicInOut) => CubicInOut,
				nameof(BounceIn) => BounceIn,
				nameof(BounceOut) => BounceOut,
				nameof(SpringIn) => SpringIn,
				nameof(SpringOut) => SpringOut,
				nameof(Hold) => Hold,
				_ => throw new InvalidOperationException($"Cannot convert \"{name}\" into {typeof(Easing)}")
			};
		}

		public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
		{
			if (value is not Easing easing)
			{
				throw new NotSupportedException();
			}

			if (easing.Syntax is string syntax)
			{
				return syntax;
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
			"SpringOut",
			"Hold",
			"CubicBezier(0.25, 0.1, 0.25, 1)",
			"Spring(0.2)",
			"Steps(4)"
			});
	}
}