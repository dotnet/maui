using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Maui
{
	public interface IVisualTreeElement
	{
		/// <summary>Gets a readonly list of the element's visual children.</summary>
		/// <remarks>Unless explicitly defined, an element's visual children will also be its logical children.</remarks>
		/// <returns>A readonly list containing the element's visual children.</returns>
		IReadOnlyList<IVisualTreeElement> GetVisualChildren();

		/// <summary>Gets the element's visual parent.</summary>
		/// <returns>The element's parent.</returns>
		IVisualTreeElement? GetVisualParent();
	}
}