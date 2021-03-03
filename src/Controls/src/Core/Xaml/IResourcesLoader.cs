using System;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Microsoft.Maui.Controls
{
	interface IResourcesLoader
	{
		T CreateFromResource<T>(string resourcePath, Assembly assembly, IXmlLineInfo lineInfo) where T : new();
		string GetResource(string resourcePath, Assembly assembly, object target, IXmlLineInfo lineInfo);
	}
}