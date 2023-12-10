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
				case _ when Compare(strValue, Linear):
					return Linear;
				case _ when Compare(strValue, SinIn):
					return SinIn;
				case _ when Compare(strValue, SinOut):
					return SinOut;
				case _ when Compare(strValue, SinInOut):
					return SinInOut;
				case _ when Compare(strValue, CubicIn):
					return CubicIn;
				case _ when Compare(strValue, CubicOut):
					return CubicOut;
				case _ when Compare(strValue, CubicInOut):
					return CubicInOut;
				case _ when Compare(strValue, BounceIn):
					return BounceIn;
				case _ when Compare(strValue, BounceOut):
					return BounceOut;
				case _ when Compare(strValue, SpringIn):
					return SpringIn;
				case _ when Compare(strValue, SpringOut):
					return SpringOut;
			}
			bool Compare(string left, string right) => left.Equals(right, StringComparison.InvariantCultureIgnoreCase);

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
