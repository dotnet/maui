#nullable enable
using System;
using System.Collections.Generic;
using Foundation;
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
	/// swipe-back itself. The driver listens to the system swipe-back recognizers (the edge pan, and on iOS 26+ the content pan) (so every rule already set on
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
		UIPanGestureRecognizer? _content;
		UIPanGestureRecognizer? _driving;
		readonly List<(UIPanGestureRecognizer Recognizer, UIGestureRecognizer.Token Token)> _targets = new();

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
				Observe(edge);
			}

			// iOS 26 adds a swipe-back that starts anywhere on the content.
			if ((OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26))
				&& navigationController.InteractiveContentPopGestureRecognizer is UIPanGestureRecognizer content)
			{
				_content = content;
				Observe(content);
			}
		}

		void Observe(UIPanGestureRecognizer recognizer)
		{
			// The action holds neither the recognizer nor the driver strongly; Detach removes it.
			var weakSelf = new WeakReference<NavigationTransitionDriver>(this);
			var token = recognizer.AddTarget(sender =>
			{
				if (sender is UIPanGestureRecognizer pan && weakSelf.TryGetTarget(out var driver))
				{
					driver.OnPopPan(pan);
				}
			});
			_targets.Add((recognizer, token));
		}

		/// <summary>Stops following the swipe-back recognizers and releases any interaction. Call when the owner is torn down.</summary>
		public void Detach()
		{
			for (int i = 0; i < _targets.Count; i++)
			{
				_targets[i].Recognizer.RemoveTarget(_targets[i].Token);
			}

			_targets.Clear();
			_edge = null;
			_content = null;
			TransitionEnded();
		}

		/// <summary>
		/// Releases the interaction controller of the transition that just finished or was cancelled, so a provider's
		/// controller (and whatever pages it holds) isn't kept until the next navigation.
		/// </summary>
		public void TransitionEnded()
		{
			_interaction = null;
			_customInteraction = null;
			_driving = null;
		}

		static bool IsActive(UIPanGestureRecognizer? recognizer)
			=> recognizer is not null && recognizer.State is UIGestureRecognizerState.Began or UIGestureRecognizerState.Changed;

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
			_driving = null;

			if (NavigationTransitionHooks.Current is not { } hooks
				|| operation is not (UINavigationControllerOperation.Push or UINavigationControllerOperation.Pop))
			{
				return null;
			}

			// Whichever swipe-back started this pop drives it.
			UIPanGestureRecognizer? driving = null;
			if (operation == UINavigationControllerOperation.Pop)
			{
				driving = IsActive(_edge) ? _edge : IsActive(_content) ? _content : null;
			}

			var isInteractive = driving is not null;

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
					_driving = driving;
				}
			}

			return animator;
		}

		/// <summary>
		/// Reports a push or pop made without animation. UIKit doesn't ask the delegate for an animator then, so the
		/// transition is resolved here instead.
		/// </summary>
		public static void OnUnanimatedNavigation(IView? navigationView, UINavigationControllerOperation operation, IView? from, IView? to)
			=> NavigationTransitionHooks.Current?.OnUnanimatedNavigation(navigationView, operation, from, to);

		/// <summary>The interaction controller for the animator just returned, when a swipe-back drives it.</summary>
		public IUIViewControllerInteractiveTransitioning? GetInteractionController()
			=> _customInteraction ?? _interaction;

		void OnPopPan(UIPanGestureRecognizer pan)
		{
			if (_interaction is not { } interaction || !ReferenceEquals(pan, _driving) || pan.View is not { } view)
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
					_driving = null;
					break;

				case UIGestureRecognizerState.Cancelled:
				case UIGestureRecognizerState.Failed:
					interaction.CancelInteractiveTransition();
					_interaction = null;
					_driving = null;
					break;
			}
		}
	}
}
