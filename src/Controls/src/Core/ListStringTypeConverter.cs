using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ListStringTypeConverter.xml" path="Type[@FullName='Microsoft.Maui.Controls.ListStringTypeConverter']/Docs/*" />
	[Xaml.ProvideCompiled("Microsoft.Maui.Controls.XamlC.ListStringTypeConverter")]
	public class ListStringTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string);

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();
			if (strValue == null)
				return null;

			return strValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not List<string> list)
				throw new NotSupportedException();
			return string.Join(", ", list);
		}
	}
}
