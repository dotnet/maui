#nullable enable
using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using AndroidX.Fragment.App;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using AAnimation = Android.Views.Animations.Animation;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Animations
{
	/// <summary>
	/// MAUI's built-in page transitions on Android: fragment animations that pose each page from the shared
	/// transition curves, with heroes flown in the window overlay.
	/// </summary>
	internal sealed class PageTransitionAnimations
	{
		readonly NavigationTransitionRequest _request;
		readonly PageTransition _transition;
		readonly List<(VisualElement From, VisualElement To)> _heroes;

		public PageTransitionAnimations(NavigationTransitionRequest request, PageTransition transition, List<(VisualElement From, VisualElement To)> heroes)
		{
			_request = request;
			_transition = transition;
			_heroes = heroes;
		}

		/// <summary>The animation for the page being shown (<paramref name="enter"/>) or hidden.</summary>
		public AAnimation? Create(bool enter)
		{
			var isBack = _request.IsBack;
			var page = enter ? _request.To : _request.From;
			var animation = new PoseAnimation(_transition, isBack, enter, _request.FlowDirection)
			{
				Duration = (long)_transition.Duration.TotalMilliseconds,
				Interpolator = CreateInterpolator(_transition.Easing),
				FillEnabled = true,
				FillBefore = true,
				FillAfter = true,
			};

			var view = PlatformView(page);
			var container = view?.Parent as ViewGroup;

			// The outgoing page is removed by the transaction, and a container draws views being removed above
			// the others by default. Going forward the incoming page belongs on top, so the outgoing one is
			// drawn first for the length of the transition.
			if (!enter && container is FragmentContainerView fragmentContainer)
			{
				SetDrawDisappearingViewsLast(fragmentContainer, isBack);
				animation.AnimationEnd += (_, _) => SetDrawDisappearingViewsLast(fragmentContainer, true);
			}

			if (!enter && container is not null && _transition.Background is { } background)
			{
				var previous = container.Background;
				container.UpdateBackground((Microsoft.Maui.Graphics.Paint)background);
				animation.AnimationEnd += (_, _) => container.Background = previous;
			}

			if (enter && _heroes.Count > 0 && view is not null)
			{
				HeroFlights.Start(view, _heroes, animation.Duration, animation.Interpolator!);
			}

			return animation;
		}

		// FragmentContainerView.setDrawDisappearingViewsLast isn't in the AndroidX binding.
		static IntPtr s_setDrawDisappearingViewsLast;

		static void SetDrawDisappearingViewsLast(FragmentContainerView container, bool value)
		{
			try
			{
				if (s_setDrawDisappearingViewsLast == IntPtr.Zero)
				{
					var type = global::Android.Runtime.JNIEnv.GetObjectClass(container.Handle);
					s_setDrawDisappearingViewsLast = global::Android.Runtime.JNIEnv.GetMethodID(type, "setDrawDisappearingViewsLast", "(Z)V");
					global::Android.Runtime.JNIEnv.DeleteLocalRef(type);
				}

				global::Android.Runtime.JNIEnv.CallVoidMethod(container.Handle, s_setDrawDisappearingViewsLast, new global::Android.Runtime.JValue(value));
			}
			catch (Java.Lang.Throwable)
			{
				// An AndroidX version without the method keeps its default drawing order.
			}
		}

		internal static IInterpolator CreateInterpolator(Easing? easing)
			=> easing is null
				? new PathInterpolator(0.4f, 0f, 0.2f, 1f)
				: new EasingInterpolator(easing);

		static AView? PlatformView(VisualElement? element)
			=> element?.Handler is IPlatformViewHandler handler ? handler.ContainerView ?? handler.PlatformView : null;

		/// <summary>Poses a page from <see cref="PageTransitionCurves.GetPose"/> at the interpolated time.</summary>
		sealed class PoseAnimation : AAnimation
		{
			readonly PageTransition _transition;
			readonly bool _isBack;
			readonly bool _incoming;
			readonly FlowDirection _flowDirection;
			int _width;
			int _height;

			public PoseAnimation(PageTransition transition, bool isBack, bool incoming, FlowDirection flowDirection)
			{
				_transition = transition;
				_isBack = isBack;
				_incoming = incoming;
				_flowDirection = flowDirection;
			}

			public override void Initialize(int width, int height, int parentWidth, int parentHeight)
			{
				base.Initialize(width, height, parentWidth, parentHeight);
				_width = width;
				_height = height;
			}

			public override bool WillChangeBounds() => false;

			protected override void ApplyTransformation(float interpolatedTime, Transformation? t)
			{
				if (t is null)
				{
					return;
				}

				var pose = PageTransitionCurves.GetPose(_transition, _isBack, _incoming, interpolatedTime, _flowDirection);
				t.Alpha = (float)pose.Opacity;

				if (t.Matrix is { } matrix)
				{
					matrix.Reset();
					if (pose.Scale != 1)
					{
						matrix.SetScale((float)pose.Scale, (float)pose.Scale, _width / 2f, _height / 2f);
					}

					matrix.PostTranslate((float)(pose.TranslationX * _width), (float)(pose.TranslationY * _height));
				}
			}
		}

		sealed class EasingInterpolator : Java.Lang.Object, IInterpolator
		{
			readonly Easing _easing;

			public EasingInterpolator(Easing easing) => _easing = easing;

			public float GetInterpolation(float input) => (float)_easing.Ease(input);
		}

		/// <summary>
		/// Shared elements on Android: pictures of the element on both pages, flown in the window's overlay
		/// from the source to the destination while the real elements are hidden.
		/// </summary>
		static class HeroFlights
		{
			internal static void Start(AView incomingView, List<(VisualElement From, VisualElement To)> heroes, long duration, IInterpolator interpolator)
			{
				if (incomingView.Context?.GetActivity()?.Window?.DecorView is not ViewGroup decor)
				{
					return;
				}

				if (decor.Overlay is null)
				{
					return;
				}

				var overlay = global::Android.Runtime.Extensions.JavaCast<ViewGroupOverlay>(decor.Overlay);

				var flights = new List<(AView Source, VisualElement To, ImageView Picture, int[] Start, float SourceAlpha)>();
				for (int i = 0; i < heroes.Count; i++)
				{
					var (from, to) = heroes[i];
					var source = PlatformView(from);
					var bitmap = source is { Width: > 0, Height: > 0 } ? Snapshot(source) : null;
					if (source is not null && bitmap is not null)
					{
						var start = new int[2];
						source.GetLocationInWindow(start);
						var picture = Picture(decor, overlay, bitmap, start[0], start[1], source.Width, source.Height);
						flights.Add((source, to, picture, start, source.Alpha));
						source.Alpha = 0;
					}
				}

				if (flights.Count == 0)
				{
					return;
				}

				// The incoming page isn't laid out yet: measure the landing spots just before it draws.
				EventHandler<ViewTreeObserver.PreDrawEventArgs>? handler = null;
				handler = (_, e) =>
				{
					incomingView.ViewTreeObserver!.PreDraw -= handler;
					e.Handled = true;
					for (int i = 0; i < flights.Count; i++)
					{
						Fly(decor, overlay, flights[i], duration, interpolator);
					}
				};
				incomingView.ViewTreeObserver!.PreDraw += handler;
			}

			static void Fly(ViewGroup decor, ViewGroupOverlay overlay, (AView Source, VisualElement To, ImageView Picture, int[] Start, float SourceAlpha) flight, long duration, IInterpolator interpolator)
			{
				var (source, to, picture, start, sourceAlpha) = flight;
				var target = PlatformView(to);
				if (target is not { Width: > 0, Height: > 0 })
				{
					// No landing spot: no morph, the page transition runs alone.
					overlay.Remove(picture);
					source.Alpha = sourceAlpha;
					return;
				}

				var end = new int[2];
				target.GetLocationInWindow(end);
				var targetAlpha = target.Alpha;
				var targetBitmap = Snapshot(target);
				ImageView? landing = targetBitmap is null ? null : Picture(decor, overlay, targetBitmap, start[0], start[1], source.Width, source.Height);
				target.Alpha = 0;

				var scaleX = target.Width / (float)source.Width;
				var scaleY = target.Height / (float)source.Height;

				picture.PivotX = 0;
				picture.PivotY = 0;
				if (landing is not null)
				{
					landing.PivotX = 0;
					landing.PivotY = 0;
					landing.Alpha = 0;
					landing.Animate()!
						.TranslationX(end[0] - start[0]).TranslationY(end[1] - start[1]).ScaleX(scaleX).ScaleY(scaleY).Alpha(1)
						.SetDuration(duration).SetInterpolator(interpolator).Start();
				}

				picture.Animate()!
					.TranslationX(end[0] - start[0]).TranslationY(end[1] - start[1]).ScaleX(scaleX).ScaleY(scaleY).Alpha(0)
					.SetDuration(duration).SetInterpolator(interpolator)
					.WithEndAction(new Java.Lang.Runnable(() =>
					{
						overlay.Remove(picture);
						if (landing is not null)
						{
							overlay.Remove(landing);
						}

						source.Alpha = sourceAlpha;
						target.Alpha = targetAlpha;
					}))!
					.Start();
			}

			static Bitmap? Snapshot(AView view)
			{
				var config = Bitmap.Config.Argb8888;
				if (config is null)
				{
					return null;
				}

				var bitmap = Bitmap.CreateBitmap(view.Width, view.Height, config);
				using var canvas = new Canvas(bitmap);
				view.Draw(canvas);
				return bitmap;
			}

			static ImageView Picture(ViewGroup decor, ViewGroupOverlay overlay, Bitmap bitmap, int x, int y, int width, int height)
			{
				var picture = new ImageView(decor.Context);
				picture.SetImageBitmap(bitmap);
				picture.SetScaleType(ImageView.ScaleType.FitXy!);
				picture.Measure(
					AView.MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.Exactly),
					AView.MeasureSpec.MakeMeasureSpec(height, MeasureSpecMode.Exactly));
				picture.Layout(x, y, x + width, y + height);
				overlay.Add(picture);
				return picture;
			}
		}
	}
}
