using System;
using System.IO;
using System.Reflection;
using Xamarin.Forms;
using System.Xml;

[assembly:Dependency(typeof(Xamarin.Forms.Xaml.ResourcesLoader))]
namespace Xamarin.Forms.Xaml
{
	class ResourcesLoader : IResourcesLoader
	{
		public ResourceDictionary CreateResourceDictionary(string resourcePath, Assembly assembly, IXmlLineInfo lineInfo)
		{
			var resourceId = XamlResourceIdAttribute.GetResourceIdForPath(assembly, resourcePath);
			if (resourceId == null)
				throw new XamlParseException($"Resource '{resourcePath}' not found.", lineInfo);

			var alternateResource = Xamarin.Forms.Internals.ResourceLoader.ResourceProvider?.Invoke(resourcePath);
			if (alternateResource != null) {
				var rd = new ResourceDictionary();
				rd.LoadFromXaml(alternateResource);
				return rd;
			}

			using (var stream = assembly.GetManifestResourceStream(resourceId)) {
				if (stream == null)
					throw new XamlParseException($"No resource found for '{resourceId}'.", lineInfo);
				using (var reader = new StreamReader(stream)) {
					var rd = new ResourceDictionary();
					rd.LoadFromXaml(reader.ReadToEnd());
					return rd;
				}
			}
		}
	}
}