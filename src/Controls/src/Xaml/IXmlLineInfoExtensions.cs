using System.Xml;

namespace Microsoft.Maui.Controls.Xaml;

static class IXmlLineInfoExtensions
{
	public static IXmlLineInfo Clone(this IXmlLineInfo xmlLineInfo)
	{
		return new XmlLineInfo(xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
	}
}