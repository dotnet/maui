using System;
using System.ComponentModel;
using System.Xml;

namespace Microsoft.Maui.Controls.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface INameScope
	{
		object FindByName(string name);
		void RegisterName(string name, object scopedElement);
		void UnregisterName(string name);
	}
}