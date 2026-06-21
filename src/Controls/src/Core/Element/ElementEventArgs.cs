#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Provides data for events pertaining to a single <see cref="Microsoft.Maui.Controls.Element"/>.</summary>
	public class ElementEventArgs : EventArgs
	{
		/// <summary>Constructs and initializes a new instance of the <see cref="Microsoft.Maui.Controls.ElementEventArgs"/> class.</summary>
		/// <param name="element">The element relevant to the event.</param>
		public ElementEventArgs(Element element) => Element = element ?? throw new ArgumentNullException(nameof(element));

		/// <summary>Gets the element relevant to the event.</summary>
		public Element Element { get; private set; }
	}
}