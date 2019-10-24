using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Xamarin.Forms.Internals;

#if __MOBILE__
using UIKit;
using NativeView = UIKit.UIView;
using NativeGestureRecognizer = UIKit.UIGestureRecognizer;
using NativeGestureRecognizerState = UIKit.UIGestureRecognizerState;

namespace Xamarin.Forms.Platform.iOS
#else
using AppKit;
using NativeView = AppKit.NSView;
using NativeGestureRecognizer = AppKit.NSGestureRecognizer;
using NativeGestureRecognizerState = AppKit.NSGestureRecognizerState;

namespace Xamarin.Forms.Platform.MacOS
#endif
{
	public class EventTracker : IDisposable
	{
		readonly NotifyCollectionChangedEventHandler _collectionChangedHandler;

		readonly Dictionary<IGestureRecognizer, NativeGestureRecognizer> _gestureRecognizers = new Dictionary<IGestureRecognizer, NativeGestureRecognizer>();

		readonly IVisualElementRenderer _renderer;
		bool _disposed;
		NativeView _handler;

		double _previousScale = 1.0;
#if __MOBILE__
		UITouchEventArgs _shouldReceiveTouch;
#endif

		public EventTracker(IVisualElementRenderer renderer)
		{
			if (renderer == null)
				throw new ArgumentNullException(nameof(renderer));

			_collectionChangedHandler = ModelGestureRecognizersOnCollectionChanged;

			_renderer = renderer;
			_renderer.ElementChanged += OnElementChanged;
		}

		ObservableCollection<IGestureRecognizer> ElementGestureRecognizers
		{
			get
			{
				if (_renderer?.Element is View)
					return ((View)_renderer.Element).GestureRecognizers as ObservableCollection<IGestureRecognizer>;
				return null;
			}
		}

		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;

			foreach (var kvp in _gestureRecognizers)
			{
				_handler.RemoveGestureRecognizer(kvp.Value);
				kvp.Value.Dispose();
			}

			_gestureRecognizers.Clear();

			if (ElementGestureRecognizers != null)
				ElementGestureRecognizers.CollectionChanged -= _collectionChangedHandler;

			_handler = null;
		}

		public void LoadEvents(NativeView handler)
		{
			if (_disposed)
				throw new ObjectDisposedException(null);

			_handler = handler;
			OnElementChanged(this, new VisualElementChangedEventArgs(null, _renderer.Element));
		}

		protected virtual NativeGestureRecognizer GetNativeRecognizer(IGestureRecognizer recognizer)
		{
			if (recognizer == null)
				return null;

			var weakRecognizer = new WeakReference(recognizer);
			var weakEventTracker = new WeakReference(this);

#if !__MOBILE__
			var clickRecognizer = recognizer as ClickGestureRecognizer;
			if (clickRecognizer != null)
			{
				var returnAction = new Action(() =>
				{
					var clickGestureRecognizer = weakRecognizer.Target as ClickGestureRecognizer;
					var eventTracker = weakEventTracker.Target as EventTracker;
					var view = eventTracker?._renderer?.Element as View;

					if (clickGestureRecognizer != null && view != null)
						clickGestureRecognizer.SendClicked(view, clickRecognizer.Buttons);
				});
				var uiRecognizer = CreateClickRecognizer((int)clickRecognizer.Buttons, clickRecognizer.NumberOfClicksRequired, returnAction);
				return uiRecognizer;
			}
#endif
			var tapRecognizer = recognizer as TapGestureRecognizer;
			if (tapRecognizer != null)
			{
				var returnAction = new Action(() =>
				{
					var tapGestureRecognizer = weakRecognizer.Target as TapGestureRecognizer;
					var eventTracker = weakEventTracker.Target as EventTracker;
					var view = eventTracker?._renderer?.Element as View;

					if (tapGestureRecognizer != null && view != null)
						tapGestureRecognizer.SendTapped(view);
				});
				var uiRecognizer = CreateTapRecognizer(tapRecognizer.NumberOfTapsRequired, returnAction);
				return uiRecognizer;
			}

			var pinchRecognizer = recognizer as PinchGestureRecognizer;
			if (pinchRecognizer != null)
			{
				double startingScale = 1;
				var uiRecognizer = CreatePinchRecognizer(r =>
				{
					var pinchGestureRecognizer = weakRecognizer.Target as PinchGestureRecognizer;
					var eventTracker = weakEventTracker.Target as EventTracker;
					var view = eventTracker?._renderer?.Element as View;

					if (pinchGestureRecognizer != null && eventTracker != null && view != null)
					{
						var oldScale = eventTracker._previousScale;
						var originPoint = r.LocationInView(null);
#if __MOBILE__
						originPoint = UIApplication.SharedApplication.KeyWindow.ConvertPointToView(originPoint, eventTracker._renderer.NativeView);
#else
						originPoint = NSApplication.SharedApplication.KeyWindow.ContentView.ConvertPointToView(originPoint, eventTracker._renderer.NativeView);
#endif
						var scaledPoint = new Point(originPoint.X / view.Width, originPoint.Y / view.Height);

						switch (r.State)
						{
							case NativeGestureRecognizerState.Began:
#if __MOBILE__
								if (r.NumberOfTouches < 2)
									return;
#endif
								pinchGestureRecognizer.SendPinchStarted(view, scaledPoint);
								startingScale = view.Scale;
								break;
							case NativeGestureRecognizerState.Changed:
#if __MOBILE__
								if (r.NumberOfTouches < 2 && pinchGestureRecognizer.IsPinching)
								{
									r.State = NativeGestureRecognizerState.Ended;
									pinchGestureRecognizer.SendPinchEnded(view);
									return;
								}
								var scale = r.Scale;
#else
								var scale = r.Magnification;
#endif
								var delta = 1.0;
								var dif = Math.Abs(scale - oldScale) * startingScale;
								if (oldScale < scale)
									delta = 1 + dif;
								if (oldScale > scale)
									delta = 1 - dif;

								pinchGestureRecognizer.SendPinch(view, delta, scaledPoint);
								eventTracker._previousScale = scale;
								break;
							case NativeGestureRecognizerState.Cancelled:
							case NativeGestureRecognizerState.Failed:
								if (pinchGestureRecognizer.IsPinching)
									pinchGestureRecognizer.SendPinchCanceled(view);
								break;
							case NativeGestureRecognizerState.Ended:
								if (pinchGestureRecognizer.IsPinching)
									pinchGestureRecognizer.SendPinchEnded(view);
								eventTracker._previousScale = 1;
								break;
						}
					}
				});
				return uiRecognizer;
			}

			var panRecognizer = recognizer as PanGestureRecognizer;
			if (panRecognizer != null)
			{
				var uiRecognizer = CreatePanRecognizer(panRecognizer.TouchPoints, r =>
				{
					var eventTracker = weakEventTracker.Target as EventTracker;
					var view = eventTracker?._renderer?.Element as View;

					var panGestureRecognizer = weakRecognizer.Target as PanGestureRecognizer;
					if (panGestureRecognizer != null && view != null)
					{
						switch (r.State)
						{
							case NativeGestureRecognizerState.Began:
#if __MOBILE__
								if (r.NumberOfTouches != panRecognizer.TouchPoints)
									return;
#endif
								panGestureRecognizer.SendPanStarted(view, Application.Current.PanGestureId);
								break;
							case NativeGestureRecognizerState.Changed:
#if __MOBILE__
								if (r.NumberOfTouches != panRecognizer.TouchPoints)
								{
									r.State = NativeGestureRecognizerState.Ended;
									panGestureRecognizer.SendPanCompleted(view, Application.Current.PanGestureId);
									Application.Current.PanGestureId++;
									return;
								}
#endif
								var translationInView = r.TranslationInView(_handler);
								panGestureRecognizer.SendPan(view, translationInView.X, translationInView.Y, Application.Current.PanGestureId);
								break;
							case NativeGestureRecognizerState.Cancelled:
							case NativeGestureRecognizerState.Failed:
								panGestureRecognizer.SendPanCanceled(view, Application.Current.PanGestureId);
								Application.Current.PanGestureId++;
								break;
							case NativeGestureRecognizerState.Ended:
#if __MOBILE__
								if (r.NumberOfTouches != panRecognizer.TouchPoints)
								{
									panGestureRecognizer.SendPanCompleted(view, Application.Current.PanGestureId);
									Application.Current.PanGestureId++;
								}
#else
								panGestureRecognizer.SendPanCompleted(view, Application.Current.PanGestureId);
								Application.Current.PanGestureId++;
#endif
								break;
						}
					}
				});
				return uiRecognizer;
			}

			return null;
		}

#if __MOBILE__
		UIPanGestureRecognizer CreatePanRecognizer(int numTouches, Action<UIPanGestureRecognizer> action)
		{
			var result = new UIPanGestureRecognizer(action);
			result.MinimumNumberOfTouches = result.MaximumNumberOfTouches = (uint)numTouches;
			return result;
		}

		UIPinchGestureRecognizer CreatePinchRecognizer(Action<UIPinchGestureRecognizer> action)
		{
			var result = new UIPinchGestureRecognizer(action);
			return result;
		}

		UITapGestureRecognizer CreateTapRecognizer(int numTaps, Action action, int numFingers = 1)
		{
			var result = new UITapGestureRecognizer(action)
			{
				NumberOfTouchesRequired = (uint)numFingers,
				NumberOfTapsRequired = (uint)numTaps,
				ShouldRecognizeSimultaneously = ShouldRecognizeTapsTogether
			};

			return result;
		}

		static bool ShouldRecognizeTapsTogether(NativeGestureRecognizer gesture, NativeGestureRecognizer other)
		{
			// If multiple tap gestures are potentially firing (because multiple tap gesture recognizers have been
			// added to the XF Element), we want to allow them to fire simultaneously if they have the same number
			// of taps and touches

			var tap = gesture as UITapGestureRecognizer;
			if (tap == null)
			{
				return false;
			}

			var otherTap = other as UITapGestureRecognizer;
			if (otherTap == null)
			{
				return false;
			}

			if (!Equals(tap.View, otherTap.View))
			{
				return false;
			}

			if (tap.NumberOfTapsRequired != otherTap.NumberOfTapsRequired)
			{
				return false;
			}
			
			if (tap.NumberOfTouchesRequired != otherTap.NumberOfTouchesRequired)
			{
				return false;
			}

			return true;
		}
#else
		NativeGestureRecognizer CreateClickRecognizer(int buttonMask, int numberOfClicksRequired, Action returnAction)
		{
			var result = new NSClickGestureRecognizer(returnAction);
			result.ButtonMask = (nuint)buttonMask;
			result.NumberOfClicksRequired = numberOfClicksRequired;
			return result;
		}

		NSPanGestureRecognizer CreatePanRecognizer(int numTouches, Action<NSPanGestureRecognizer> action)
		{
			var result = new NSPanGestureRecognizer(action);
			return result;
		}

		NSMagnificationGestureRecognizer CreatePinchRecognizer(Action<NSMagnificationGestureRecognizer> action)
		{
			var result = new NSMagnificationGestureRecognizer(action);
			return result;
		}

		NSClickGestureRecognizer CreateTapRecognizer(int numTaps, Action action)
		{
			var result = new NSClickGestureRecognizer(action);
			result.NumberOfClicksRequired = numTaps;
			return result;
		}
#endif

		void LoadRecognizers()
		{
			if (ElementGestureRecognizers == null)
				return;

#if __MOBILE__
			if (_shouldReceiveTouch == null)
			{
				// Cache this so we don't create a new UITouchEventArgs instance for every recognizer
				_shouldReceiveTouch = ShouldReceiveTouch;
			}
#endif

			foreach (var recognizer in ElementGestureRecognizers)
			{
				if (_gestureRecognizers.ContainsKey(recognizer))
					continue;

				var nativeRecognizer = GetNativeRecognizer(recognizer);
				if (nativeRecognizer != null)
				{
#if __MOBILE__
					nativeRecognizer.ShouldReceiveTouch = _shouldReceiveTouch;
#endif
					_handler.AddGestureRecognizer(nativeRecognizer);

					_gestureRecognizers[recognizer] = nativeRecognizer;
				}
			}

			var toRemove = _gestureRecognizers.Keys.Where(key => !ElementGestureRecognizers.Contains(key)).ToArray();
			foreach (var gestureRecognizer in toRemove)
			{
				var uiRecognizer = _gestureRecognizers[gestureRecognizer];
				_gestureRecognizers.Remove(gestureRecognizer);

				_handler.RemoveGestureRecognizer(uiRecognizer);
				uiRecognizer.Dispose();
			}
		}

#if __MOBILE__
		bool ShouldReceiveTouch(UIGestureRecognizer recognizer, UITouch touch)
		{
			if (touch.View is IVisualElementRenderer)
			{
				return true;
			}

			// If the touch is coming from the UIView our renderer is wrapping (e.g., if it's  
			// wrapping a UIView which already has a gesture recognizer), then we should let it through
			// (This goes for children of that control as well)
			if (_renderer?.NativeView == null)
			{
				return false;
			}
			
			if (touch.View.IsDescendantOfView(_renderer.NativeView) && touch.View.GestureRecognizers?.Length > 0)
			{
				return true;
			}

			return false;
		}
#endif

		void ModelGestureRecognizersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			LoadRecognizers();
		}

		void OnElementChanged(object sender, VisualElementChangedEventArgs e)
		{
			if (e.OldElement != null)
			{
				// unhook
				var oldView = e.OldElement as View;
				if (oldView != null)
				{
					var oldRecognizers = (ObservableCollection<IGestureRecognizer>)oldView.GestureRecognizers;
					oldRecognizers.CollectionChanged -= _collectionChangedHandler;
				}
			}

			if (e.NewElement != null)
			{
				// hook
				if (ElementGestureRecognizers != null)
				{
					ElementGestureRecognizers.CollectionChanged += _collectionChangedHandler;
					LoadRecognizers();
				}
			}
		}
	}
}