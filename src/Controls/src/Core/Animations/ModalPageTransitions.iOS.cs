#nullable enable
using System.Runtime.CompilerServices;
using UIKit;
using iOSPage = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// Modal present and dismiss transitions on iOS and Mac Catalyst, through the modal wrapper's
	/// <see cref="UIViewController.TransitioningDelegate"/>.
	/// </summary>
	internal static class ModalPageTransitions
	{
		static readonly ConditionalWeakTable<UIViewController, ModalTransitionDelegate> s_delegates = new();

		/// <summary>
		/// Resolves the presentation of <paramref name="modal"/> over <paramref name="presenter"/> and prepares
		/// <paramref name="wrapper"/> for it. Returns whether the presentation should be animated.
		/// </summary>
		internal static bool PreparePresent(UIViewController wrapper, Page? presenter, Page modal, bool animated)
		{
			var resolved = PageTransitionResolver.Resolve(NavigationTransitionKind.PushModal, presenter, modal, animated);
			if (resolved.IsPlatformDefault)
			{
				return animated;
			}

			if (resolved.IsSuppressed)
			{
				return false;
			}

			// A custom transition shows the presenter underneath (a fade or a scale has something to show),
			// unless the page chose its own presentation style.
			if (!modal.IsSet(iOSPage.ModalPresentationStyleProperty))
			{
				wrapper.ModalPresentationStyle = UIModalPresentationStyle.OverFullScreen;
			}

			var transitionDelegate = s_delegates.GetValue(wrapper, static _ => new ModalTransitionDelegate());
			transitionDelegate.Present = resolved.Platform;
			wrapper.TransitioningDelegate = transitionDelegate;
			return true;
		}

		/// <summary>
		/// Resolves the dismissal of <paramref name="modal"/> back to <paramref name="presenter"/>. Returns whether
		/// the dismissal should be animated.
		/// </summary>
		internal static bool PrepareDismiss(UIViewController wrapper, Page modal, Page? presenter, bool animated)
		{
			var resolved = PageTransitionResolver.Resolve(NavigationTransitionKind.PopModal, modal, presenter, animated);
			if (resolved.IsSuppressed)
			{
				return false;
			}

			if (s_delegates.TryGetValue(wrapper, out var transitionDelegate))
			{
				// Platform keeps UIKit's own dismissal of the presentation style the modal was shown with.
				transitionDelegate.Dismiss = resolved.IsPlatformDefault ? null : resolved.Platform;
			}
			else if (!resolved.IsPlatformDefault
				&& wrapper.ModalPresentationStyle is UIModalPresentationStyle.FullScreen or UIModalPresentationStyle.OverFullScreen)
			{
				// A per-navigation transition on a modal presented with the platform's animation.
				transitionDelegate = s_delegates.GetValue(wrapper, static _ => new ModalTransitionDelegate());
				transitionDelegate.Dismiss = resolved.Platform;
				wrapper.TransitioningDelegate = transitionDelegate;
			}

			return animated;
		}

		sealed class ModalTransitionDelegate : UIViewControllerTransitioningDelegate
		{
			public PlatformNavigationTransition? Present { get; set; }

			public PlatformNavigationTransition? Dismiss { get; set; }

			public override IUIViewControllerAnimatedTransitioning GetAnimationControllerForPresentedController(UIViewController presented, UIViewController presenting, UIViewController source)
			{
				var transition = Present;
				Present = null;
				return transition?.GetAnimator(out _)!;
			}

			public override IUIViewControllerAnimatedTransitioning GetAnimationControllerForDismissedController(UIViewController dismissed)
			{
				var transition = Dismiss;
				Dismiss = null;
				return transition?.GetAnimator(out _)!;
			}
		}
	}
}
