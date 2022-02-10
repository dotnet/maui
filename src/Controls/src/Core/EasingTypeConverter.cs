using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using static Microsoft.Maui.Easing;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/EasingTypeConverter.xml" path="Type[@FullName='Microsoft.Maui.Controls.EasingTypeConverter']/Docs" />
	[Xaml.ProvideCompiled("Microsoft.Maui.Controls.XamlC.EasingTypeConverter")]
	public class EasingTypeConverter : TypeConverter
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/EasingTypeConverter.xml" path="//Member[@MemberName='CanConvertFrom']/Docs" />
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		/// <include file="../../docs/Microsoft.Maui.Controls/EasingTypeConverter.xml" path="//Member[@MemberName='CanConvertTo']/Docs" />
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string);

		/// <include file="../../docs/Microsoft.Maui.Controls/EasingTypeConverter.xml" path="//Member[@MemberName='ConvertFrom']/Docs" />
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();

			if (string.IsNullOrWhiteSpace(strValue))
				return null;

			strValue = strValue?.Trim() ?? "";
			var parts = strValue.Split('.');
			if (parts.Length == 2 && parts[0] == nameof(Easing))
				strValue = parts[parts.Length - 1];

			if (strValue.Equals(nameof(Linear), StringComparison.OrdinalIgnoreCase))
				return Linear;
			if (strValue.Equals(nameof(SinIn), StringComparison.OrdinalIgnoreCase))
				return SinIn;
			if (strValue.Equals(nameof(SinOut), StringComparison.OrdinalIgnoreCase))
				return SinOut;
			if (strValue.Equals(nameof(SinInOut), StringComparison.OrdinalIgnoreCase))
				return SinInOut;
			if (strValue.Equals(nameof(CubicIn), StringComparison.OrdinalIgnoreCase))
				return CubicIn;
			if (strValue.Equals(nameof(CubicOut), StringComparison.OrdinalIgnoreCase))
				return CubicOut;
			if (strValue.Equals(nameof(CubicInOut), StringComparison.OrdinalIgnoreCase))
				return CubicInOut;
			if (strValue.Equals(nameof(BounceIn), StringComparison.OrdinalIgnoreCase))
				return BounceIn;
			if (strValue.Equals(nameof(BounceOut), StringComparison.OrdinalIgnoreCase))
				return BounceOut;
			if (strValue.Equals(nameof(SpringIn), StringComparison.OrdinalIgnoreCase))
				return SpringIn;
			if (strValue.Equals(nameof(SpringOut), StringComparison.OrdinalIgnoreCase))
				return SpringOut;

			var fallbackValue = typeof(Easing)
				.GetTypeInfo()
				.DeclaredFields
				.FirstOrDefault(f => f.Name.Equals(strValue, StringComparison.OrdinalIgnoreCase))
				?.GetValue(null);

			if (fallbackValue is Easing fallbackEasing)
				return fallbackEasing;

			throw new InvalidOperationException($"Cannot convert \"{strValue}\" into {typeof(Easing)}");
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/EasingTypeConverter.xml" path="//Member[@MemberName='ConvertTo']/Docs" />
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not Easing easing)
				throw new NotSupportedException();

			if (easing == Linear)
				return nameof(Linear);
			if (easing == SinIn)
				return nameof(SinIn);
			if (easing == SinOut)
				return nameof(SinOut);
			if (easing == SinInOut)
				return nameof(SinInOut);
			if (easing == CubicIn)
				return nameof(CubicIn);
			if (easing == CubicOut)
				return nameof(CubicOut);
			if (easing == CubicInOut)
				return nameof(CubicInOut);
			if (easing == BounceIn)
				return nameof(BounceIn);
			if (easing == BounceOut)
				return nameof(BounceOut);
			if (easing == SpringIn)
				return nameof(SpringIn);
			if (easing == SpringOut)
				return nameof(SpringOut);
			throw new NotSupportedException();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/EasingTypeConverter.xml" path="//Member[@MemberName='GetStandardValuesSupported']/Docs" />
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
			=> true;

		/// <include file="../../docs/Microsoft.Maui.Controls/EasingTypeConverter.xml" path="//Member[@MemberName='GetStandardValuesExclusive']/Docs" />
		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
			=> false;

		/// <include file="../../docs/Microsoft.Maui.Controls/EasingTypeConverter.xml" path="//Member[@MemberName='GetStandardValues']/Docs" />
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
