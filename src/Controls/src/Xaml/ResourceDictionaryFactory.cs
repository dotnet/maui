using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Xml;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Controls.Xaml
{
	internal static class ResourceDictionaryFactory
	{
		internal static ResourceDictionary CreateFromSource(string value, Type rootType, IXmlLineInfo lineInfo)
		{
			(value, var assembly) = ResourceDictionary.RDSourceTypeConverter.SplitUriAndAssembly(value, rootType.Assembly);

			var rootTargetPath = XamlResourceIdAttribute.GetPathForType(rootType);
			var resourcePath = ResourceDictionary.RDSourceTypeConverter.GetResourcePath(new Uri(value, UriKind.Relative), rootTargetPath);
			var uri = ResourceDictionary.RDSourceTypeConverter.CombineUriAndAssembly(value, assembly);

			var type = XamlResourceIdAttribute.GetTypeForPath(assembly, resourcePath);
			if (type is not null)
			{
				return ResourceDictionary.FromType(uri, type);
			}

			return ResourceDictionary.FromInstance(uri, CreateFromResource(resourcePath, assembly, lineInfo));

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
}
