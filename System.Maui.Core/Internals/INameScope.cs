using System;
using System.ComponentModel;
using System.Xml;

namespace System.Maui.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface INameScope
	{
		object FindByName(string name);
		void RegisterName(string name, object scopedElement);
	}
}