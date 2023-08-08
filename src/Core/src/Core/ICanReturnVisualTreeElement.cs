using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui
{
	interface ICanReturnVisualTreeElement
	{
		IVisualTreeElement? GetElement();
	}
}
