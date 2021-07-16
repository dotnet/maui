using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	[Xaml.ProvideCompiled("Microsoft.Maui.Controls.XamlC.UriTypeConverter")]
	[Xaml.TypeConversion(typeof(Uri))]
	public class UriTypeConverter : StringTypeConverterBase
	{
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();
			if (string.IsNullOrWhiteSpace(strValue))
				return null;
			return new Uri(strValue, UriKind.RelativeOrAbsolute);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not Uri uri)
				throw new NotSupportedException();
			return uri.ToString();
		}
	}
}
