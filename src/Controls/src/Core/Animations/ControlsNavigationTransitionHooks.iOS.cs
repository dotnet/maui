#nullable enable
using UIKit;

namespace Microsoft.Maui.Controls.Animations
{
	internal sealed partial class ControlsNavigationTransitionHooks
	{
		public IUIViewControllerAnimatedTransitioning? GetAnimationController(
			IView? navigationView,
			UINavigationControllerOperation operation,
			IView? from,
			IView? to,
			bool isInteractive,
			out IUIViewControllerInteractiveTransitioning? interaction)
		{
			var kind = operation == UINavigationControllerOperation.Pop
				? NavigationTransitionKind.Pop
				: NavigationTransitionKind.Push;

			// UIKit only asks for an animator when the navigation is animated.
			var resolved = PageTransitionResolver.Resolve(kind, from as Page, to as Page, animated: true, isInteractive);
			return resolved.Platform.GetAnimator(out interaction);
		}

		public void OnUnanimatedNavigation(IView? navigationView, UINavigationControllerOperation operation, IView? from, IView? to)
		{
			if (operation is not (UINavigationControllerOperation.Push or UINavigationControllerOperation.Pop))
			{
				return;
			}

			var kind = operation == UINavigationControllerOperation.Pop
				? NavigationTransitionKind.Pop
				: NavigationTransitionKind.Push;

			// Consumes per-navigation arguments and records what the page was shown with, as an animated navigation does.
			PageTransitionResolver.Resolve(kind, from as Page, to as Page, animated: false);
		}
	}

	internal static partial class PageTransitionMotion
	{
		private static partial bool IsReducedMotionEnabledPlatform() => UIAccessibility.IsReduceMotionEnabled;
	}
}
