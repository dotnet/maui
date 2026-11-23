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
		/// Shared elements on Android: pictures of the element on both pages, flown in the overlay of the window the pages
		/// are in (the activity's, or a modal dialog's) from the source to the destination while the real elements are hidden.
		/// </summary>
		sealed class HeroFlights
		{
			readonly ViewGroup _root;
			readonly ViewGroupOverlay _overlay;
			readonly List<Flight> _flights;
			ViewTreeObserver? _observer;
			bool _started;
			bool _cancelled;

			HeroFlights(ViewGroup root, ViewGroupOverlay overlay, List<Flight> flights)
			{
				_root = root;
				_overlay = overlay;
				_flights = flights;
			}

			sealed class Flight
			{
				public Flight(AView source, VisualElement to, ImageView picture, int[] start, float sourceAlpha)
				{
					Source = source;
					To = to;
					Picture = picture;
					Start = start;
					SourceAlpha = sourceAlpha;
				}

				public AView Source { get; }
				public VisualElement To { get; }
				public ImageView Picture { get; }
				public int[] Start { get; }
				public float SourceAlpha { get; }
			}

			internal static void Start(AView incomingView, List<(VisualElement From, VisualElement To)> heroes, long duration, IInterpolator interpolator)
			{
				// The outgoing page is on screen, so its window is the one both pages are in; locations are in its coordinates.
				AView? firstSource = null;
				for (int i = 0; i < heroes.Count && firstSource is null; i++)
				{
					firstSource = PlatformView(heroes[i].From);
				}

				if (firstSource is not { IsAttachedToWindow: true } || firstSource.RootView is not ViewGroup root || root.Overlay is null)
				{
					return;
				}

				var overlay = global::Android.Runtime.Extensions.JavaCast<ViewGroupOverlay>(root.Overlay);
				var flights = new List<Flight>();
				for (int i = 0; i < heroes.Count; i++)
				{
					var (from, to) = heroes[i];
					var source = PlatformView(from);
					var bitmap = source is { IsAttachedToWindow: true, Width: > 0, Height: > 0 } && ReferenceEquals(source.RootView, root) ? Snapshot(source) : null;
					if (source is not null && bitmap is not null)
					{
						var start = new int[2];
						source.GetLocationInWindow(start);
						var picture = Picture(root, overlay, bitmap, start[0], start[1], source.Width, source.Height);
						flights.Add(new Flight(source, to, picture, start, source.Alpha));
						source.Alpha = 0;
					}
				}

				if (flights.Count == 0)
				{
					return;
				}

				new HeroFlights(root, overlay, flights).WaitForIncoming(incomingView, duration, interpolator);
			}

			// The incoming page isn't laid out yet: measure the landing spots just before the window next draws. The
			// listener is on the window's own observer, kept so it is removed from the observer it was added to.
			void WaitForIncoming(AView incomingView, long duration, IInterpolator interpolator)
			{
				var observer = _root.ViewTreeObserver;
				if (observer is null || !observer.IsAlive)
				{
					Cancel();
					return;
				}

				_observer = observer;
				EventHandler<ViewTreeObserver.PreDrawEventArgs>? onPreDraw = null;
				EventHandler<AView.ViewDetachedFromWindowEventArgs>? onDetached = null;

				void Unsubscribe()
				{
					if (_observer is { IsAlive: true } subscribed && onPreDraw is not null)
					{
						subscribed.PreDraw -= onPreDraw;
					}

					_observer = null;
					if (onDetached is not null)
					{
						_root.ViewDetachedFromWindow -= onDetached;
					}
				}

				onPreDraw = (_, e) =>
				{
					// Still waiting for the incoming page to reach the window.
					if (!incomingView.IsAttachedToWindow)
					{
						e.Handled = true;
						return;
					}

					Unsubscribe();
					e.Handled = true;
					_started = true;
					for (int i = 0; i < _flights.Count; i++)
					{
						Fly(_flights[i], duration, interpolator);
					}
				};

				// The window going away before the flight starts (the root page replaced, the modal closed): nothing flies,
				// and the pictures and hidden sources are put back.
				onDetached = (_, _) =>
				{
					Unsubscribe();
					Cancel();
				};

				observer.PreDraw += onPreDraw;
				_root.ViewDetachedFromWindow += onDetached;
			}

			void Cancel()
			{
				if (_started || _cancelled)
				{
					return;
				}

				_cancelled = true;
				for (int i = 0; i < _flights.Count; i++)
				{
					_overlay.Remove(_flights[i].Picture);
					_flights[i].Source.Alpha = _flights[i].SourceAlpha;
				}
			}

			void Fly(Flight flight, long duration, IInterpolator interpolator)
			{
				var source = flight.Source;
				var picture = flight.Picture;
				var start = flight.Start;
				var target = PlatformView(flight.To);
				if (target is not { IsAttachedToWindow: true, Width: > 0, Height: > 0 } || !ReferenceEquals(target.RootView, _root))
				{
					// No landing spot in this window: no morph, the page transition runs alone.
					_overlay.Remove(picture);
					source.Alpha = flight.SourceAlpha;
					return;
				}

				var end = new int[2];
				target.GetLocationInWindow(end);
				var targetAlpha = target.Alpha;
				var targetBitmap = Snapshot(target);
				ImageView? landing = targetBitmap is null ? null : Picture(_root, _overlay, targetBitmap, start[0], start[1], source.Width, source.Height);
				target.Alpha = 0;

				var scaleX = target.Width / (float)source.Width;
				var scaleY = target.Height / (float)source.Height;
				var overlay = _overlay;

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

						source.Alpha = flight.SourceAlpha;
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

			static ImageView Picture(ViewGroup root, ViewGroupOverlay overlay, Bitmap bitmap, int x, int y, int width, int height)
			{
				var picture = new ImageView(root.Context);
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
