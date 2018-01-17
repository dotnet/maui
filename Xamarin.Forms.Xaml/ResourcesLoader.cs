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
		public T CreateFromResource<T>(string resourcePath, Assembly assembly, IXmlLineInfo lineInfo) where T: new()
		{
			var alternateResource = Xamarin.Forms.Internals.ResourceLoader.ResourceProvider?.Invoke(assembly.GetName(), resourcePath);
			if (alternateResource != null) {
				var rd = new T();
				rd.LoadFromXaml(alternateResource);
				return rd;
			}

			var resourceId = XamlResourceIdAttribute.GetResourceIdForPath(assembly, resourcePath);
			if (resourceId == null)
				throw new XamlParseException($"Resource '{resourcePath}' not found.", lineInfo);

			using (var stream = assembly.GetManifestResourceStream(resourceId)) {
				if (stream == null)
					throw new XamlParseException($"No resource found for '{resourceId}'.", lineInfo);
				using (var reader = new StreamReader(stream)) {
					var rd = new T();
					rd.LoadFromXaml(reader.ReadToEnd());
					return rd;
				}
			}
		}

		public string GetResource(string resourcePath, Assembly assembly, IXmlLineInfo lineInfo)
		{
			var alternateResource = Xamarin.Forms.Internals.ResourceLoader.ResourceProvider?.Invoke(assembly.GetName(), resourcePath);
			if (alternateResource != null)
				return alternateResource;

			var resourceId = XamlResourceIdAttribute.GetResourceIdForPath(assembly, resourcePath);
			if (resourceId == null)
				throw new XamlParseException($"Resource '{resourcePath}' not found.", lineInfo);

			using (var stream = assembly.GetManifestResourceStream(resourceId)) {
				if (stream == null)
					throw new XamlParseException($"No resource found for '{resourceId}'.", lineInfo);
				using (var reader = new StreamReader(stream))
					return reader.ReadToEnd();
			}
		}
	}
}