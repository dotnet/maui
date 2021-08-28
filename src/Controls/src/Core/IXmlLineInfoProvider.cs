#nullable disable
using System.Xml;

namespace Microsoft.Maui.Controls.Xaml
{
	public interface IXmlLineInfoProvider
	{
		IXmlLineInfo XmlLineInfo { get; }
	}
}