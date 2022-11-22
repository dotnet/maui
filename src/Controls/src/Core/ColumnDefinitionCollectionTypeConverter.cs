using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ColumnDefinitionCollectionTypeConverter.xml" path="Type[@FullName='Microsoft.Maui.Controls.ColumnDefinitionCollectionTypeConverter']/Docs/*" />
	[ProvideCompiled("Microsoft.Maui.Controls.XamlC.ColumnDefinitionCollectionTypeConverter")]
	public class ColumnDefinitionCollectionTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string);

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();

			if (strValue != null)
			{
				var lengths = strValue.Split(',');
				var converter = new GridLengthTypeConverter();
				var definitions = new ColumnDefinition[lengths.Length];
				for (var i = 0; i < lengths.Length; i++)
					definitions[i] = new ColumnDefinition { Width = (GridLength)converter.ConvertFromInvariantString(lengths[i]) };
				return new ColumnDefinitionCollection(definitions);
			}

			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", strValue, typeof(ColumnDefinitionCollection)));
		}


		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not ColumnDefinitionCollection cdc)
				throw new NotSupportedException();
			var converter = new GridLengthTypeConverter();
			return string.Join(", ", cdc.Select(cd => converter.ConvertToInvariantString(cd.Width)));
		}
	}
}
