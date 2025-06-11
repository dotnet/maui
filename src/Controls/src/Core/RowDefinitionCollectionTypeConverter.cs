#nullable disable
using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
			=> destinationType == typeof(string);

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value as string ?? value?.ToString()
				?? throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(RowDefinitionCollection)));

			// fast path for no value or empty string
			if (strValue.Length == 0)
				return new RowDefinitionCollection();

#if NETSTANDARD2_1_OR_GREATER
			var unsplit = (ReadOnlySpan<char>)strValue;
#else
			var unsplit = strValue;
#endif
			var lengths = unsplit.Split(',');

			var definitions = new List<RowDefinition>(lengths.Length);
			for (var i = 0; i < lengths.Length; i++)
			{
				var length = GridLengthTypeConverter.ParseStringToGridLength(lengths[i]);
				definitions.Add(new RowDefinition(length));
			}
			return new RowDefinitionCollection(definitions, copy: false);
		}


		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not RowDefinitionCollection definitions)
				throw new NotSupportedException();

			var count = definitions.Count;

			// fast path for empty or single definitions
			if (count == 0)
				return string.Empty;
			if (count == 1)
				return GridLengthTypeConverter.ConvertToString(definitions[0].Height);

			// for multiple items
			var pool = ArrayPool<string>.Shared;
			var rentedArray = pool.Rent(definitions.Count);
			for (var i = 0; i < definitions.Count; i++)
			{
				var definition = definitions[i];
				rentedArray[i] = GridLengthTypeConverter.ConvertToString(definition.Height);
			}
			var result = string.Join(", ", rentedArray, 0, definitions.Count);
			pool.Return(rentedArray);
			return result;
		}
	}
}
