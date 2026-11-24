#nullable enable
using System;
using System.Threading.Tasks;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Controls.Animations
{
	internal static partial class RootPageTransition
	{
		const int Samples = 24;

		static Task ReplacePlatformAsync(Window window, Page page, NavigationTransitionRequest request, PageTransition transition)
		{
			if (window.Handler?.PlatformView is not UIWindow uiWindow || uiWindow.SnapshotView(false) is not { } picture)
			{
				window.Page = page;
				return Task.CompletedTask;
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
			(CGAffineTransform Transform, nfloat Alpha) incomingOriginal = (CGAffineTransform.MakeIdentity(), 1);
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
				if (incoming is not null)
				{
					incoming.Layer.RemoveAllAnimations();
					incoming.Transform = incomingOriginal.Transform;
					incoming.Alpha = incomingOriginal.Alpha;
				}

				if (running is not null)
				{
					Untrack(window, running);
				}

				tcs.TrySetResult();
			}

			// This replacement owns the window's root before the page is assigned: a replacement started from the
			// assignment's own callbacks finishes this one rather than racing it.
			running = Track(window, Cleanup);

			window.Page = page;
			if (running.IsSuperseded)
			{
				return tcs.Task;
			}

			incoming = uiWindow.RootViewController?.View;
			incoming?.LayoutIfNeeded();
			uiWindow.AddSubview(picture);
			uiWindow.BringSubviewToFront(picture);
			if (incoming is not null)
			{
				incomingOriginal = (incoming.Transform, incoming.Alpha);
			}

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

			return tcs.Task;
		}

	}
}
