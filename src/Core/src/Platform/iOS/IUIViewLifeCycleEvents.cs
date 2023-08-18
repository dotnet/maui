using System;

namespace Microsoft.Maui.Platform
{
	internal interface IUIViewLifeCycleEvents
	{
		event EventHandler MovedToWindow;
	}
}