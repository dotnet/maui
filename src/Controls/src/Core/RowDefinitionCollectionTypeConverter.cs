#nullable disable
using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
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
			var strValue = value?.ToString()
				?? throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(RowDefinitionCollection)));

			var definitions = ConvertFrom(strValue);
			
			return new RowDefinitionCollection(definitions);
		}


#if NETSTANDARD2_1_OR_GREATER
		private static RowDefinition[] ConvertFrom(ReadOnlySpan<char> value)
#else
		private static RowDefinition[] ConvertFrom(string value)
#endif
		{
			int start = 0;

			int rowsCount = 0;
			for (int i = 0; i < value.Length; i++)
			{
				if (value[i] == ',')
				{
					rowsCount++;
				}
			}
			rowsCount++;

			RowDefinition[] definitions = new RowDefinition[rowsCount];
			int currentRow = 0;

			for (int i = 0; i < value.Length; i++)
			{
				if (value[i] == ',')
				{
#if NETSTANDARD2_1_OR_GREATER
					var gridLengthSlice = value[start..i];
#else
    				var gridLengthSlice = value.Substring(start, i - start);
#endif
					var gridLength = GridLengthTypeConverter.ParseStringToGridLength(gridLengthSlice);
					definitions[currentRow++] = new RowDefinition(gridLength);
					start = i + 1;
				}
			}

			// Handle the last segment
			if (start < value.Length)
			{
#if NETSTANDARD2_1_OR_GREATER
				var gridLengthSlice = value[start..];
#else
				var gridLengthSlice = value.Substring(start);
#endif
				var gridLength = GridLengthTypeConverter.ParseStringToGridLength(gridLengthSlice);
				definitions[currentRow] = new RowDefinition(gridLength);
			}

			return definitions;
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not RowDefinitionCollection rowDefinitions)
				throw new NotSupportedException();

    		StringBuilder sb = new(rowDefinitions.Count * 4);

			int count = rowDefinitions.Count;
    		int lastIndex = count - 1;

			for (var i = 0; i < count; i++)
			{
				var rowDefinition = rowDefinitions[i];
				var width = GridLengthTypeConverter.ConvertToString(rowDefinition.Height);

				if (i < lastIndex)
				{
					sb.Append(width).Append(", ");
				}
				else
				{
					sb.Append(width);
				}
			}

			return sb.ToString();
		}
	}
}
