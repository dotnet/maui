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

			// The outgoing page is removed by the transaction. Despite its name, the argument of
			// setDrawDisappearingViewsLast is "draw disappearing views first" (true by default, beneath the others):
			// forward, the outgoing page stays beneath the incoming one; back, it is drawn on top as it leaves.
			if (!enter && container is FragmentContainerView fragmentContainer)
			{
				SetDrawDisappearingViewsLast(fragmentContainer, drawDisappearingViewsFirst: !isBack);
				animation.AnimationEnd += (_, _) => SetDrawDisappearingViewsLast(fragmentContainer, drawDisappearingViewsFirst: true);
			}

			if (!enter && container is not null && _transition.Background is { } background)
			{
				// Detach the container's own drawable before painting the transition background, so it is neither
				// recolored nor disposed, and put that same, untouched drawable back afterwards.
				var previous = container.Background;
				container.Background = null;
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

		static void SetDrawDisappearingViewsLast(FragmentContainerView container, bool drawDisappearingViewsFirst)
		{
			try
			{
				if (s_setDrawDisappearingViewsLast == IntPtr.Zero)
				{
					var type = global::Android.Runtime.JNIEnv.GetObjectClass(container.Handle);
					s_setDrawDisappearingViewsLast = global::Android.Runtime.JNIEnv.GetMethodID(type, "setDrawDisappearingViewsLast", "(Z)V");
					global::Android.Runtime.JNIEnv.DeleteLocalRef(type);
				}

				global::Android.Runtime.JNIEnv.CallVoidMethod(container.Handle, s_setDrawDisappearingViewsLast, new global::Android.Runtime.JValue(drawDisappearingViewsFirst));
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
				public Flight(VisualElement from, AView source, VisualElement to, ImageView picture, int[] start)
				{
					From = from;
					Source = source;
					To = to;
					Picture = picture;
					Start = start;
				}

				public VisualElement From { get; }
				public AView Source { get; }
				public VisualElement To { get; }
				public ImageView Picture { get; }
				public ImageView? Landing { get; set; }
				public int[] Start { get; }
				public bool Landed { get; set; }
			}

			// Ends the temporary hiding by reapplying the element's own mapped opacity, so a change to its Opacity
			// made during the flight (and any mapper customization) is what shows afterwards.
			static void RestoreOpacity(VisualElement element) => element.Handler?.UpdateValue(nameof(IView.Opacity));

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
						flights.Add(new Flight(from, source, to, picture, start));
						source.Alpha = 0;
					}
				}

				if (flights.Count == 0)
				{
					return;
				}

				new HeroFlights(root, overlay, flights).WaitForIncoming(incomingView, firstSource, duration, interpolator);
			}

			// The incoming page isn't laid out yet: measure the landing spots just before the window next draws. The
			// listener is on the window's own observer, kept so it is removed from the observer it was added to.
			void WaitForIncoming(AView incomingView, AView outgoingView, long duration, IInterpolator interpolator)
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
				Java.Lang.Runnable? timeout = null;

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
						outgoingView.ViewDetachedFromWindow -= onDetached;
					}

					if (timeout is not null)
					{
						_root.RemoveCallbacks(timeout);
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

					// Interruption stays handled while the pictures fly: the incoming page leaving the window (its root
					// replaced, the modal closed) stops them, removes them and puts both elements back.
					EventHandler<AView.ViewDetachedFromWindowEventArgs>? onFlightDetached = null;
					onFlightDetached = (_, _) =>
					{
						incomingView.ViewDetachedFromWindow -= onFlightDetached;
						StopFlights();
					};
					incomingView.ViewDetachedFromWindow += onFlightDetached;
					_stopListening = () => incomingView.ViewDetachedFromWindow -= onFlightDetached;
				};

				// The navigation being interrupted before the flight starts (the outgoing page removed, say by a root page
				// replacement that keeps the window, or the window itself going away): nothing flies, and the pictures and
				// hidden sources are put back.
				onDetached = (_, _) =>
				{
					Unsubscribe();
					Cancel();
				};

				// The incoming page never reaching the window (removed before it drew) cancels too, once the transition
				// would have finished.
				timeout = new Java.Lang.Runnable(() =>
				{
					Unsubscribe();
					Cancel();
				});

				observer.PreDraw += onPreDraw;
				_root.ViewDetachedFromWindow += onDetached;
				outgoingView.ViewDetachedFromWindow += onDetached;
				_root.PostDelayed(timeout, duration + 500);
			}

			Action? _stopListening;

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
					RestoreOpacity(_flights[i].From);
				}
			}

			// Stops flights still in the air: their animations end where they are, and the pictures go.
			void StopFlights()
			{
				for (int i = 0; i < _flights.Count; i++)
				{
					Land(_flights[i], cancelAnimations: true);
				}
			}

			void Land(Flight flight, bool cancelAnimations)
			{
				if (flight.Landed)
				{
					return;
				}

				flight.Landed = true;
				if (cancelAnimations)
				{
					flight.Picture.Animate()?.Cancel();
					flight.Landing?.Animate()?.Cancel();
				}

				_overlay.Remove(flight.Picture);
				if (flight.Landing is not null)
				{
					_overlay.Remove(flight.Landing);
				}

				RestoreOpacity(flight.From);
				RestoreOpacity(flight.To);

				var allLanded = true;
				for (int i = 0; i < _flights.Count && allLanded; i++)
				{
					allLanded = _flights[i].Landed;
				}

				if (allLanded)
				{
					_stopListening?.Invoke();
					_stopListening = null;
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
					Land(flight, cancelAnimations: false);
					return;
				}

				var end = new int[2];
				target.GetLocationInWindow(end);
				var targetBitmap = Snapshot(target);
				ImageView? landing = targetBitmap is null ? null : Picture(_root, _overlay, targetBitmap, start[0], start[1], source.Width, source.Height);
				flight.Landing = landing;
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
					.WithEndAction(new Java.Lang.Runnable(() => Land(flight, cancelAnimations: false)))!
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
