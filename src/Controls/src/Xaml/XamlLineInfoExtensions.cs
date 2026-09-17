using System.Xml;

namespace Microsoft.Maui.Controls.Xaml
{
	static class XamlLineInfoExtensions
	{
		public static IXmlLineInfo ToXmlLineInfo(this XamlLineInfo lineInfo)
			=> lineInfo == null ? null : new XmlLineInfoAdapter(lineInfo);

		sealed class XmlLineInfoAdapter : IXmlLineInfo
		{
			readonly XamlLineInfo _lineInfo;

			public XmlLineInfoAdapter(XamlLineInfo lineInfo) => _lineInfo = lineInfo;

			public bool HasLineInfo() => _lineInfo.HasLineInfo();
			public int LineNumber => _lineInfo.LineNumber;
			public int LinePosition => _lineInfo.LinePosition;
		}
	}
}
