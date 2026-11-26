#nullable disable
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
		public ShellRouteParameters Parameters { get; set; }
		public bool? CanCancel { get; set; }

		// A per-navigation page transition (PageTransitions.GoToAsync). It applies only while this navigation runs,
		// so a navigation suspended in Navigating doesn't lend it to another.
		public Animations.PageTransition PageTransition { get; set; }

		// Set for the navigation that removes a modal the user dismissed on the platform. Its route was built for the stack
		// at that time, so it only applies while that modal is still on top: once it is gone, or covered, the route is stale.
		public Func<bool> DismissalStillApplies { get; set; }
	}
}
