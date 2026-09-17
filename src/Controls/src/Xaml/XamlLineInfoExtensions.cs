using System.Xml;

namespace Microsoft.Maui.Controls.Xaml
{
	static class XamlLineInfoExtensions
	{
		public static IXmlLineInfo ToXmlLineInfo(this IXamlLineInfo lineInfo)
			=> lineInfo == null ? null : new XmlLineInfoAdapter(lineInfo);

		sealed class XmlLineInfoAdapter : IXmlLineInfo
		{
			readonly IXamlLineInfo _lineInfo;

			public XmlLineInfoAdapter(IXamlLineInfo lineInfo) => _lineInfo = lineInfo;

			public bool HasLineInfo() => _lineInfo.HasLineInfo();
			public int LineNumber => _lineInfo.LineNumber;
			public int LinePosition => _lineInfo.LinePosition;
		}
	}
}
