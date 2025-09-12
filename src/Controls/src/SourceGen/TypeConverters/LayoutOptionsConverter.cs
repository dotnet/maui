using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

internal class LayoutOptionsConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => new[] { "LayoutOptions", "Microsoft.Maui.Controls.LayoutOptions" };

	public string Convert(string value, BaseNode node, ITypeSymbol toType, SourceGenContext context, LocalVariable? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;
		if (!string.IsNullOrEmpty(value))
		{
			value = value.Trim();

			var parts = value.Split(['.']);
			if (parts.Length > 2 || (parts.Length == 2 && parts[0] != "LayoutOptions"))
			{
				context.ReportConversionFailed( xmlLineInfo, value, Descriptors.LayoutOptionsConversionFailed);
				return "default";
			}

			value = parts[parts.Length - 1];

			var layoutOptionsType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.LayoutOptions")!;
			var layoutOptionsMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				{ "Start", $"{layoutOptionsType.ToFQDisplayString()}.Start" },
				{ "Center", $"{layoutOptionsType.ToFQDisplayString()}.Center" },
				{ "End", $"{layoutOptionsType.ToFQDisplayString()}.End" },
				{ "Fill", $"{layoutOptionsType.ToFQDisplayString()}.Fill" },

				// The following options are obsoleted, but here for now for compatibility
				{ "StartAndExpand", $"{layoutOptionsType.ToFQDisplayString()}.StartAndExpand" },
				{ "CenterAndExpand", $"{layoutOptionsType.ToFQDisplayString()}.CenterAndExpand" },
				{ "EndAndExpand", $"{layoutOptionsType.ToFQDisplayString()}.EndAndExpand" },
				{ "FillAndExpand", $"{layoutOptionsType.ToFQDisplayString()}.FillAndExpand" }
			};			
		
			if (layoutOptionsMap.TryGetValue(value, out var layoutOption))
			{
				return layoutOption;
			}
		}

		context.ReportConversionFailed( xmlLineInfo, value, Descriptors.LayoutOptionsConversionFailed);
		return "default";
	}
}