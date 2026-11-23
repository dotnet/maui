#nullable enable
using System;
using UIKit;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// Supplies the animator and interaction controller for a <see cref="UINavigationController"/> from
	/// <see cref="NavigationTransitionHooks"/>, and lets the edge swipe-back keep following the finger
	/// with a custom animator.
	/// </summary>
	/// <remarks>
	/// Once a navigation controller's delegate returns an animator of its own, UIKit stops driving the
	/// swipe-back itself. The driver listens to the system edge recognizer (so every rule already set on
	/// it, such as MAUI's back-button behaviour, still decides whether it begins) and feeds its progress to
	/// a percent-driven interaction controller, which scrubs any animator built from UIView animations.
	/// </remarks>
	internal sealed class NavigationTransitionDriver
	{
		const double FinishVelocity = 600;

		readonly WeakReference<UINavigationController> _navigationController;
		UIPercentDrivenInteractiveTransition? _interaction;
		IUIViewControllerInteractiveTransitioning? _customInteraction;
		UIPanGestureRecognizer? _edge;

		public NavigationTransitionDriver(UINavigationController navigationController)
		{
			_navigationController = new WeakReference<UINavigationController>(navigationController);
		}

		/// <summary>
		/// Starts following the edge pan. Call once the navigation controller's view is loaded.
		/// </summary>
		public void AttachInteractivePop()
		{
			if (_edge is not null || !_navigationController.TryGetTarget(out var navigationController))
			{
				return;
			}

			if (navigationController.InteractivePopGestureRecognizer is UIPanGestureRecognizer edge)
			{
				_edge = edge;
				var weakSelf = new WeakReference<NavigationTransitionDriver>(this);
				edge.AddTarget(() =>
				{
					if (weakSelf.TryGetTarget(out var driver))
					{
						driver.OnEdgePan(edge);
					}
				});
			}
		}

		/// <summary>
		/// The animator for a push or pop, or <see langword="null"/> to let UIKit animate (and drive the swipe-back) itself.
		/// </summary>
		public IUIViewControllerAnimatedTransitioning? GetAnimationController(
			IView? navigationView,
			UINavigationControllerOperation operation,
			IView? from,
			IView? to)
		{
			_interaction = null;
			_customInteraction = null;

			if (NavigationTransitionHooks.Current is not { } hooks
				|| operation is not (UINavigationControllerOperation.Push or UINavigationControllerOperation.Pop))
			{
				return null;
			}

			var isInteractive = operation == UINavigationControllerOperation.Pop
				&& _edge is { } edge
				&& edge.State is UIGestureRecognizerState.Began or UIGestureRecognizerState.Changed;

			var animator = hooks.GetAnimationController(navigationView, operation, from, to, isInteractive, out var interaction);
			if (animator is not null && isInteractive)
			{
				if (interaction is not null)
				{
					_customInteraction = interaction;
				}
				else
				{
					_interaction = new UIPercentDrivenInteractiveTransition();
				}
			}

			return animator;
		}

		/// <summary>The interaction controller for the animator just returned, when a swipe-back drives it.</summary>
		public IUIViewControllerInteractiveTransitioning? GetInteractionController()
			=> _customInteraction ?? _interaction;

		void OnEdgePan(UIPanGestureRecognizer pan)
		{
			if (_interaction is not { } interaction || pan.View is not { } view)
			{
				return;
			}

			var width = Math.Max(1, (double)view.Bounds.Width);
			var translation = (double)pan.TranslationInView(view).X;

			// The swipe starts at the trailing edge in right-to-left layouts.
			if (view.EffectiveUserInterfaceLayoutDirection == UIUserInterfaceLayoutDirection.RightToLeft)
			{
				translation = -translation;
			}

			var progress = (nfloat)Math.Clamp(translation / width, 0, 1);

			switch (pan.State)
			{
				case UIGestureRecognizerState.Changed:
					interaction.UpdateInteractiveTransition(progress);
					break;

				case UIGestureRecognizerState.Ended:
					var velocity = (double)pan.VelocityInView(view).X;
					if (view.EffectiveUserInterfaceLayoutDirection == UIUserInterfaceLayoutDirection.RightToLeft)
					{
						velocity = -velocity;
					}

					if (progress > 0.5 || velocity > FinishVelocity)
					{
						interaction.FinishInteractiveTransition();
					}
					else
					{
						interaction.CancelInteractiveTransition();
					}

					_interaction = null;
					break;

				case UIGestureRecognizerState.Cancelled:
				case UIGestureRecognizerState.Failed:
					interaction.CancelInteractiveTransition();
					_interaction = null;
					break;
			}
		}
	}
}
