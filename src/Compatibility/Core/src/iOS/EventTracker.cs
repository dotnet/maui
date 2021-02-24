using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Maui.Controls.Internals;

#if __MOBILE__
using UIKit;
using NativeView = UIKit.UIView;
using NativeGestureRecognizer = UIKit.UIGestureRecognizer;
using NativeGestureRecognizerState = UIKit.UIGestureRecognizerState;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
#else
using AppKit;
using NativeView = AppKit.NSView;
using NativeGestureRecognizer = AppKit.NSGestureRecognizer;
using NativeGestureRecognizerState = AppKit.NSGestureRecognizerState;
using Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific;

namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
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
		DragAndDropDelegate _dragAndDropDelegate;
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
				return ((_renderer?.Element as IGestureController)
							?.CompositeGestureRecognizers as ObservableCollection<IGestureRecognizer>);
			}
		}

		internal void Disconnect()
		{
			if (ElementGestureRecognizers != null)
				ElementGestureRecognizers.CollectionChanged -= _collectionChangedHandler;
		}

		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;

			foreach (var kvp in _gestureRecognizers)
			{
				_handler.RemoveGestureRecognizer(kvp.Value);
#if __MOBILE__
				kvp.Value.ShouldReceiveTouch = null;
#endif
				kvp.Value.Dispose();
			}

			_gestureRecognizers.Clear();

			Disconnect();

			_handler = null;
		}

		public void LoadEvents(NativeView handler)
		{
			if (_disposed)
				throw new ObjectDisposedException(null);

			_handler = handler;
			OnElementChanged(this, new VisualElementChangedEventArgs(null, _renderer.Element));
		}

		static IList<GestureElement> GetChildGestures(
			NativeGestureRecognizer sender,
			WeakReference weakEventTracker, WeakReference weakRecognizer, EventTracker eventTracker, View view)
		{
			if (!weakRecognizer.IsAlive)
				return null;

			if (eventTracker._disposed || view == null)
				return null;

			var originPoint = sender.LocationInView(eventTracker._renderer.NativeView);
			var childGestures = view.GetChildElements(new Point(originPoint.X, originPoint.Y));
			return childGestures;
		}

#if !__MOBILE__
		Action<NSClickGestureRecognizer> CreateRecognizerHandler(WeakReference weakEventTracker, WeakReference weakRecognizer, ClickGestureRecognizer clickRecognizer)
		{
			return new Action<NSClickGestureRecognizer>((sender) =>
			{
				var eventTracker = weakEventTracker.Target as EventTracker;
				var view = eventTracker?._renderer?.Element as View;
				var childGestures = GetChildGestures(sender, weakEventTracker, weakRecognizer, eventTracker, view);
				
				if (childGestures?.GetChildGesturesFor<TapGestureRecognizer>(x => x.NumberOfTapsRequired == (int)sender.NumberOfClicksRequired).Count() > 0)
					return;

				if (weakRecognizer.Target is ClickGestureRecognizer clickGestureRecognizer && view != null)
					clickGestureRecognizer.SendClicked(view, clickRecognizer.Buttons);
			});
		}

		NSGestureProbe CreateTapRecognizerHandler(WeakReference weakEventTracker, WeakReference weakRecognizer)
		{
			return new NSGestureProbe((gesturerecognizer) =>
			{
				var tapGestureRecognizer = weakRecognizer.Target as TapGestureRecognizer;
				var eventTracker = weakEventTracker.Target as EventTracker;
				var view = eventTracker?._renderer?.Element as View;

				var handled = false;
				if (tapGestureRecognizer != null && view != null)
				{
					tapGestureRecognizer.SendTapped(view);
					handled = true;
				}

				return handled;
			});
		}

		Action<NSClickGestureRecognizer> CreateChildRecognizerHandler(WeakReference weakEventTracker, WeakReference weakRecognizer)
		{
			return new Action<NSClickGestureRecognizer>((sender) =>
			{
				var eventTracker = weakEventTracker.Target as EventTracker;
				var view = eventTracker?._renderer?.Element as View;
				var childGestures = GetChildGestures(sender, weakEventTracker, weakRecognizer, eventTracker, view);
				
				var clickGestureRecognizer = ((ChildGestureRecognizer)weakRecognizer.Target).GestureRecognizer as ClickGestureRecognizer;
				var recognizers = childGestures?.GetChildGesturesFor<ClickGestureRecognizer>(x => x.NumberOfClicksRequired == (int)sender.NumberOfClicksRequired);

				foreach (var item in recognizers)
					if (item == clickGestureRecognizer && view != null)
						clickGestureRecognizer.SendClicked(view, clickGestureRecognizer.Buttons);
			});
		}

		NSGestureProbe CreateChildTapRecognizerHandler(WeakReference weakEventTracker, WeakReference weakRecognizer)
		{
			return new NSGestureProbe((gesturerecognizer) =>
			{
				var eventTracker = weakEventTracker.Target as EventTracker;
				var view = eventTracker?._renderer?.Element as View;
				var childGestures = GetChildGestures(gesturerecognizer, weakEventTracker, weakRecognizer, eventTracker, view);

				var tapGestureRecognizer = ((ChildGestureRecognizer)weakRecognizer.Target).GestureRecognizer as TapGestureRecognizer;
				var nativeRecognizer = gesturerecognizer as NSClickGestureRecognizer;
				var recognizers = childGestures?.GetChildGesturesFor<TapGestureRecognizer>(x => x.NumberOfTapsRequired == (int)nativeRecognizer.NumberOfClicksRequired);

				var handled = false;
				foreach (var item in recognizers)
				{
					if (item == tapGestureRecognizer && view != null)
					{
						tapGestureRecognizer.SendTapped(view);
						handled = true;
					}
				}
						
				return handled;
			});
		}
