using System;
using System.Reflection;
using System.Xml;

namespace Xamarin.Forms
{
	interface IResourcesLoader
	{
		ResourceDictionary CreateResourceDictionary(string resourceID, Assembly assembly, IXmlLineInfo lineInfo);
	}
}