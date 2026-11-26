#nullable enable
using System;
using UIKit;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>Shell tab switches on iOS and Mac Catalyst, through the tab bar controller's animator.</summary>
	internal static class TabPageTransitions
	{
		/// <summary>
		/// The animator for switching <paramref name="tabs"/> from <paramref name="from"/> to <paramref name="to"/>,
		/// or <see langword="null"/> to switch without animation, as UIKit does.
		/// </summary>
		internal static IUIViewControllerAnimatedTransitioning? GetAnimator(
			UITabBarController tabs,
			UIViewController from,
			UIViewController to,
			Func<UIViewController, ShellSection?> sectionForViewController)
		{
			var toSection = sectionForViewController(to);
			if (toSection is null)
			{
				return null;
			}

			var fromSection = sectionForViewController(from);
			var controllers = tabs.ViewControllers ?? Array.Empty<UIViewController>();
			var reverse = Array.IndexOf(controllers, to) < Array.IndexOf(controllers, from);

			// The content shown in the section is the most specific place a TabTransition can be set.
			Element target = toSection.CurrentItem is { } content ? content : toSection;
			var resolved = PageTransitionResolver.ResolveTab(
				target,
				(fromSection as IShellSectionController)?.PresentedPage,
				(toSection as IShellSectionController)?.PresentedPage,
				reverse);

			return resolved.Platform.GetAnimator(out _);
		}
	}
}
