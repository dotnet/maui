using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

internal class ImageSourceConverter : BaseTypeConverter
{
	public override IEnumerable<string> SupportedTypes => new[] { "ImageSource", "Microsoft.Maui.Controls.ImageSource" };

	public override string Convert(string value, BaseNode node, ITypeSymbol toType, SourceGenContext context, LocalVariable? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;
		// IMPORTANT! Update ImageSourceDesignTypeConverter.IsValid if making changes here
		if (!string.IsNullOrEmpty(value))
		{
			value = value.Trim();

			return Uri.TryCreate(value, UriKind.Absolute, out Uri uri) && uri.Scheme != "file" ?
				$"global::Microsoft.Maui.Controls.ImageSource.FromUri(new global::System.Uri(\"{uri}\"))" : $"global::Microsoft.Maui.Controls.ImageSource.FromFile(\"{value}\")";
		}

		ReportConversionFailed(context, xmlLineInfo, value, toType, Descriptors.ConversionFailed);
		return "default";
	}
}