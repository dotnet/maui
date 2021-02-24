using System;

namespace Microsoft.Maui.Controls
{
	public class ElementEventArgs : EventArgs
	{
		public ElementEventArgs(Element element) => Element = element ?? throw new ArgumentNullException(nameof(element));

		public Element Element { get; private set; }
	}
}