using System;
using System.Buffers;
using System.Collections.Generic;
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
		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
			=> destinationType == typeof(string);

		public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
		{
			var strValue = value as string ?? value?.ToString()
				?? throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(RowDefinitionCollection)));

			// fast path for no value or empty string
			if (strValue.Length == 0)
				return new RowDefinitionCollection();

#if NET6_0_OR_GREATER
			var unsplit = (ReadOnlySpan<char>)strValue;
			var count = unsplit.Count(',') + 1;
			var definitions = new List<RowDefinition>(count);
			foreach (var range in unsplit.Split(','))
			{
				var length = Converters.GridLengthTypeConverter.ParseStringToGridLength(unsplit[range]);
				definitions.Add(new RowDefinition(length));
			}
#else
			var lengths = strValue.Split(',');
			var count = lengths.Length;
			var definitions = new List<RowDefinition>(count);
			foreach (var lengthStr in lengths)
			{
				var length = Converters.GridLengthTypeConverter.ParseStringToGridLength(lengthStr);
				definitions.Add(new RowDefinition(length));
			}
#endif

			return new RowDefinitionCollection(definitions, copy: false);
		}

		public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
		{
			if (value is not RowDefinitionCollection definitions)
				throw new NotSupportedException();

			var count = definitions.Count;

			// fast path for empty or single definitions
			if (count == 0)
				return string.Empty;
			if (count == 1)
				return Converters.GridLengthTypeConverter.ConvertToString(definitions[0].Height);

			// for multiple items
			var pool = ArrayPool<string>.Shared;
			var rentedArray = pool.Rent(definitions.Count);
			for (var i = 0; i < definitions.Count; i++)
			{
				var definition = definitions[i];
				rentedArray[i] = Converters.GridLengthTypeConverter.ConvertToString(definition.Height);
			}
			var result = string.Join(", ", rentedArray, 0, definitions.Count);
			pool.Return(rentedArray);
			return result;
		}
	}
}