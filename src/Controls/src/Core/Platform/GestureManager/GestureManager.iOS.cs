#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;
using NativeGestureRecognizer = UIKit.UIGestureRecognizer;
using NativeGestureRecognizerState = UIKit.UIGestureRecognizerState;
using PlatformView = UIKit.UIView;

namespace Microsoft.Maui.Controls.Platform
{
	class GestureManager : IDisposable
	{
		readonly NotifyCollectionChangedEventHandler _collectionChangedHandler;

		readonly Dictionary<IGestureRecognizer, NativeGestureRecognizer> _gestureRecognizers = new Dictionary<IGestureRecognizer, NativeGestureRecognizer>();

		readonly IPlatformViewHandler _handler;

		bool _disposed;
		PlatformView? _nativeView;
		UIAccessibilityTrait _addedFlags;
		bool? _defaultAccessibilityRespondsToUserInteraction;

		double _previousScale = 1.0;
#if __MOBILE__
		UITouchEventArgs? _shouldReceiveTouch;
		DragAndDropDelegate? _dragAndDropDelegate;
#endif

		public GestureManager(IViewHandler handler)
		{
			if (handler == null)
				throw new ArgumentNullException(nameof(handler));

			_handler = (IPlatformViewHandler)handler;
			_nativeView = _handler.PlatformView;

			if (_nativeView == null)
				throw new ArgumentNullException(nameof(handler.PlatformView));

			_collectionChangedHandler = GestureRecognizersOnCollectionChanged;

			// In XF this was called inside ViewDidLoad
			if (_handler.VirtualView is View view)
				OnElementChanged(this, new VisualElementChangedEventArgs(null, view));
			else
				throw new ArgumentNullException(nameof(handler.VirtualView));
		}

		ObservableCollection<IGestureRecognizer>? ElementGestureRecognizers
		{
			get
			{
				if (_handler?.VirtualView is IGestureController gc &&
					gc.CompositeGestureRecognizers is ObservableCollection<IGestureRecognizer> oc)
					return oc;

				return null;
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
				if (_nativeView != null)
					_nativeView.RemoveGestureRecognizer(kvp.Value);
#if __MOBILE__
				kvp.Value.ShouldReceiveTouch = null;
#endif
				kvp.Value.Dispose();
			}

			_gestureRecognizers.Clear();

			Disconnect();

			_nativeView = null;
		}

		static IList<GestureElement>? GetChildGestures(
			NativeGestureRecognizer sender,
			WeakReference weakEventTracker, WeakReference weakRecognizer, GestureManager? eventTracker, View? view)
		{
			if (!weakRecognizer.IsAlive)
				return null;

			if (eventTracker == null || eventTracker._disposed || view == null)
				return null;

			var originPoint = sender.LocationInView(eventTracker._handler.PlatformView);
			var childGestures = view.GetChildElements(new Point(originPoint.X, originPoint.Y));
			return childGestures;
		}

#if !__MOBILE__
		Action<NSClickGestureRecognizer> CreateRecognizerHandler(WeakReference weakEventTracker, WeakReference weakRecognizer, ClickGestureRecognizer clickRecognizer)
		{
			return new Action<NSClickGestureRecognizer>((sender) =>
			{
				var eventTracker = weakEventTracker.Target as EventTracker;
				var view = eventTracker?._handler?.Element as View;
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
				var view = eventTracker?._handler?.Element as View;

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
				var view = eventTracker?._handler?.Element as View;
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
				var view = eventTracker?._handler?.Element as View;
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
				var eventTracker = weakEventTracker.Target as GestureManager;
				var view = eventTracker?._handler?.VirtualView as View;

				var childGestures = GetChildGestures(sender, weakEventTracker, weakRecognizer, eventTracker, view);

				if (childGestures?.HasChildGesturesFor<TapGestureRecognizer>(x => x.NumberOfTapsRequired == (int)sender.NumberOfTapsRequired) == true)
					return;

				if (weakRecognizer.Target is TapGestureRecognizer tapGestureRecognizer && view != null)
					tapGestureRecognizer.SendTapped(view);
			});
		}

		Action<UITapGestureRecognizer> CreateChildRecognizerHandler(WeakReference weakEventTracker, WeakReference weakRecognizer)
		{
			return new Action<UITapGestureRecognizer>((sender) =>
			{
				var eventTracker = weakEventTracker.Target as GestureManager;
				var view = eventTracker?._handler?.VirtualView as View;

				var childGestures = GetChildGestures(sender, weakEventTracker, weakRecognizer, eventTracker, view);

				var recognizers = childGestures?.GetChildGesturesFor<TapGestureRecognizer>(x => x.NumberOfTapsRequired == (int)sender.NumberOfTapsRequired);

				if (recognizers == null || weakRecognizer.Target == null)
					return;

				var tapGestureRecognizer = ((ChildGestureRecognizer)weakRecognizer.Target).GestureRecognizer as TapGestureRecognizer;
				foreach (var item in recognizers)
					if (item == tapGestureRecognizer && view != null)
						tapGestureRecognizer.SendTapped(view);
			});
		}
#endif

