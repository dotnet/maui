#nullable enable
using System;
using System.Collections.Generic;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// MAUI's built-in page transitions on iOS and Mac Catalyst, as a UIKit animated transition. The motion is
	/// Core Animation keyframes sampled from the transition's curve, so it keeps running while the UI thread is
	/// busy and a percent-driven interaction (the swipe-back) can scrub it.
	/// </summary>
	internal sealed class PageTransitionAnimator : UIViewControllerAnimatedTransitioning
	{
		// Enough samples that any curve looks smooth; Core Animation interpolates linearly between them.
		const int Samples = 24;

		readonly NavigationTransitionRequest _request;
		readonly PageTransition _transition;
		readonly List<(VisualElement From, VisualElement To)> _heroes;

		public PageTransitionAnimator(NavigationTransitionRequest request, PageTransition transition, List<(VisualElement From, VisualElement To)> heroes)
		{
			_request = request;
			_transition = transition;
			_heroes = heroes;
		}

		public override double TransitionDuration(IUIViewControllerContextTransitioning? transitionContext)
			=> _transition.Duration.TotalSeconds;

		public override void AnimateTransition(IUIViewControllerContextTransitioning transitionContext)
		{
			var container = transitionContext.ContainerView;
			var toViewController = transitionContext.GetViewControllerForKey(UITransitionContext.ToViewControllerKey);
			var fromViewController = transitionContext.GetViewControllerForKey(UITransitionContext.FromViewControllerKey);

			// A modal shown over its presenter leaves the presenter out of the container, so one side can be
			// missing: there is no from-view on present and no to-view on dismiss.
			var fromView = transitionContext.GetViewFor(UITransitionContext.FromViewKey);
			var toView = transitionContext.GetViewFor(UITransitionContext.ToViewKey);
			if (fromView is null && toView is null)
			{
				fromView = fromViewController?.View;
				toView = toViewController?.View;
			}

			var isBack = _request.IsBack;
			var flowDirection = _request.FlowDirection;

			UIView? backdrop = null;
			if (_transition.Background is { } background)
			{
				backdrop = new UIView(container.Bounds) { AutoresizingMask = UIViewAutoresizing.FlexibleDimensions };
				backdrop.UpdateBackground((Paint)background);
				container.InsertSubview(backdrop, 0);
			}

			if (toView is not null)
			{
				if (toViewController is not null)
				{
					toView.Frame = transitionContext.GetFinalFrameForViewController(toViewController);
				}

				if (fromView is null || fromView.Superview != container)
				{
					container.AddSubview(toView);
				}
				else if (PageTransitionCurves.IncomingOnTop(isBack))
				{
					container.InsertSubviewAbove(toView, fromView);
				}
				else
				{
					container.InsertSubviewBelow(toView, fromView);
				}

				// Hero frames are measured on the laid-out page, before any pose moves it.
				toView.LayoutIfNeeded();
			}

			// The views' own transform and alpha (a custom page handler can set them): poses are applied on top of them,
			// and they are what the views go back to, whether the transition finishes or is cancelled.
			var toOriginal = toView is null ? default : (toView.Transform, toView.Alpha);
			var fromOriginal = fromView is null ? default : (fromView.Transform, fromView.Alpha);

			// The managed values mapped onto those views (a page's own view is its handler's): a change made while the
			// transition runs is applied again once the captured state has been put back.
			var toManaged = ManagedViewState.Of(ElementShownBy(_request.To, toView));
			var fromManaged = ManagedViewState.Of(ElementShownBy(_request.From, fromView));

			var flights = HeroFlight.Prepare(container, _heroes);
			var width = (double)container.Bounds.Width;
			var height = (double)container.Bounds.Height;
			var easing = _request.IsInteractive ? Easing.Linear : PageTransitionCurves.For(_transition);

			void Apply(double progress)
			{
				if (toView is not null)
				{
					ApplyPose(toView, PageTransitionCurves.GetPose(_transition, isBack, true, progress, flowDirection), width, height, toOriginal.Transform, toOriginal.Alpha);
				}

				if (fromView is not null)
				{
					ApplyPose(fromView, PageTransitionCurves.GetPose(_transition, isBack, false, progress, flowDirection), width, height, fromOriginal.Transform, fromOriginal.Alpha);
				}

				for (int i = 0; i < flights.Count; i++)
				{
					flights[i].Apply(progress);
				}
			}

			void Finish(bool cancelled)
			{
				if (toView is not null)
				{
					toView.Transform = toOriginal.Transform;
					toView.Alpha = toOriginal.Alpha;
					toManaged.ReapplyChanged();
				}

				if (fromView is not null)
				{
					fromView.Transform = fromOriginal.Transform;
					fromView.Alpha = fromOriginal.Alpha;
					fromManaged.ReapplyChanged();
				}

				for (int i = 0; i < flights.Count; i++)
				{
					flights[i].Land();
				}

				backdrop?.RemoveFromSuperview();

				if (cancelled)
				{
					toView?.RemoveFromSuperview();
				}

				transitionContext.CompleteTransition(!cancelled);
			}

			Apply(0);

			var duration = _transition.Duration.TotalSeconds;
			if (duration <= 0)
			{
				Apply(1);
				Finish(transitionContext.TransitionWasCancelled);
				return;
			}

			UIView.AnimateKeyframes(
				duration,
				0,
				UIViewKeyframeAnimationOptions.CalculationModeLinear | (UIViewKeyframeAnimationOptions)(ulong)UIViewAnimationOptions.CurveLinear,
				() =>
				{
					for (int i = 1; i <= Samples; i++)
					{
						var progress = easing.Ease(i / (double)Samples);
						UIView.AddKeyframeWithRelativeStartTime((i - 1) / (double)Samples, 1.0 / Samples, () => Apply(progress));
					}
				},
				_ => Finish(transitionContext.TransitionWasCancelled));
		}

		/// <summary>
		/// <paramref name="page"/> when <paramref name="view"/> is the native view its handler maps its own properties onto,
		/// else <see langword="null"/> (a container MAUI doesn't map, whose captured state is simply put back).
		/// </summary>
		internal static VisualElement? ElementShownBy(Page? page, UIView? view)
		{
			if (page?.Handler is not IPlatformViewHandler handler || view is null)
			{
				return null;
			}

			var mapped = handler.ContainerView ?? handler.PlatformView;
			return ReferenceEquals(mapped, view) ? page : null;
		}

		/// <summary>The animation keys on <paramref name="view"/>'s layer that weren't in <paramref name="before"/>.</summary>
		internal static string[] AddedAnimations(UIView? view, string[] before)
		{
			if (view?.Layer.AnimationKeys is not { } after)
			{
				return Array.Empty<string>();
			}

			var added = new List<string>();
			foreach (var key in after)
			{
				if (Array.IndexOf(before, key) < 0)
				{
					added.Add(key);
				}
			}

			return added.ToArray();
		}

		internal static void ApplyPose(UIView view, PageTransitionCurves.Pose pose, double width, double height, CGAffineTransform original, nfloat originalAlpha)
		{
			var transform = CGAffineTransform.MakeTranslation((nfloat)(pose.TranslationX * width), (nfloat)(pose.TranslationY * height));
			if (pose.Scale != 1)
			{
				transform = CGAffineTransform.Scale(transform, (nfloat)pose.Scale, (nfloat)pose.Scale);
			}

			// The view's own transform first, then the pose.
			view.Transform = CGAffineTransform.Multiply(original, transform);
			view.Alpha = (nfloat)(pose.Opacity * originalAlpha);
		}

		/// <summary>
		/// One shared element: snapshots of the element on both pages flown from the source frame to the
		/// destination frame and crossfaded on the way, while the real elements are hidden underneath.
		/// </summary>
		internal sealed class HeroFlight
		{
			readonly UIView _source;
			readonly UIView _target;
			readonly UIView _fromSnapshot;
			readonly UIView _toSnapshot;
			readonly CGRect _start;
			readonly CGRect _end;
			readonly ManagedViewState _fromManaged;
			readonly ManagedViewState _toManaged;
			readonly bool _sourceHidden;
			readonly bool _targetHidden;

			/// <summary>Snapshot views currently flying, for tests.</summary>
			internal static int ActiveSnapshots;

			HeroFlight(VisualElement from, VisualElement to, UIView source, UIView target, UIView fromSnapshot, UIView toSnapshot, CGRect start, CGRect end)
			{
				_source = source;
				_target = target;
				_fromSnapshot = fromSnapshot;
				_toSnapshot = toSnapshot;
				_start = start;
				_end = end;
				_fromManaged = ManagedViewState.Of(from);
				_toManaged = ManagedViewState.Of(to);
				_sourceHidden = source.Hidden;
				_targetHidden = target.Hidden;
			}

			internal static List<HeroFlight> Prepare(UIView container, List<(VisualElement From, VisualElement To)> heroes)
			{
				var flights = new List<HeroFlight>(heroes.Count);
				for (int i = 0; i < heroes.Count; i++)
				{
					var (from, to) = heroes[i];
					if (PlatformView(from) is { Window: not null } source
						&& PlatformView(to) is { } target
						&& !target.Bounds.IsEmpty)
					{
						var flight = Create(container, from, to, source, target);
						if (flight is not null)
						{
							flights.Add(flight);
						}
					}
				}

				return flights;
			}

			static HeroFlight? Create(UIView container, VisualElement from, VisualElement to, UIView source, UIView target)
			{
				var fromSnapshot = source.SnapshotView(false);
				var toSnapshot = target.SnapshotView(true);
				if (fromSnapshot is null || toSnapshot is null)
				{
					return null;
				}

				var start = source.ConvertRectToView(source.Bounds, container);
				var end = target.ConvertRectToView(target.Bounds, container);
				fromSnapshot.Frame = start;
				toSnapshot.Frame = start;
				toSnapshot.Alpha = 0;
				container.AddSubview(fromSnapshot);
				container.AddSubview(toSnapshot);

				var flight = new HeroFlight(from, to, source, target, fromSnapshot, toSnapshot, start, end);
				ActiveSnapshots += 2;

				// Hidden rather than made transparent: Opacity maps onto alpha, so a change to it during the flight (a
				// binding, say) would show the real element under its snapshot; only a change to IsVisible maps onto Hidden.
				source.Hidden = true;
				target.Hidden = true;
				return flight;
			}

			static UIView? PlatformView(VisualElement element)
				=> element.Handler is IPlatformViewHandler handler ? handler.ContainerView ?? handler.PlatformView : null;

			internal void Apply(double progress)
			{
				var frame = new CGRect(
					Lerp(_start.X, _end.X, progress),
					Lerp(_start.Y, _end.Y, progress),
					Lerp(_start.Width, _end.Width, progress),
					Lerp(_start.Height, _end.Height, progress));

				_fromSnapshot.Frame = frame;
				_toSnapshot.Frame = frame;
				_fromSnapshot.Alpha = (nfloat)(1 - progress);
				_toSnapshot.Alpha = (nfloat)progress;
			}

			internal void Land()
			{
				// Unchanged IsVisible puts back the exact native state (a handler may have set its own); a change made during
				// the flight is applied through the handler's mapper.
				Restore(_fromManaged, _source, _sourceHidden);
				Restore(_toManaged, _target, _targetHidden);
				_fromSnapshot.RemoveFromSuperview();
				_toSnapshot.RemoveFromSuperview();
				ActiveSnapshots -= 2;
			}

			static void Restore(ManagedViewState managed, UIView view, bool hidden)
			{
				if (managed.VisibilityChanged)
				{
					managed.Element?.Handler?.UpdateValue(nameof(IView.Visibility));
				}
				else
				{
					view.Hidden = hidden;
				}
			}

			static nfloat Lerp(nfloat from, nfloat to, double progress) => (nfloat)(from + (to - from) * progress);
		}
	}
}
