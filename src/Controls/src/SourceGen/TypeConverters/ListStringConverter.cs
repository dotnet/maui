using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

internal class ListStringConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => new[] { "List<string>", "System.Collections.Generic.List<string>", "System.Collections.Generic.IList`1[System.String]", "IList<string>" };

	public string Convert(string value, BaseNode node, ITypeSymbol toType, SourceGenContext context, LocalVariable? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;
		if (!string.IsNullOrEmpty(value))
		{
			value = value.Trim();

			// Validate input is safe for code generation
			if (!StringHelpers.IsSafeForCodeGeneration(value))
			{
				context.ReportConversionFailed(xmlLineInfo, value, Descriptors.ListStringConversionFailed);
				return "default";
			}

			var listStringType = context.Compilation.GetTypeByMetadataName("System.Collections.Generic.List`1")!;
			var stringType = context.Compilation.GetSpecialType(SpecialType.System_String);
			var constructedListType = listStringType.Construct(stringType);
			
			// Split and escape each string element individually
			var elements = value.Split([','], StringSplitOptions.RemoveEmptyEntries)
							   .Select(v => {
								   var trimmed = v.Trim();
								   if (!StringHelpers.IsSafeForCodeGeneration(trimmed))
									   return null; // Mark as invalid
								   return StringHelpers.EscapeStringLiteral(trimmed);
							   });
			
			// Check if any element was invalid
			if (elements.Any(e => e == null))
			{
				context.ReportConversionFailed(xmlLineInfo, value, Descriptors.ListStringConversionFailed);
				return "default";
			}
			
			return $"new {constructedListType.ToFQDisplayString()} {{ {string.Join(", ", elements)} }}";
		}

		context.ReportConversionFailed( xmlLineInfo, value, Descriptors.ListStringConversionFailed);
		return "default";
	}
}