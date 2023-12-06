using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using static Microsoft.Maui.Easing;

#nullable disable

namespace Microsoft.Maui.Converters
{
	/// <inheritdoc/>
	public class EasingTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string);

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();

			if (string.IsNullOrWhiteSpace(strValue))
				return null;

			var parts = (strValue = strValue.Trim()).Split('.');

			if (parts.Length == 2 && parts[0] == nameof(Easing))
				strValue = parts[parts.Length - 1];

			switch (easing)
			{
				case nameof(Linear):
					return nameof(Linear);
				case nameof(SinIn):
					return nameof(SinIn);
				case nameof(SinOut):
					return nameof(SinOut);
				case nameof(SinInOut):
					return nameof(SinInOut);
				case nameof(CubicIn):
					return nameof(CubicIn);
				case nameof(CubicOut):
					return nameof(CubicOut);
				case nameof(CubicInOut):
					return nameof(CubicInOut);
				case nameof(BounceIn):
					return nameof(BounceIn);
				case nameof(BounceOut):
					return nameof(BounceOut);
				case nameof(SpringIn):
					return nameof(SpringIn);
				case nameof(SpringOut):
					return nameof(SpringOut);
			}

			var delcaredFields = typeof(Easing)
								.GetTypeInfo()
								.DeclaredFields;

			var fallbackValue = null;
			for (int i = 0; i < delcaredFields.Length; i++)
			{
				if (delcaredFields[i].Name.Equals(strValue, StringComparison.OrdinalIgnoreCase))
				{
					fallbackValue = delcaredFields[i].GetValue(null);
					break;
				}
			}

			if (fallbackValue is Easing fallbackEasing)
				return fallbackEasing;

			throw new InvalidOperationException($"Cannot convert \"{strValue}\" into {typeof(Easing)}");
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not Easing easing)
				throw new NotSupportedException();

			return easing switch
			{
				nameof(Linear) => nameof(Linear),
				nameof(SinIn) => nameof(SinIn),
				nameof(SinOut) => nameof(SinOut),
				nameof(SinInOut) => nameof(SinInOut),
				nameof(CubicIn) => nameof(CubicIn),
				nameof(CubicOut) => nameof(CubicOut),
				nameof(CubicInOut) => nameof(CubicInOut),
				nameof(BounceIn) => nameof(BounceIn),
				nameof(BounceOut) => nameof(BounceOut),
				nameof(SpringIn) => nameof(SpringIn),
				nameof(SpringOut) => nameof(SpringOut),
				_ => throw new NotSupportedException(),
			};
		}

		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			=> true;

		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			=> false;

		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
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
