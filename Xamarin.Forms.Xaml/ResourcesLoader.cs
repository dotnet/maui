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
		public ResourceDictionary CreateResourceDictionary(string resourceID, Assembly assembly, IXmlLineInfo lineInfo)
		{
			using (var stream = assembly.GetManifestResourceStream(resourceID)) {
				if (stream == null)
					throw new XamlParseException($"No resource found for '{resourceID}'.", lineInfo);
				using (var reader = new StreamReader(stream)) {
					var rd = new ResourceDictionary();
					rd.LoadFromXaml(reader.ReadToEnd());
					return rd;
				}
			}
		}
	}
}