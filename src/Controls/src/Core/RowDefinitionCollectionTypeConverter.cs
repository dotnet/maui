using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/RowDefinitionCollectionTypeConverter.xml" path="Type[@FullName='Microsoft.Maui.Controls.RowDefinitionCollectionTypeConverter']/Docs/*" />
	[ProvideCompiled("Microsoft.Maui.Controls.XamlC.RowDefinitionCollectionTypeConverter")]
	public class RowDefinitionCollectionTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> true;

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();

			if (strValue != null)
			{
				var lengths = strValue.Split(',');
				var converter = new GridLengthTypeConverter();
				var definitions = new RowDefinition[lengths.Length];
				for (var i = 0; i < lengths.Length; i++)
					definitions[i] = new RowDefinition { Height = (GridLength)converter.ConvertFromInvariantString(lengths[i]) };
				return new RowDefinitionCollection(definitions);
			}

			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", strValue, typeof(RowDefinitionCollection)));
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not RowDefinitionCollection rdc)
				throw new NotSupportedException();
			var converter = new GridLengthTypeConverter();
			return string.Join(", ", rdc.Select(rd => converter.ConvertToInvariantString(rd.Height)));
		}
	}
}
