using System.Xml;

namespace Xamarin.Forms.Internals
{
	public interface INameScope
	{
		object FindByName(string name);
		void RegisterName(string name, object scopedElement);
		void RegisterName(string name, object scopedElement, IXmlLineInfo xmlLineInfo);
		void UnregisterName(string name);
	}
}