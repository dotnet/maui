using System;
using System.Xml;

namespace Xamarin.Forms.Internals
{
	public interface INameScope
	{
		object FindByName(string name);
		void RegisterName(string name, object scopedElement);
		void UnregisterName(string name);
		[Obsolete]void RegisterName(string name, object scopedElement, IXmlLineInfo xmlLineInfo);
	}
}