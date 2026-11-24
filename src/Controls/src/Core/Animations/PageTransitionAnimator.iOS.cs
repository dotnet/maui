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
				}

				if (fromView is not null)
				{
					fromView.Transform = fromOriginal.Transform;
					fromView.Alpha = fromOriginal.Alpha;
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
		sealed class HeroFlight
		{
			readonly VisualElement _from;
			readonly VisualElement _to;
			readonly UIView _source;
			readonly UIView _target;
			readonly UIView _fromSnapshot;
			readonly UIView _toSnapshot;
			readonly CGRect _start;
			readonly CGRect _end;
			readonly double _fromOpacity;
			readonly double _toOpacity;
			readonly nfloat _sourceAlpha;
			readonly nfloat _targetAlpha;

			/// <summary>Snapshot views currently flying, for tests.</summary>
			internal static int ActiveSnapshots;

			HeroFlight(VisualElement from, VisualElement to, UIView source, UIView target, UIView fromSnapshot, UIView toSnapshot, CGRect start, CGRect end)
			{
				_from = from;
				_to = to;
				_source = source;
				_target = target;
				_fromSnapshot = fromSnapshot;
				_toSnapshot = toSnapshot;
				_start = start;
				_end = end;
				_fromOpacity = from.Opacity;
				_toOpacity = to.Opacity;
				_sourceAlpha = source.Alpha;
				_targetAlpha = target.Alpha;
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
				source.Alpha = 0;
				target.Alpha = 0;
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
				// Unchanged Opacity puts back the exact native alpha (a handler may have set its own); a change made during
				// the flight is applied through the handler's mapper.
				Restore(_from, _source, _fromOpacity, _sourceAlpha);
				Restore(_to, _target, _toOpacity, _targetAlpha);
				_fromSnapshot.RemoveFromSuperview();
				_toSnapshot.RemoveFromSuperview();
				ActiveSnapshots -= 2;
			}

			static void Restore(VisualElement element, UIView view, double opacity, nfloat alpha)
			{
				if (element.Opacity == opacity)
				{
					view.Alpha = alpha;
				}
				else
				{
					element.Handler?.UpdateValue(nameof(IView.Opacity));
				}
			}

			static nfloat Lerp(nfloat from, nfloat to, double progress) => (nfloat)(from + (to - from) * progress);
		}
	}
}
