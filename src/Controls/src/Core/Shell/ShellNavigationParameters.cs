using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	// At somepoint this will be shaped into a public API but for now we are using it
	// to simplify internal navigation parameters
	internal class ShellNavigationParameters
	{
		public ShellNavigatingEventArgs DeferredArgs { get; set; }
		public ShellNavigationState TargetState { get; set; }
		public bool EnableRelativeShellRoutes { get; set; }
		public bool? Animated { get; set; }

		public bool PopAllPagesNotSpecifiedOnTargetState { get; set; }
		// This is used to service Navigation.PushAsync style APIs where the user doesn't use routes at all
		public Page PagePushing { get; set; }
	}
}
