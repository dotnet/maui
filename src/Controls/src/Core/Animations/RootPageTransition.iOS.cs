#nullable enable
using System;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Controls.Animations
{
	internal static partial class RootPageTransition
	{
		const int Samples = 24;

		static async Task ReplacePlatformAsync(Window window, Page page, NavigationTransitionRequest request, PageTransition transition)
		{
			if (window.Handler?.PlatformView is not UIWindow uiWindow || uiWindow.SnapshotView(false) is not { } picture)
			{
				window.Page = page;
				return;
			}

			// The outgoing root as a picture, laid over the new root once it has replaced the old one (replacing the
			// root view controller rebuilds the window's view hierarchy, so it is added after the assignment).
			picture.Frame = uiWindow.Bounds;

			var width = (double)uiWindow.Bounds.Width;
			var height = (double)uiWindow.Bounds.Height;
			var easing = PageTransitionCurves.For(transition);
			var flowDirection = request.FlowDirection;
			var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
			UIView? incoming = null;
			UIView? backdrop = null;
			(CGAffineTransform Transform, nfloat Alpha) incomingOriginal = (CGAffineTransform.MakeIdentity(), 1);

			// The managed values mapped onto the incoming view (when it is the page's own view, as a NavigationPage's is),
			// and the animations the replacement added to it: only those are removed, never the view's own.
			ManagedViewState incomingManaged = default;
			string[] ownedAnimations = Array.Empty<string>();
			var finished = false;
			RunningTransition? running = null;

			void Cleanup()
			{
				if (finished)
				{
					return;
				}

				finished = true;
				picture.Layer.RemoveAllAnimations();
				picture.RemoveFromSuperview();
				backdrop?.RemoveFromSuperview();
				if (incoming is not null)
				{
					foreach (var key in ownedAnimations)
					{
						incoming.Layer.RemoveAnimation(key);
					}

					// What the view had, then whatever the page changed meanwhile, through its handler.
					incoming.Transform = incomingOriginal.Transform;
					incoming.Alpha = incomingOriginal.Alpha;
					incomingManaged.ReapplyChanged();
				}

				if (running is not null)
				{
					Untrack(window, running);
				}

				tcs.TrySetResult();
			}

			// This replacement owns the window's root before the page is assigned: a replacement started from the
			// assignment's own callbacks finishes this one rather than racing it.
			running = Track(window, page, Cleanup);

			window.Page = page;

			// A replacement started from another assignment's callbacks is queued behind it: wait until this page is
			// really the window's page before taking its native root.
			await Task.WhenAny(WhenAssignedAsync(window, page), tcs.Task).ConfigureAwait(true);
			if (running.IsSuperseded || !ReferenceEquals(window.Page, page))
			{
				// Replaced meanwhile: nothing of this one is left to animate (Cleanup only releases this replacement).
				Cleanup();
				await tcs.Task.ConfigureAwait(true);
				return;
			}

			incoming = uiWindow.RootViewController?.View;
			incoming?.LayoutIfNeeded();

			// The transition's background shows behind the moving root: an untransformed view directly beneath it,
			// removed on every way the replacement ends.
			if (transition.Background is { } background && incoming?.Superview is { } host)
			{
				backdrop = new UIView(host.Bounds) { AutoresizingMask = UIViewAutoresizing.FlexibleDimensions };
				backdrop.UpdateBackground((Microsoft.Maui.Graphics.Paint)background);
				host.InsertSubviewBelow(backdrop, incoming);
			}

			uiWindow.AddSubview(picture);
			uiWindow.BringSubviewToFront(picture);
			if (incoming is not null)
			{
				incomingOriginal = (incoming.Transform, incoming.Alpha);
				incomingManaged = ManagedViewState.Of(PageTransitionAnimator.ElementShownBy(page, incoming));
			}

			var animationsBefore = incoming?.Layer.AnimationKeys ?? Array.Empty<string>();

			void Apply(double progress)
			{
				if (incoming is not null)
				{
					PageTransitionAnimator.ApplyPose(incoming, PageTransitionCurves.GetRootPose(transition, true, progress, flowDirection), width, height, incomingOriginal.Transform, incomingOriginal.Alpha);
				}

				PageTransitionAnimator.ApplyPose(picture, PageTransitionCurves.GetRootPose(transition, false, progress, flowDirection), width, height, CGAffineTransform.MakeIdentity(), 1);
			}

			Apply(0);
			UIView.AnimateKeyframes(
				transition.Duration.TotalSeconds,
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
				_ => Cleanup());

			ownedAnimations = PageTransitionAnimator.AddedAnimations(incoming, animationsBefore);
			await tcs.Task.ConfigureAwait(true);
		}

	}
}