#else

		Action<UITapGestureRecognizer> CreateRecognizerHandler(WeakReference weakEventTracker, WeakReference weakRecognizer, TapGestureRecognizer clickRecognizer)
		{
			return new Action<UITapGestureRecognizer>((sender) =>
			{
				EventTracker eventTracker = weakEventTracker.Target as EventTracker;
				View view = eventTracker?._renderer?.Element as View;

				var childGestures = GetChildGestures(sender, weakEventTracker, weakRecognizer, eventTracker, view);

				if (childGestures?.GetChildGesturesFor<TapGestureRecognizer>(x => x.NumberOfTapsRequired == (int)sender.NumberOfTapsRequired).Count() > 0)
					return;

				if (weakRecognizer.Target is TapGestureRecognizer tapGestureRecognizer && view != null)
					tapGestureRecognizer.SendTapped(view);
			});
		}

		Action<UITapGestureRecognizer> CreateChildRecognizerHandler(WeakReference weakEventTracker, WeakReference weakRecognizer)
		{
			return new Action<UITapGestureRecognizer>((sender) =>
			{
				var eventTracker = weakEventTracker.Target as EventTracker;
				var view = eventTracker?._renderer?.Element as View;

				var childGestures = GetChildGestures(sender, weakEventTracker, weakRecognizer, eventTracker, view);

				var recognizers = childGestures?.GetChildGesturesFor<TapGestureRecognizer>(x => x.NumberOfTapsRequired == (int)sender.NumberOfTapsRequired);

				if(recognizers == null)
					return;

				var tapGestureRecognizer = ((ChildGestureRecognizer)weakRecognizer.Target).GestureRecognizer as TapGestureRecognizer;
				foreach (var item in recognizers)
					if (item == tapGestureRecognizer && view != null)
						tapGestureRecognizer.SendTapped(view);
			});
		}
