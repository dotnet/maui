using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/RowDefinitionCollectionTypeConverter.xml" path="Type[@FullName='Microsoft.Maui.Controls.RowDefinitionCollectionTypeConverter']/Docs" />
	[ProvideCompiled("Microsoft.Maui.Controls.XamlC.RowDefinitionCollectionTypeConverter")]
	public class RowDefinitionCollectionTypeConverter : TypeConverter
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/RowDefinitionCollectionTypeConverter.xml" path="//Member[@MemberName='CanConvertFrom']/Docs" />
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		/// <include file="../../docs/Microsoft.Maui.Controls/RowDefinitionCollectionTypeConverter.xml" path="//Member[@MemberName='CanConvertTo']/Docs" />
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> true;

		/// <include file="../../docs/Microsoft.Maui.Controls/RowDefinitionCollectionTypeConverter.xml" path="//Member[@MemberName='ConvertFrom']/Docs" />
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();

			if (strValue != null)
			{
				var lengths = strValue.Split(',');
				var converter = new GridLengthTypeConverter();
				return new RowDefinitionCollection(lengths.Select(length => new RowDefinition {Height = (GridLength)converter.ConvertFromInvariantString(length) }).ToArray());
			}

			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", strValue, typeof(RowDefinitionCollection)));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RowDefinitionCollectionTypeConverter.xml" path="//Member[@MemberName='ConvertTo']/Docs" />
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not RowDefinitionCollection rdc)
				throw new NotSupportedException();
			var converter = new GridLengthTypeConverter();
			return string.Join(", ", rdc.Select(rd => converter.ConvertToInvariantString(rd.Height)));
		}
	}
}