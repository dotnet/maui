using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ColumnDefinitionCollectionTypeConverter.xml" path="Type[@FullName='Microsoft.Maui.Controls.ColumnDefinitionCollectionTypeConverter']/Docs/*" />
	[ProvideCompiled("Microsoft.Maui.Controls.XamlC.ColumnDefinitionCollectionTypeConverter")]
	public class ColumnDefinitionCollectionTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
			=> destinationType == typeof(string);

		public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object? value)
		{
			var strValue = value?.ToString()
				?? throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(ColumnDefinitionCollection)));

			var definitions = ConvertFrom(strValue);

			return new ColumnDefinitionCollection(definitions);
		}

#if NETSTANDARD2_1_OR_GREATER
		private static ColumnDefinition[] ConvertFrom(ReadOnlySpan<char> value)
#else
		private static ColumnDefinition[] ConvertFrom(string value)
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

			ColumnDefinition[] definitions = new ColumnDefinition[rowsCount];
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
					definitions[currentRow++] = new ColumnDefinition(gridLength);
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
				definitions[currentRow] = new ColumnDefinition(gridLength);
			}

			return definitions;
		}

		public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
		{
			if (value is not ColumnDefinitionCollection columnDefinitions)
				throw new NotSupportedException();

			StringBuilder sb = new(columnDefinitions.Count * 4);

			int count = columnDefinitions.Count;
			int lastIndex = count - 1;

			for (var i = 0; i < count; i++)
			{
				var columnDefinition = columnDefinitions[i];
				var width = GridLengthTypeConverter.ConvertToString(columnDefinition.Width);

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
