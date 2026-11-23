#nullable enable
using System.Threading.Tasks;
using Android.Animation;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Platform;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Animations
{
	internal static partial class RootPageTransition
	{
		static Task ReplacePlatformAsync(Window window, Page page, NavigationTransitionRequest request, PageTransition transition)
		{
			var activity = (window.Handler?.PlatformView as global::Android.App.Activity) ?? window.Handler?.MauiContext?.Context?.GetActivity();
			var decor = activity?.Window?.DecorView as ViewGroup;
			var content = decor?.FindViewById(global::Android.Resource.Id.Content);
			var config = Bitmap.Config.Argb8888;
			if (decor?.Overlay is null || content is not { Width: > 0, Height: > 0 } || config is null)
			{
				window.Page = page;
				return Task.CompletedTask;
			}

			// The outgoing root as a picture in the window overlay, while the new root goes in underneath it.
			var bitmap = Bitmap.CreateBitmap(content.Width, content.Height, config);
			using (var canvas = new Canvas(bitmap))
			{
				content.Draw(canvas);
			}

			var location = new int[2];
			content.GetLocationInWindow(location);
			var picture = new ImageView(decor.Context);
			picture.SetImageBitmap(bitmap);
			picture.Measure(
				AView.MeasureSpec.MakeMeasureSpec(content.Width, MeasureSpecMode.Exactly),
				AView.MeasureSpec.MakeMeasureSpec(content.Height, MeasureSpecMode.Exactly));
			picture.Layout(location[0], location[1], location[0] + content.Width, location[1] + content.Height);

			var overlay = global::Android.Runtime.Extensions.JavaCast<ViewGroupOverlay>(decor.Overlay);
			overlay.Add(picture);

			var width = content.Width;
			var height = content.Height;
			var flowDirection = request.FlowDirection;
			var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
			ValueAnimator? animator = null;
			var finished = false;
			RunningTransition? running = null;

			void Cleanup()
			{
				if (finished)
				{
					return;
				}

				finished = true;
				overlay.Remove(picture);
				Pose(content, PageTransitionCurves.Pose.Identity, width, height);
				bitmap.Recycle();
				if (running is not null)
				{
					Untrack(window, running);
				}

				tcs.TrySetResult();
			}

			// This replacement owns the window's root (the content view is shared) before the page is assigned: a
			// replacement started from the assignment's own callbacks finishes this one rather than racing it.
			running = Track(window, () =>
			{
				animator?.End();
				Cleanup();
			});

			window.Page = page;
			if (running.IsSuperseded)
			{
				return tcs.Task;
			}

			void Apply(float progress)
			{
				Pose(content, PageTransitionCurves.GetRootPose(transition, true, progress, flowDirection), width, height);
				Pose(picture, PageTransitionCurves.GetRootPose(transition, false, progress, flowDirection), width, height);
			}

			animator = ValueAnimator.OfFloat(0f, 1f)!;
			animator.SetDuration((long)transition.Duration.TotalMilliseconds);
			animator.SetInterpolator(PageTransitionAnimations.CreateInterpolator(transition.Easing));
			animator.Update += (_, e) => Apply(e.Animation.AnimatedFraction);
			animator.AnimationEnd += (_, _) => Cleanup();

			Apply(0);
			animator.Start();
			return tcs.Task;
		}

		static void Pose(AView view, PageTransitionCurves.Pose pose, int width, int height)
		{
			view.PivotX = width / 2f;
			view.PivotY = height / 2f;
			view.TranslationX = (float)(pose.TranslationX * width);
			view.TranslationY = (float)(pose.TranslationY * height);
			view.ScaleX = (float)pose.Scale;
			view.ScaleY = (float)pose.Scale;
			view.Alpha = (float)pose.Opacity;
		}
	}
}
