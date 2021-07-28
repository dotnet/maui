using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Maui
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IVisualTreeHelper
	{
		IReadOnlyList<IElement> GetVisualChildren();
	}
}