using System;
using System.Reflection;
using System.Xml;
using System.IO;

namespace Xamarin.Forms
{
	interface IResourcesLoader
	{
		T CreateFromResource<T>(string resourcePath, Assembly assembly, IXmlLineInfo lineInfo) where T : new();
		string GetResource(string resourcePath, Assembly assembly, IXmlLineInfo lineInfo);
	}
}