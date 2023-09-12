using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui
{
	interface IVisualTreeElementProvidable
	{
		IVisualTreeElement? GetElement();
	}
}
