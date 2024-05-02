using System;

namespace Microsoft.Maui.Platform
{
	internal interface IUIViewLifeCycleEvents
	{
		const string UnconditionalSuppressMessage =
			"This event isn't meant to cleanup by itself. The subscriber needs to unsubscribe when it knows it no longer cares about this event.";

		event EventHandler MovedToWindow;
	}
}