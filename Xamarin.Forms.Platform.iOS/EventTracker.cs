using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

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
		UITouchEventArgs _shouldReceive;
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
#if __MOBILE__
			_shouldReceive = (r, t) => t.View is IVisualElementRenderer;
#endif
			_handler = handler;
			OnElementChanged(this, new VisualElementChangedEventArgs(null, _renderer.Element));
		}

		protected virtual NativeGestureRecognizer GetNativeRecognizer(IGestureRecognizer recognizer)
		{
			if (recognizer == null)
				return null;

			var weakRecognizer = new WeakReference(recognizer);
			var weakEventTracker = new WeakReference(this);

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
					var pinchGestureRecognizer = weakRecognizer.Target as IPinchGestureController;
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

					var panGestureRecognizer = weakRecognizer.Target as IPanGestureController;
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
			var result = new UITapGestureRecognizer(action);
			result.NumberOfTouchesRequired = (uint)numFingers;
			result.NumberOfTapsRequired = (uint)numTaps;
			return result;
		}
#else
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

			foreach (var recognizer in ElementGestureRecognizers)
			{
				if (_gestureRecognizers.ContainsKey(recognizer))
					continue;

				var nativeRecognizer = GetNativeRecognizer(recognizer);
				if (nativeRecognizer != null)
				{
#if __MOBILE__
					nativeRecognizer.ShouldReceiveTouch = _shouldReceive;
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