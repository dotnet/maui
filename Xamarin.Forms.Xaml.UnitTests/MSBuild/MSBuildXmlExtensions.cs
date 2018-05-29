using System.Xml.Linq;

namespace Xamarin.Forms.Xaml.UnitTests
{
	static class MSBuildXmlExtensions
	{
		static readonly XNamespace ns = XNamespace.Get ("http://schemas.microsoft.com/developer/msbuild/2003");

		public static XElement NewElement (string name) => new XElement (ns + name);

		public static XElement WithAttribute (this XElement element, string name, object value)
		{
			element.SetAttributeValue (name, value);
			return element;
		}

		public static XElement WithValue (this XElement element, object value)
		{
			element.SetValue (value);
			return element;
		}
	}
}
