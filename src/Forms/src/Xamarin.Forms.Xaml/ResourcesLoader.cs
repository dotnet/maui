using System.IO;
using System.Reflection;
using System.Xml;
using Xamarin.Forms;

[assembly: Dependency(typeof(Xamarin.Forms.Xaml.ResourcesLoader))]
namespace Xamarin.Forms.Xaml
{
	class ResourcesLoader : IResourcesLoader
	{
		public T CreateFromResource<T>(string resourcePath, Assembly assembly, IXmlLineInfo lineInfo) where T : new()
		{
			var rd = new T();

			var resourceLoadingResponse = Forms.Internals.ResourceLoader.ResourceProvider2?.Invoke(new Forms.Internals.ResourceLoader.ResourceLoadingQuery
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

		public string GetResource(string resourcePath, Assembly assembly, object target, IXmlLineInfo lineInfo)
		{
			var resourceLoadingResponse = Forms.Internals.ResourceLoader.ResourceProvider2?.Invoke(new Forms.Internals.ResourceLoader.ResourceLoadingQuery
			{
				AssemblyName = assembly.GetName(),
				ResourcePath = resourcePath,
				Instance = target
			});

			var alternateResource = resourceLoadingResponse?.ResourceContent;
			if (alternateResource != null)
				return alternateResource;

			var resourceId = XamlResourceIdAttribute.GetResourceIdForPath(assembly, resourcePath);
			if (resourceId == null)
				throw new XamlParseException($"Resource '{resourcePath}' not found.", lineInfo);

			using (var stream = assembly.GetManifestResourceStream(resourceId))
			{
				if (stream == null)
					throw new XamlParseException($"No resource found for '{resourceId}'.", lineInfo);
				using (var reader = new StreamReader(stream))
					return reader.ReadToEnd();
			}
		}
	}
}