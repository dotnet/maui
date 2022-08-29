#nullable disable
using System.Xml;

namespace Microsoft.Maui.Controls.Xaml
{
	/// <include file="../../docs/Microsoft.Maui.Controls.Xaml/XmlLineInfo.xml" path="Type[@FullName='Microsoft.Maui.Controls.Xaml.XmlLineInfo']/Docs/*" />
	public class XmlLineInfo : IXmlLineInfo
	{
		readonly bool _hasLineInfo;

		/// <include file="../../docs/Microsoft.Maui.Controls.Xaml/XmlLineInfo.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public XmlLineInfo()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Xaml/XmlLineInfo.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public XmlLineInfo(int linenumber, int lineposition)
		{
			_hasLineInfo = true;
			LineNumber = linenumber;
			LinePosition = lineposition;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Xaml/XmlLineInfo.xml" path="//Member[@MemberName='HasLineInfo']/Docs/*" />
		public bool HasLineInfo()
		{
			return _hasLineInfo;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls.Xaml/XmlLineInfo.xml" path="//Member[@MemberName='LineNumber']/Docs/*" />
		public int LineNumber { get; }

		/// <include file="../../docs/Microsoft.Maui.Controls.Xaml/XmlLineInfo.xml" path="//Member[@MemberName='LinePosition']/Docs/*" />
		public int LinePosition { get; }
	}
}