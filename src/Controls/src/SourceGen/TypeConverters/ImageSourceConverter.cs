using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

class ImageSourceConverter : ISGTypeConverter
{
	public IEnumerable<string> SupportedTypes => ["ImageSource", "Microsoft.Maui.Controls.ImageSource"];

	public string Convert(string value, BaseNode node, ITypeSymbol toType, IndentedTextWriter writer, SourceGenContext context, ILocalValue? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;
		// IMPORTANT! Update ImageSourceDesignTypeConverter.IsValid if making changes here
		if (!string.IsNullOrEmpty(value))
		{
			value = value.Trim();

			var imageSourceType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.ImageSource")!;
			var uriType = context.Compilation.GetTypeByMetadataName("System.Uri")!;
			
			return Uri.TryCreate(value, UriKind.Absolute, out Uri uri) && uri.Scheme != "file" ?
				$"{imageSourceType.ToFQDisplayString()}.FromUri(new {uriType.ToFQDisplayString()}(\"{uri}\"))" : 
				$"{imageSourceType.ToFQDisplayString()}.FromFile(\"{value}\")";
		}

		context.ReportConversionFailed( xmlLineInfo, value, toType, Descriptors.ConversionFailed);
		return "default";
	}
}