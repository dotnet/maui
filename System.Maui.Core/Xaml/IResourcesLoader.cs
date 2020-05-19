using System;
using System.Reflection;
using System.Xml;
using System.IO;

namespace System.Maui
{
	interface IResourcesLoader
	{
		T CreateFromResource<T>(string resourcePath, Assembly assembly, IXmlLineInfo lineInfo) where T : new();
		string GetResource(string resourcePath, Assembly assembly, object target, IXmlLineInfo lineInfo);
	}
}