#nullable enable
using System;
using UIKit;

namespace Microsoft.Maui.Controls.Animations
{
	public sealed partial class PlatformNavigationTransition
	{
		/// <summary>
		/// A transition run by <paramref name="animator"/>. When a swipe-back drives the navigation
		/// (<see cref="NavigationTransitionRequest.IsInteractive"/>), <paramref name="interaction"/> drives the animator;
		/// left <see langword="null"/>, MAUI drives it with a percent-driven interaction that follows the finger, which
		/// suits any animator built from <c>UIView</c> animations.
		/// </summary>
		public PlatformNavigationTransition(IUIViewControllerAnimatedTransitioning animator, IUIViewControllerInteractiveTransitioning? interaction = null)
		{
			_kind = PlatformNavigationTransitionKind.Custom;
			Animator = animator ?? throw new ArgumentNullException(nameof(animator));
			Interaction = interaction;
		}

		internal IUIViewControllerAnimatedTransitioning? Animator { get; }

		internal IUIViewControllerInteractiveTransitioning? Interaction { get; }

		/// <summary>The animator UIKit runs for this transition, or <see langword="null"/> for UIKit's own.</summary>
		internal IUIViewControllerAnimatedTransitioning? GetAnimator(out IUIViewControllerInteractiveTransitioning? interaction)
		{
			interaction = Interaction;
			if (IsPlatform)
			{
				return null;
			}

			return IsSuppress ? new SuppressedTransitionAnimator() : Animator;
		}

		/// <summary>Swaps the pages with no animation.</summary>
		sealed class SuppressedTransitionAnimator : UIViewControllerAnimatedTransitioning
		{
			public override double TransitionDuration(IUIViewControllerContextTransitioning? transitionContext) => 0;

			public override void AnimateTransition(IUIViewControllerContextTransitioning transitionContext)
			{
				var toViewController = transitionContext.GetViewControllerForKey(UITransitionContext.ToViewControllerKey);
				var toView = transitionContext.GetViewFor(UITransitionContext.ToViewKey);
				if (toView is not null)
				{
					if (toViewController is not null)
					{
						toView.Frame = transitionContext.GetFinalFrameForViewController(toViewController);
					}

					transitionContext.ContainerView.AddSubview(toView);
				}

				transitionContext.CompleteTransition(!transitionContext.TransitionWasCancelled);
			}
		}
	}
}
