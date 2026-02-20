using System;
using System.IO;
using System.Reflection;
using System.Xml;
using Microsoft.Maui.Controls.StyleSheets;

namespace Microsoft.Maui.Controls.Xaml
{
	/// <summary>
	/// Provides a XAML markup extension that loads a CSS style sheet from a source or inline content.
	/// </summary>
	[ContentProperty(nameof(Style))]
	[ProvideCompiled("Microsoft.Maui.Controls.XamlC.StyleSheetProvider")]
	[RequireService([typeof(IXmlLineInfoProvider), typeof(IRootObjectProvider)])]
	public sealed class StyleSheetExtension : IValueProvider
	{
		/// <summary>
		/// Gets or sets the inline CSS style sheet content.
		/// </summary>
		public string Style { get; set; }

		/// <summary>
		/// Gets or sets the URI to the CSS style sheet resource.
		/// </summary>
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