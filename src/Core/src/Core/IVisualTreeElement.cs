using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Maui
{
	public interface IVisualTreeElement
	{
		IReadOnlyList<IVisualTreeElement> GetVisualChildren();

		IVisualTreeElement? GetVisualParent();
	}
}