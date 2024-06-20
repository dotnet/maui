using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Microsoft.Maui.Controls.Xaml
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	[RequiresUnreferencedCode(TrimmerConstants.XamlRuntimeParsingNotSupportedWarning)]
#if !NETSTANDARD
	[RequiresDynamicCode(TrimmerConstants.XamlRuntimeParsingNotSupportedWarning)]
#endif
	public static class ResourceDictionaryHelpers
	{
		// Used from XamlC generated code
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void LoadFromSource(ResourceDictionary rd, Uri source, string resourcePath, Assembly assembly, IXmlLineInfo lineInfo)
		{
			var sourceInstance = CreateFromResource(resourcePath, assembly, lineInfo);
			rd.SetSource(source, sourceInstance);
		}

		internal static void LoadFromSource(ResourceDictionary rd, string value, Type rootType, IXmlLineInfo lineInfo)
		{
			(value, var assembly) = ResourceDictionary.RDSourceTypeConverter.SplitUriAndAssembly(value, rootType.Assembly);

			var rootTargetPath = XamlResourceIdAttribute.GetPathForType(rootType);
			var resourcePath = ResourceDictionary.RDSourceTypeConverter.GetResourcePath(new Uri(value, UriKind.Relative), rootTargetPath);
			var sourceUri = ResourceDictionary.RDSourceTypeConverter.CombineUriAndAssembly(value, assembly);

			var type = XamlResourceIdAttribute.GetTypeForPath(assembly, resourcePath);
			var sourceInstance = type is not null
				? ResourceDictionary.GetOrCreateInstance(type)
				: CreateFromResource(resourcePath, assembly, lineInfo);

			rd.SetSource(sourceUri, sourceInstance);
		}

		static ResourceDictionary CreateFromResource(string resourcePath, Assembly assembly, IXmlLineInfo lineInfo)
		{
			var rd = new ResourceDictionary();

			var resourceLoadingResponse = Maui.Controls.Internals.ResourceLoader.ResourceProvider2?.Invoke(new Maui.Controls.Internals.ResourceLoader.ResourceLoadingQuery
			{
				AssemblyName = assembly.GetName(),
				ResourcePath = resourcePath,
				Instance = rd,
			});

			var alternateResource = resourceLoadingResponse?.ResourceContent;
			if (alternateResource != null)
			{
				XamlLoader.Load(rd, alternateResource, resourceLoadingResponse.UseDesignProperties);
				return rd;
			}

			var resourceId = XamlResourceIdAttribute.GetResourceIdForPath(assembly, resourcePath);
			if (resourceId == null)
				throw new XamlParseException($"Resource '{resourcePath}' not found.", lineInfo);

			using (var stream = assembly.GetManifestResourceStream(resourceId))
			{
				if (stream == null)
					throw new XamlParseException($"No resource found for '{resourceId}'.", lineInfo);
				using (var reader = new StreamReader(stream))
				{
					rd.LoadFromXaml(reader.ReadToEnd(), assembly);
					return rd;
				}
			}
		}
	}
}
