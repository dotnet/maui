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

			// The outgoing root as a picture above everything, while the new root goes in underneath it.
			picture.Frame = uiWindow.Bounds;
			uiWindow.AddSubview(picture);
			window.Page = page;

			var incoming = uiWindow.RootViewController?.View;
			incoming?.LayoutIfNeeded();

			var width = (double)uiWindow.Bounds.Width;
			var height = (double)uiWindow.Bounds.Height;
			var easing = PageTransitionCurves.For(transition);
			var flowDirection = request.FlowDirection;
			var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

			void Apply(double progress)
			{
				if (incoming is not null)
				{
					Pose(incoming, PageTransitionCurves.GetRootPose(transition, true, progress, flowDirection), width, height);
				}

				Pose(picture, PageTransitionCurves.GetRootPose(transition, false, progress, flowDirection), width, height);
			}

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
					incoming.Transform = CGAffineTransform.MakeIdentity();
					incoming.Alpha = 1;
				}

				if (running is not null)
				{
					Untrack(window, running);
				}

				tcs.TrySetResult();
			}

			// A later ReplacePageAsync on this window ends this one first: the root view is shared.
			running = Track(window, Cleanup);

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

		static void Pose(UIView view, PageTransitionCurves.Pose pose, double width, double height)
		{
			var transform = CGAffineTransform.MakeTranslation((nfloat)(pose.TranslationX * width), (nfloat)(pose.TranslationY * height));
			if (pose.Scale != 1)
			{
				transform = CGAffineTransform.Scale(transform, (nfloat)pose.Scale, (nfloat)pose.Scale);
			}

			view.Transform = transform;
			view.Alpha = (nfloat)pose.Opacity;
		}
	}
}
