using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Maui
{
	public interface IVisualTreeHelper
	{
		IReadOnlyList<IElement> GetVisualChildren();
	}
}