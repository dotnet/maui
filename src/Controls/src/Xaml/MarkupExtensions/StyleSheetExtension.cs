using System;
using System.IO;
using System.Reflection;
using System.Xml;
using Microsoft.Maui.Controls.StyleSheets;

namespace Microsoft.Maui.Controls.Xaml
{
	[ContentProperty(nameof(Style))]
	[ProvideCompiled("Microsoft.Maui.Controls.XamlC.StyleSheetProvider")]
	[RequireService([typeof(IXmlLineInfoProvider), typeof(IRootObjectProvider)])]
	public sealed class StyleSheetExtension : IValueProvider
	{
		public string Style { get; set; }

		[System.ComponentModel.TypeConverter(typeof(UriTypeConverter))]
		public Uri Source { get; set; }

		object IValueProvider.ProvideValue(IServiceProvider serviceProvider)
		{
			IXmlLineInfo lineInfo;

			if (!string.IsNullOrEmpty(Style) && Source != null)
				throw new XamlParseException($"StyleSheet cannot have both a Source and a content", serviceProvider);

			if (Source != null)
			{
				lineInfo = (serviceProvider.GetService(typeof(IXmlLineInfoProvider)) as IXmlLineInfoProvider)?.XmlLineInfo;
				if (Source.IsAbsoluteUri)
					throw new XamlParseException($"Source only accepts Relative URIs", lineInfo);

				var rootObjectType = (serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider)?.RootObject.GetType();
				if (rootObjectType == null)
					return null;
				var rootTargetPath = XamlResourceIdAttribute.GetPathForType(rootObjectType);
				var resourcePath = ResourceDictionary.RDSourceTypeConverter.GetResourcePath(Source, rootTargetPath);
				var assembly = rootObjectType.Assembly;

				return StyleSheet.FromResource(resourcePath, assembly, lineInfo);
			}

			if (!string.IsNullOrEmpty(Style))
			{
				using (var reader = new StringReader(Style))
					return StyleSheet.FromReader(reader);
			}

			throw new XamlParseException($"StyleSheet require either a Source or a content", serviceProvider);
		}
	}
}