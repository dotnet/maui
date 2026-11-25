#nullable enable
using System;
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
		static async Task ReplacePlatformAsync(Window window, Page page, NavigationTransitionRequest request, PageTransition transition)
		{
			var activity = (window.Handler?.PlatformView as global::Android.App.Activity) ?? window.Handler?.MauiContext?.Context?.GetActivity();
			var decor = activity?.Window?.DecorView as ViewGroup;
			var content = decor?.FindViewById(global::Android.Resource.Id.Content);
			var config = Bitmap.Config.Argb8888;
			if (decor?.Overlay is null || content is not { Width: > 0, Height: > 0 } || config is null)
			{
				window.Page = page;
				return;
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
			ActivePictures++;

			var width = content.Width;
			var height = content.Height;
			var flowDirection = request.FlowDirection;
			var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
			ValueAnimator? animator = null;
			var finished = false;

			// The content view survives root replacement; whatever the activity configured on it is composed with the
			// motion and restored afterwards.
			var original = NativePose.Of(content);
			RunningTransition? running = null;

			// The transition's background shows behind the moving content: the decor view, which isn't transformed,
			// paints it for the duration, and its own background comes back on every way the replacement ends.
			var hasBackdrop = false;
			global::Android.Graphics.Drawables.Drawable? decorBackground = null;
			if (transition.Background is { } background)
			{
				decorBackground = decor.Background;
				decor.Background = null;
				decor.UpdateBackground((Microsoft.Maui.Graphics.Paint)background);
				hasBackdrop = true;
			}

			void Cleanup()
			{
				if (finished)
				{
					return;
				}

				finished = true;
				overlay.Remove(picture);
				ActivePictures--;
				original.Restore(content);
				if (hasBackdrop)
				{
					decor.Background = decorBackground;
				}

				bitmap.Recycle();
				if (running is not null)
				{
					Untrack(window, running);
				}

				tcs.TrySetResult();
			}

			// This replacement owns the window's root (the content view is shared) before the page is assigned: a
			// replacement started from the assignment's own callbacks finishes this one rather than racing it.
			running = Track(window, page, () =>
			{
				animator?.End();
				Cleanup();
			});

			window.Page = page;

			// A replacement started from another assignment's callbacks is queued behind it: wait until this page is
			// really the window's page before animating its root.
			await Task.WhenAny(WhenAssignedAsync(window, page), tcs.Task).ConfigureAwait(true);
			if (running.IsSuperseded || !ReferenceEquals(window.Page, page))
			{
				// Replaced meanwhile: nothing of this one is left to animate (Cleanup only releases this replacement).
				Cleanup();
				await tcs.Task.ConfigureAwait(true);
				return;
			}

			void Apply(float progress)
			{
				Pose(content, PageTransitionCurves.GetRootPose(transition, true, progress, flowDirection), width, height, original);
				Pose(picture, PageTransitionCurves.GetRootPose(transition, false, progress, flowDirection), width, height, NativePose.Identity);
			}

			animator = ValueAnimator.OfFloat(0f, 1f)!;
			animator.SetDuration((long)transition.Duration.TotalMilliseconds);
			animator.SetInterpolator(PageTransitionAnimations.CreateInterpolator(transition.Easing));
			animator.Update += (_, e) => Apply(e.Animation.AnimatedFraction);
			animator.AnimationEnd += (_, _) => Cleanup();

			Apply(0);
			animator.Start();
			await tcs.Task.ConfigureAwait(true);
		}

		static void Pose(AView view, PageTransitionCurves.Pose pose, int width, int height, NativePose original)
		{
			view.PivotX = width / 2f;
			view.PivotY = height / 2f;
			view.TranslationX = original.TranslationX + (float)(pose.TranslationX * width);
			view.TranslationY = original.TranslationY + (float)(pose.TranslationY * height);
			view.ScaleX = original.ScaleX * (float)pose.Scale;
			view.ScaleY = original.ScaleY * (float)pose.Scale;
			view.Alpha = original.Alpha * (float)pose.Opacity;
		}

		readonly struct NativePose
		{
			NativePose(float translationX, float translationY, float scaleX, float scaleY, float alpha, float pivotX, float pivotY, bool hasPivot)
			{
				TranslationX = translationX;
				TranslationY = translationY;
				ScaleX = scaleX;
				ScaleY = scaleY;
				Alpha = alpha;
				PivotX = pivotX;
				PivotY = pivotY;
				HasPivot = hasPivot;
			}

			public static NativePose Identity => new(0, 0, 1, 1, 1, 0, 0, false);

			public float TranslationX { get; }
			public float TranslationY { get; }
			public float ScaleX { get; }
			public float ScaleY { get; }
			public float Alpha { get; }
			public float PivotX { get; }
			public float PivotY { get; }
			public bool HasPivot { get; }

			public static NativePose Of(AView view)
				=> new(view.TranslationX, view.TranslationY, view.ScaleX, view.ScaleY, view.Alpha, view.PivotX, view.PivotY, !OperatingSystem.IsAndroidVersionAtLeast(28) || view.IsPivotSet);

			public void Restore(AView view)
			{
				view.TranslationX = TranslationX;
				view.TranslationY = TranslationY;
				view.ScaleX = ScaleX;
				view.ScaleY = ScaleY;
				view.Alpha = Alpha;
				if (HasPivot)
				{
					view.PivotX = PivotX;
					view.PivotY = PivotY;
				}
				else if (OperatingSystem.IsAndroidVersionAtLeast(28))
				{
					view.ResetPivot();
				}
			}
		}
	}
}
