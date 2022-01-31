using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/AcceleratorTypeConverter.xml" path="Type[@FullName='Microsoft.Maui.Controls.AcceleratorTypeConverter']/Docs" />
	public class AcceleratorTypeConverter : TypeConverter
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/AcceleratorTypeConverter.xml" path="//Member[@MemberName='CanConvertFrom']/Docs" />
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		/// <include file="../../docs/Microsoft.Maui.Controls/AcceleratorTypeConverter.xml" path="//Member[@MemberName='CanConvertTo']/Docs" />
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string);

		/// <include file="../../docs/Microsoft.Maui.Controls/AcceleratorTypeConverter.xml" path="//Member[@MemberName='ConvertFrom']/Docs" />
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();

			if (string.IsNullOrEmpty(strValue))
				return null;

			return Accelerator.FromString(strValue);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/AcceleratorTypeConverter.xml" path="//Member[@MemberName='ConvertTo']/Docs" />
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not Accelerator acc)
				throw new NotSupportedException();
			return acc.ToString();
		}
	}
}
