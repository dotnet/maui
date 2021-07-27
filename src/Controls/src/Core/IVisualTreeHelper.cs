using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IVisualTreeHelper
	{
		IReadOnlyList<object> GetVisualChildren();
	}
}