using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/UriTypeConverter.xml" path="Type[@FullName='Microsoft.Maui.Controls.UriTypeConverter']/Docs" />
	[Xaml.ProvideCompiled("Microsoft.Maui.Controls.XamlC.UriTypeConverter")]
	public class UriTypeConverter : TypeConverter
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/UriTypeConverter.xml" path="//Member[@MemberName='CanConvertFrom']/Docs" />
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		/// <include file="../../docs/Microsoft.Maui.Controls/UriTypeConverter.xml" path="//Member[@MemberName='CanConvertTo']/Docs" />
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string);

		/// <include file="../../docs/Microsoft.Maui.Controls/UriTypeConverter.xml" path="//Member[@MemberName='ConvertFrom']/Docs" />
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();
			if (string.IsNullOrWhiteSpace(strValue))
				return null;
			return new Uri(strValue, UriKind.RelativeOrAbsolute);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/UriTypeConverter.xml" path="//Member[@MemberName='ConvertTo']/Docs" />
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not Uri uri)
				throw new NotSupportedException();
			return uri.ToString();
		}
	}
}
