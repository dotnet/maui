using System;

namespace Xamarin.Forms
{
	public class ElementEventArgs : EventArgs
	{
		public ElementEventArgs(Element element) => Element = element ?? throw new ArgumentNullException(nameof(element));

		public Element Element { get; private set; }
	}
}