#nullable disable
using System.Xml;

namespace Microsoft.Maui.Controls.Xaml
{
	public class XmlLineInfo : IXmlLineInfo
	{
		readonly bool _hasLineInfo;

		public XmlLineInfo()
		{
		}

		public XmlLineInfo(int linenumber, int lineposition)
		{
			_hasLineInfo = true;
			LineNumber = linenumber;
			LinePosition = lineposition;
		}

		public bool HasLineInfo()
		{
			return _hasLineInfo;
		}

		public int LineNumber { get; }

		public int LinePosition { get; }
	}
}