#endif

		protected virtual NativeGestureRecognizer GetNativeRecognizer(IGestureRecognizer recognizer)
		{
			if (recognizer == null)
				return null;

			var weakRecognizer = new WeakReference(recognizer);
			var weakEventTracker = new WeakReference(this);

			var tapRecognizer = recognizer as TapGestureRecognizer;

#if !__MOBILE__
			if (recognizer is ClickGestureRecognizer clickRecognizer)
			{
				var returnAction = CreateRecognizerHandler(weakEventTracker, weakRecognizer, clickRecognizer);
				var uiRecognizer = CreateClickRecognizer((int)clickRecognizer.Buttons, clickRecognizer.NumberOfClicksRequired, returnAction);
				return uiRecognizer;
			}

			if (tapRecognizer != null)
			{
				var returnAction = CreateTapRecognizerHandler(weakEventTracker, weakRecognizer);
				var uiRecognizer = CreateTapRecognizer(tapRecognizer.NumberOfTapsRequired, returnAction);
				return uiRecognizer;
			}
#else
			if (tapRecognizer != null)
			{
				var returnAction = CreateRecognizerHandler(weakEventTracker, weakRecognizer, tapRecognizer);

				var uiRecognizer = CreateTapRecognizer(tapRecognizer.NumberOfTapsRequired, returnAction);
				return uiRecognizer;
			}

			var swipeRecognizer = recognizer as SwipeGestureRecognizer;
			if (swipeRecognizer != null)
			{
				var returnAction = new Action<SwipeDirection>((direction) =>
				{
					var swipeGestureRecognizer = weakRecognizer.Target as SwipeGestureRecognizer;
					var eventTracker = weakEventTracker.Target as EventTracker;
					var view = eventTracker?._renderer.Element as View;

					if (swipeGestureRecognizer != null && view != null)
						swipeGestureRecognizer.SendSwiped(view, direction);
				});
				var uiRecognizer = CreateSwipeRecognizer(swipeRecognizer.Direction, returnAction, 1);
				return uiRecognizer;
			}
#endif

			if (recognizer is ChildGestureRecognizer childRecognizer)
			{
#if !__MOBILE__
				if (childRecognizer.GestureRecognizer is ClickGestureRecognizer clickChildRecognizer)
				{
					var returnAction = CreateChildRecognizerHandler(weakEventTracker, weakRecognizer);
					var uiRecognizer = CreateClickRecognizer((int)clickChildRecognizer.Buttons, clickChildRecognizer.NumberOfClicksRequired, returnAction);
					return uiRecognizer;
				}

				if (childRecognizer.GestureRecognizer is TapGestureRecognizer tapChildRecognizer)
				{
					var returnAction = CreateChildTapRecognizerHandler(weakEventTracker, weakRecognizer);
					var uiRecognizer = CreateTapRecognizer(tapChildRecognizer.NumberOfTapsRequired, returnAction);
					return uiRecognizer;
				}
#else
				if (childRecognizer.GestureRecognizer is TapGestureRecognizer tapChildRecognizer)
				{
					var returnAction = CreateChildRecognizerHandler(weakEventTracker, weakRecognizer);
					var uiRecognizer = CreateTapRecognizer(tapChildRecognizer.NumberOfTapsRequired, returnAction);
					return uiRecognizer;
				}
#endif
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
						originPoint = UIApplication.SharedApplication.GetKeyWindow().ConvertPointToView(originPoint, eventTracker._renderer.NativeView);
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

			// enable touches to pass through so that underlying scrolling views will still receive the pan
			result.ShouldRecognizeSimultaneously = (g, o) => Application.Current?.OnThisPlatform().GetPanGestureRecognizerShouldRecognizeSimultaneously() ?? false;
			return result;
		}

		UIPinchGestureRecognizer CreatePinchRecognizer(Action<UIPinchGestureRecognizer> action)
		{
			var result = new UIPinchGestureRecognizer(action);
			return result;
		}

		UISwipeGestureRecognizer CreateSwipeRecognizer(SwipeDirection direction, Action<SwipeDirection> action, int numFingers = 1)
		{
			var result = new UISwipeGestureRecognizer();
			result.NumberOfTouchesRequired = (uint)numFingers;
			result.Direction = (UISwipeGestureRecognizerDirection)direction;
			result.ShouldRecognizeSimultaneously = (g, o) => true;
			result.AddTarget(() => action(direction));
			return result;
		}

		UITapGestureRecognizer CreateTapRecognizer(int numTaps, Action<UITapGestureRecognizer> action, int numFingers = 1)
		{
			var result = new UITapGestureRecognizer(action)
			{
				NumberOfTouchesRequired = (uint)numFingers,
				NumberOfTapsRequired = (uint)numTaps,
				ShouldRecognizeSimultaneously = ShouldRecognizeTapsTogether
			};

			return result;
		}
#else
		NativeGestureRecognizer CreateClickRecognizer(int buttonMask, int numberOfClicksRequired, Action<NSClickGestureRecognizer> returnAction)
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

		NSClickGestureRecognizer CreateTapRecognizer(int numTaps, NSGestureProbe action)
		{
			var result = new NSClickGestureRecognizer();

			result.NumberOfClicksRequired = numTaps;
			result.ShouldBegin = action;
			result.ShouldRecognizeSimultaneously = ShouldRecognizeTapsTogether;

			return result;
		}

#endif

		static bool ShouldRecognizeTapsTogether(NativeGestureRecognizer gesture, NativeGestureRecognizer other)
		{
			// If multiple tap gestures are potentially firing (because multiple tap gesture recognizers have been
			// added to the XF Element), we want to allow them to fire simultaneously if they have the same number
			// of taps and touches

#if __MOBILE__
			var tap = gesture as UITapGestureRecognizer;
#else
			var tap = gesture as NSClickGestureRecognizer;
#endif
			if (tap == null)
			{
				return false;
			}

#if __MOBILE__
			var otherTap = other as UITapGestureRecognizer;
#else
			var otherTap = other as NSClickGestureRecognizer;
#endif
			if (otherTap == null)
			{
				return false;
			}

			if (!Equals(tap.View, otherTap.View))
			{
				return false;
			}

#if __MOBILE__
			if (tap.NumberOfTapsRequired != otherTap.NumberOfTapsRequired)
#else
			if (tap.NumberOfClicksRequired != otherTap.NumberOfClicksRequired)
#endif
			{
				return false;
			}

			if (tap.NumberOfTouchesRequired != otherTap.NumberOfTouchesRequired)
			{
				return false;
			}

			return true;
		}

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

#if __MOBILE__
			UIDragInteraction uIDragInteraction = null;
			UIDropInteraction uIDropInteraction = null;

			if (_dragAndDropDelegate != null)
			{
				foreach(var interaction in _renderer.NativeView.Interactions)
				{
					if (interaction is UIDragInteraction uIDrag && uIDrag.Delegate == _dragAndDropDelegate)
						uIDragInteraction = uIDrag;

					if (interaction is UIDropInteraction uiDrop && uiDrop.Delegate == _dragAndDropDelegate)
						uIDropInteraction = uiDrop;
				}
			}

			bool dragFound = false;
			bool dropFound = false;
#endif
			for (int i = 0; i < ElementGestureRecognizers.Count; i++)
			{
				IGestureRecognizer recognizer = ElementGestureRecognizers[i];
				if (_gestureRecognizers.ContainsKey(recognizer))
					continue;

				var nativeRecognizer = GetNativeRecognizer(recognizer);
				if (nativeRecognizer != null && _handler != null)
				{
#if __MOBILE__
					nativeRecognizer.ShouldReceiveTouch = _shouldReceiveTouch;
#endif
					_handler.AddGestureRecognizer(nativeRecognizer);

					_gestureRecognizers[recognizer] = nativeRecognizer;
				}

#if __MOBILE__
				if(Forms.IsiOS11OrNewer && recognizer is DragGestureRecognizer)
				{
					dragFound = true;
					_dragAndDropDelegate = _dragAndDropDelegate ?? new DragAndDropDelegate();
					if (uIDragInteraction == null)
					{
						var interaction = new UIDragInteraction(_dragAndDropDelegate);
						interaction.Enabled = true;
						_renderer.NativeView.AddInteraction(interaction);
					}
				}

				if (Forms.IsiOS11OrNewer && recognizer is DropGestureRecognizer)
				{
					dropFound = true;
					_dragAndDropDelegate = _dragAndDropDelegate ?? new DragAndDropDelegate();
					if (uIDropInteraction == null)
					{
						var interaction = new UIDropInteraction(_dragAndDropDelegate);
						_renderer.NativeView.AddInteraction(interaction);
					}
				}
#endif
			}

#if __MOBILE__
			if (!dragFound && uIDragInteraction != null)
				_renderer.NativeView.RemoveInteraction(uIDragInteraction);

			if (!dropFound && uIDropInteraction != null)
				_renderer.NativeView.RemoveInteraction(uIDropInteraction);
#endif

			var toRemove = _gestureRecognizers.Keys.Where(key => !ElementGestureRecognizers.Contains(key)).ToArray();

			for (int i = 0; i < toRemove.Length; i++)
			{
				IGestureRecognizer gestureRecognizer = toRemove[i];
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

			if (touch.View.IsDescendantOfView(_renderer.NativeView) &&
				(touch.View.GestureRecognizers?.Length > 0 || _renderer.NativeView.GestureRecognizers?.Length > 0))
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
