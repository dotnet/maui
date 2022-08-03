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
		PlatformView? _platformView;
		UIAccessibilityTrait _addedFlags;
		bool? _defaultAccessibilityRespondsToUserInteraction;

		double _previousScale = 1.0;
		UITouchEventArgs? _shouldReceiveTouch;
		DragAndDropDelegate? _dragAndDropDelegate;

		public GestureManager(IViewHandler handler)
		{
			if (handler == null)
				throw new ArgumentNullException(nameof(handler));

			_handler = (IPlatformViewHandler)handler;
			_platformView = _handler.PlatformView;

			if (_platformView == null)
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
				if (_platformView != null)
					_platformView.RemoveGestureRecognizer(kvp.Value);
				kvp.Value.ShouldReceiveTouch = null;
				kvp.Value.Dispose();
			}

			_gestureRecognizers.Clear();

			Disconnect();

			_platformView = null;
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

		protected virtual NativeGestureRecognizer? GetPlatformRecognizer(IGestureRecognizer recognizer)
		{
			if (recognizer == null)
				return null;

			var weakRecognizer = new WeakReference(recognizer);
			var weakEventTracker = new WeakReference(this);

			var tapRecognizer = recognizer as TapGestureRecognizer;

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

			if (recognizer is ChildGestureRecognizer childRecognizer)
			{
				if (childRecognizer.GestureRecognizer is TapGestureRecognizer tapChildRecognizer)
				{
					var returnAction = CreateChildRecognizerHandler(weakEventTracker, weakRecognizer);
					var uiRecognizer = CreateTapRecognizer(tapChildRecognizer.NumberOfTapsRequired, returnAction);
					return uiRecognizer;
				}
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
						originPoint = window.ConvertPointToView(originPoint, eventTracker._platformView);

						var scaledPoint = new Point(originPoint.X / view.Width, originPoint.Y / view.Height);

						switch (r.State)
						{
							case NativeGestureRecognizerState.Began:
								if (r.NumberOfTouches < 2)
									return;

								pinchGestureRecognizer.SendPinchStarted(view, scaledPoint);
								startingScale = view.Scale;
								break;
							case NativeGestureRecognizerState.Changed:
								if (r.NumberOfTouches < 2 && pinchGestureRecognizer.IsPinching)
								{
									r.State = NativeGestureRecognizerState.Ended;
									pinchGestureRecognizer.SendPinchEnded(view);
									return;
								}
								var scale = r.Scale;
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
								if (r.NumberOfTouches != panRecognizer.TouchPoints)
									return;
								panGestureRecognizer.SendPanStarted(view, PanGestureRecognizer.CurrentId.Value);
								break;
							case NativeGestureRecognizerState.Changed:
								if (r.NumberOfTouches != panRecognizer.TouchPoints)
								{
									r.State = NativeGestureRecognizerState.Ended;
									panGestureRecognizer.SendPanCompleted(view, PanGestureRecognizer.CurrentId.Value);
									PanGestureRecognizer.CurrentId.Increment();
									return;
								}
								var translationInView = r.TranslationInView(_platformView);
								panGestureRecognizer.SendPan(view, translationInView.X, translationInView.Y, PanGestureRecognizer.CurrentId.Value);
								break;
							case NativeGestureRecognizerState.Cancelled:
							case NativeGestureRecognizerState.Failed:
								panGestureRecognizer.SendPanCanceled(view, PanGestureRecognizer.CurrentId.Value);
								PanGestureRecognizer.CurrentId.Increment();
								break;
							case NativeGestureRecognizerState.Ended:
								if (r.NumberOfTouches != panRecognizer.TouchPoints)
								{
									panGestureRecognizer.SendPanCompleted(view, PanGestureRecognizer.CurrentId.Value);
									PanGestureRecognizer.CurrentId.Increment();
								}
								break;
						}
					}
				});
				return uiRecognizer;
			}

			return null;
		}

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

		void LoadRecognizers()
		{
			if (ElementGestureRecognizers == null)
				return;

			if (_shouldReceiveTouch == null)
			{
				// Cache this so we don't create a new UITouchEventArgs instance for every recognizer
				_shouldReceiveTouch = ShouldReceiveTouch;
			}

			UIDragInteraction? uIDragInteraction = null;
			UIDropInteraction? uIDropInteraction = null;

			if (_dragAndDropDelegate != null && _platformView != null)
			{
				if (OperatingSystem.IsIOSVersionAtLeast(11))
				{
					foreach (var interaction in _platformView.Interactions)
					{
						if (interaction is UIDragInteraction uIDrag && uIDrag.Delegate == _dragAndDropDelegate)
							uIDragInteraction = uIDrag;

						if (interaction is UIDropInteraction uiDrop && uiDrop.Delegate == _dragAndDropDelegate)
							uIDropInteraction = uiDrop;
					}
				}
			}

			bool dragFound = false;
			bool dropFound = false;

			if (_platformView != null &&
				_handler.VirtualView is View v &&
				v.TapGestureRecognizerNeedsDelegate() &&
				(_platformView.AccessibilityTraits & UIAccessibilityTrait.Button) != UIAccessibilityTrait.Button)
			{
				_platformView.AccessibilityTraits |= UIAccessibilityTrait.Button;
				_addedFlags |= UIAccessibilityTrait.Button;
				if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsTvOSVersionAtLeast(13))
				{
					_defaultAccessibilityRespondsToUserInteraction = _platformView.AccessibilityRespondsToUserInteraction;
					_platformView.AccessibilityRespondsToUserInteraction = true;
				}
			}

			for (int i = 0; i < ElementGestureRecognizers.Count; i++)
			{
				IGestureRecognizer recognizer = ElementGestureRecognizers[i];

				if (_gestureRecognizers.ContainsKey(recognizer))
					continue;

				var nativeRecognizer = GetPlatformRecognizer(recognizer);

				if (nativeRecognizer != null && _platformView != null)
				{
					nativeRecognizer.ShouldReceiveTouch = _shouldReceiveTouch;
					_platformView.AddGestureRecognizer(nativeRecognizer);

					_gestureRecognizers[recognizer] = nativeRecognizer;
				}

				if (OperatingSystem.IsIOSVersionAtLeast(11) && recognizer is DragGestureRecognizer)
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

				if (OperatingSystem.IsIOSVersionAtLeast(11) && recognizer is DropGestureRecognizer)
				{
					dropFound = true;
					_dragAndDropDelegate = _dragAndDropDelegate ?? new DragAndDropDelegate(_handler);
					if (uIDropInteraction == null && _handler.PlatformView != null)
					{
						var interaction = new UIDropInteraction(_dragAndDropDelegate);
						_handler.PlatformView.AddInteraction(interaction);
					}
				}
			}
			if (OperatingSystem.IsIOSVersionAtLeast(11))
			{
				if (!dragFound && uIDragInteraction != null && _handler.PlatformView != null)
					_handler.PlatformView.RemoveInteraction(uIDragInteraction);

				if (!dropFound && uIDropInteraction != null && _handler.PlatformView != null)
					_handler.PlatformView.RemoveInteraction(uIDropInteraction);
			}

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

				if (_platformView != null)
					_platformView.RemoveGestureRecognizer(uiRecognizer);

				uiRecognizer.Dispose();
			}
		}

		bool ShouldReceiveTouch(UIGestureRecognizer recognizer, UITouch touch)
		{
			var platformView = _handler?.PlatformView;
			var virtualView = _handler?.VirtualView;

			if (virtualView == null || platformView == null)
			{
				return false;
			}

			if (virtualView.InputTransparent)
			{
				return false;
			}

			if (touch.View == platformView)
			{
				return true;
			}

			// If the touch is coming from the UIView our handler is wrapping (e.g., if it's  
			// wrapping a UIView which already has a gesture recognizer), then we should let it through
			// (This goes for children of that control as well)

			if (touch.View.IsDescendantOfView(platformView) &&
				(touch.View.GestureRecognizers?.Length > 0 || platformView.GestureRecognizers?.Length > 0))
			{
				return true;
			}

			return false;
		}

		void GestureRecognizersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			if (_platformView != null)
			{
				_platformView.AccessibilityTraits &= ~_addedFlags;

				if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsTvOSVersionAtLeast(13))
				{
					if (_defaultAccessibilityRespondsToUserInteraction != null)
						_platformView.AccessibilityRespondsToUserInteraction = _defaultAccessibilityRespondsToUserInteraction.Value;
				}
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