		protected virtual NativeGestureRecognizer? GetPlatformRecognizer(IGestureRecognizer recognizer)
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
					var eventTracker = weakEventTracker.Target as GestureManager;
					var view = eventTracker?._handler.VirtualView as View;

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

			var pinchRecognizer = recognizer as IPinchGestureController;
			if (pinchRecognizer != null)
			{
				double startingScale = 1;
				var uiRecognizer = CreatePinchRecognizer(r =>
				{
					if (weakRecognizer.Target is IPinchGestureController pinchGestureRecognizer &&
						weakEventTracker.Target is GestureManager eventTracker &&
						eventTracker._handler?.VirtualView is View view &&
						UIApplication.SharedApplication.GetKeyWindow() is UIWindow window)
					{
						var oldScale = eventTracker._previousScale;
						var originPoint = r.LocationInView(null);
#if __MOBILE__
						originPoint = window.ConvertPointToView(originPoint, eventTracker._nativeView);
#else
						originPoint = NSApplication.SharedApplication.KeyWindow.ContentView.ConvertPointToView(originPoint, eventTracker._handler.PlatformView);
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
					var eventTracker = weakEventTracker.Target as GestureManager;
					var view = eventTracker?._handler?.VirtualView as View;

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
								panGestureRecognizer.SendPanStarted(view, PanGestureRecognizer.CurrentId.Value);
								break;
							case NativeGestureRecognizerState.Changed:
#if __MOBILE__
								if (r.NumberOfTouches != panRecognizer.TouchPoints)
								{
									r.State = NativeGestureRecognizerState.Ended;
									panGestureRecognizer.SendPanCompleted(view, PanGestureRecognizer.CurrentId.Value);
									PanGestureRecognizer.CurrentId.Increment();
									return;
								}
#endif
								var translationInView = r.TranslationInView(_nativeView);
								panGestureRecognizer.SendPan(view, translationInView.X, translationInView.Y, PanGestureRecognizer.CurrentId.Value);
								break;
							case NativeGestureRecognizerState.Cancelled:
							case NativeGestureRecognizerState.Failed:
								panGestureRecognizer.SendPanCanceled(view, PanGestureRecognizer.CurrentId.Value);
								PanGestureRecognizer.CurrentId.Increment();
								break;
							case NativeGestureRecognizerState.Ended:
#if __MOBILE__
								if (r.NumberOfTouches != panRecognizer.TouchPoints)
								{
									panGestureRecognizer.SendPanCompleted(view, PanGestureRecognizer.CurrentId.Value);
									PanGestureRecognizer.CurrentId.Increment();
								}
#else
								panGestureRecognizer.SendPanCompleted(view, PanGestureRecognizer.CurrentId.Value);
								PanGestureRecognizer.CurrentId.Increment();
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

		// This logic should all be replaced once we implement the "InputTransparent" property
		// https://github.com/dotnet/maui/issues/1190		
		bool? _previousUserInteractionEnabled;
		void CalculateUserInteractionEnabled()
		{
			if (ElementGestureRecognizers == null || _nativeView == null || _handler?.VirtualView == null)
				return;

			bool hasGestureRecognizers = ElementGestureRecognizers.Count > 0;

			// If no gestures have ever been added then don't do anything
			if (!hasGestureRecognizers && _previousUserInteractionEnabled == null)
				return;

			_previousUserInteractionEnabled ??= _nativeView.UserInteractionEnabled;

			if (hasGestureRecognizers)
			{
				_nativeView.UserInteractionEnabled = true;
			}
			else
			{
				_nativeView.UserInteractionEnabled = _previousUserInteractionEnabled.Value;

				// These are the known places where UserInteractionEnabled is modified inside Maui.Core
				// Once we implement "InputTransparent" all of this should just get managed the "InputTransparent" mapper property
				if (_handler.VirtualView is ITextInput)
					_handler.UpdateValue(nameof(ITextInput.IsReadOnly));

				_handler.UpdateValue(nameof(IView.IsEnabled));
				_previousUserInteractionEnabled = null;
			}
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

			CalculateUserInteractionEnabled();
			UIDragInteraction? uIDragInteraction = null;
			UIDropInteraction? uIDropInteraction = null;

			if (_dragAndDropDelegate != null && _nativeView != null)
			{
				foreach (var interaction in _nativeView.Interactions)
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
			if (_nativeView != null &&
				_handler.VirtualView is View v &&
				v.TapGestureRecognizerNeedsDelegate() &&
				(_nativeView.AccessibilityTraits & UIAccessibilityTrait.Button) != UIAccessibilityTrait.Button)
			{
				_nativeView.AccessibilityTraits |= UIAccessibilityTrait.Button;
				_addedFlags |= UIAccessibilityTrait.Button;
				_defaultAccessibilityRespondsToUserInteraction = _nativeView.AccessibilityRespondsToUserInteraction;
				_nativeView.AccessibilityRespondsToUserInteraction = true;
			}

			for (int i = 0; i < ElementGestureRecognizers.Count; i++)
			{
				IGestureRecognizer recognizer = ElementGestureRecognizers[i];

				if (_gestureRecognizers.ContainsKey(recognizer))
					continue;

				var nativeRecognizer = GetPlatformRecognizer(recognizer);

				if (nativeRecognizer != null && _nativeView != null)
				{
#if __MOBILE__
					nativeRecognizer.ShouldReceiveTouch = _shouldReceiveTouch;
#endif
					_nativeView.AddGestureRecognizer(nativeRecognizer);

					_gestureRecognizers[recognizer] = nativeRecognizer;
				}

#if __MOBILE__
				if (PlatformVersion.IsAtLeast(11) && recognizer is DragGestureRecognizer)
				{
					dragFound = true;
					_dragAndDropDelegate = _dragAndDropDelegate ?? new DragAndDropDelegate(_handler);
					if (uIDragInteraction == null && _handler.PlatformView != null)
					{
						var interaction = new UIDragInteraction(_dragAndDropDelegate);
						interaction.Enabled = true;
						_handler.PlatformView.AddInteraction(interaction);
					}
				}

				if (PlatformVersion.IsAtLeast(11) && recognizer is DropGestureRecognizer)
				{
					dropFound = true;
					_dragAndDropDelegate = _dragAndDropDelegate ?? new DragAndDropDelegate(_handler);
					if (uIDropInteraction == null && _handler.PlatformView != null)
					{
						var interaction = new UIDropInteraction(_dragAndDropDelegate);
						_handler.PlatformView.AddInteraction(interaction);
					}
				}
#endif
			}

#if __MOBILE__
			if (!dragFound && uIDragInteraction != null && _handler.PlatformView != null)
				_handler.PlatformView.RemoveInteraction(uIDragInteraction);

			if (!dropFound && uIDropInteraction != null && _handler.PlatformView != null)
				_handler.PlatformView.RemoveInteraction(uIDropInteraction);
#endif

			var toRemove = new List<IGestureRecognizer>();

			foreach (var key in _gestureRecognizers.Keys)
			{
				if (!ElementGestureRecognizers.Contains(key))
					toRemove.Add(key);
			}

			for (int i = 0; i < toRemove.Count; i++)
			{
				IGestureRecognizer gestureRecognizer = toRemove[i];
				var uiRecognizer = _gestureRecognizers[gestureRecognizer];
				_gestureRecognizers.Remove(gestureRecognizer);

				if (_nativeView != null)
					_nativeView.RemoveGestureRecognizer(uiRecognizer);

				uiRecognizer.Dispose();
			}
		}

#if __MOBILE__
		bool ShouldReceiveTouch(UIGestureRecognizer recognizer, UITouch touch)
		{
			if (touch.View == _handler.PlatformView)
			{
				return true;
			}

			// If the touch is coming from the UIView our handler is wrapping (e.g., if it's  
			// wrapping a UIView which already has a gesture recognizer), then we should let it through
			// (This goes for children of that control as well)
			if (_handler?.PlatformView == null)
			{
				return false;
			}

			if (touch.View.IsDescendantOfView(_handler.PlatformView) &&
				(touch.View.GestureRecognizers?.Length > 0 || _handler.PlatformView.GestureRecognizers?.Length > 0))
			{
				return true;
			}

			return false;
		}
#endif

		void GestureRecognizersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			if (_nativeView != null)
			{
				_nativeView.AccessibilityTraits &= ~_addedFlags;

				if (_defaultAccessibilityRespondsToUserInteraction != null)
					_nativeView.AccessibilityRespondsToUserInteraction = _defaultAccessibilityRespondsToUserInteraction.Value;
			}

			_addedFlags = UIAccessibilityTrait.None;
			_defaultAccessibilityRespondsToUserInteraction = null;
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