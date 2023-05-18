using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public partial class VisualElement
	{
		partial void HandlePlatformUnloadedLoaded()
		{
			if (Window != null)
			{
				OnLoadedCore();
			}
			else
			{
				OnUnloadedCore();
			}
		}
	}
}
