#nullable enable
using System;
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

			var transitionDelegate = Install(wrapper);
			transitionDelegate.Present = resolved.Platform;
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
				transitionDelegate = Install(wrapper);
				transitionDelegate.Dismiss = resolved.Platform;
			}

			return animated;
		}

		// Wraps whatever transitioning delegate the wrapper already has (a custom page handler's, which the modal wrapper
		// copies), and forwards to it whenever MAUI has nothing of its own to run.
		static ModalTransitionDelegate Install(UIViewController wrapper)
		{
			var transitionDelegate = s_delegates.GetValue(wrapper, static _ => new ModalTransitionDelegate());
			if (!ReferenceEquals(wrapper.TransitioningDelegate, transitionDelegate))
			{
				transitionDelegate.Inner = wrapper.TransitioningDelegate;
				wrapper.TransitioningDelegate = transitionDelegate;
			}

			return transitionDelegate;
		}

		sealed class ModalTransitionDelegate : UIViewControllerTransitioningDelegate
		{
			public PlatformNavigationTransition? Present { get; set; }

			public PlatformNavigationTransition? Dismiss { get; set; }

			// Weak: the page's own controller keeps the original delegate (the modal wrapper only copied it).
			WeakReference<IUIViewControllerTransitioningDelegate>? _inner;

			public IUIViewControllerTransitioningDelegate? Inner
			{
				get => _inner is not null && _inner.TryGetTarget(out var inner) ? inner : null;
				set => _inner = value is null ? null : new WeakReference<IUIViewControllerTransitioningDelegate>(value);
			}

			public override IUIViewControllerAnimatedTransitioning GetAnimationControllerForPresentedController(UIViewController presented, UIViewController presenting, UIViewController source)
			{
				var transition = Present;
				Present = null;
				if (transition is null)
				{
					return Inner?.GetAnimationControllerForPresentedController(presented, presenting, source)!;
				}

				var own = transition.GetAnimator(out _);
				_own = own is null ? null : new WeakReference<IUIViewControllerAnimatedTransitioning>(own);
				return own!;
			}

			public override IUIViewControllerAnimatedTransitioning GetAnimationControllerForDismissedController(UIViewController dismissed)
			{
				var transition = Dismiss;
				Dismiss = null;
				if (transition is null)
				{
					return Inner?.GetAnimationControllerForDismissedController(dismissed)!;
				}

				var own = transition.GetAnimator(out _);
				_own = own is null ? null : new WeakReference<IUIViewControllerAnimatedTransitioning>(own);
				return own!;
			}

			// An interaction controller only ever comes from the original delegate, for the original delegate's animator.
			WeakReference<IUIViewControllerAnimatedTransitioning>? _own;

			bool IsInners(IUIViewControllerAnimatedTransitioning animator)
				=> _own is null || !_own.TryGetTarget(out var own) || !ReferenceEquals(animator, own);

			public override IUIViewControllerInteractiveTransitioning GetInteractionControllerForPresentation(IUIViewControllerAnimatedTransitioning animator)
				=> (IsInners(animator) ? Inner?.GetInteractionControllerForPresentation(animator) : null)!;

			public override IUIViewControllerInteractiveTransitioning GetInteractionControllerForDismissal(IUIViewControllerAnimatedTransitioning animator)
				=> (IsInners(animator) ? Inner?.GetInteractionControllerForDismissal(animator) : null)!;

			public override UIPresentationController GetPresentationControllerForPresentedViewController(UIViewController presentedViewController, UIViewController? presentingViewController, UIViewController sourceViewController)
				=> Inner?.GetPresentationControllerForPresentedViewController(presentedViewController, presentingViewController!, sourceViewController)!;
		}
	}
}
