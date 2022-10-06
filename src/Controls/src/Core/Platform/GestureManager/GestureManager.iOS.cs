#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using System.Runtime.Versioning;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform.iOS;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;
using PlatformView = UIKit.UIView;

namespace Microsoft.Maui.Controls.Platform
{
	class GestureManager : IDisposable
	{
		readonly NotifyCollectionChangedEventHandler _collectionChangedHandler;

		readonly Dictionary<IGestureRecognizer, UIGestureRecognizer> _gestureRecognizers = new Dictionary<IGestureRecognizer, UIGestureRecognizer>();
		readonly List<INativeObject> _interactions = new List<INativeObject>();
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
				if (TryGetTapGestureRecognizer(kvp.Key, out TapGestureRecognizer? tapGestureRecognizer) &&
					tapGestureRecognizer != null)
				{
					tapGestureRecognizer.PropertyChanged -= OnTapGestureRecognizerPropertyChanged;
				}

				if (_platformView != null)
					_platformView.RemoveGestureRecognizer(kvp.Value);
				kvp.Value.ShouldReceiveTouch = null;
				kvp.Value.Dispose();
			}

			if (_platformView != null && OperatingSystem.IsIOSVersionAtLeast(11))
			{
				foreach (IUIInteraction interaction in _interactions)
				{
					_platformView.RemoveInteraction(interaction);
				}
			}
			_gestureRecognizers.Clear();

			Disconnect();

			_platformView = null;
		}

		static IList<GestureElement>? GetChildGestures(
			CGPoint originPoint,
			WeakReference weakEventTracker, WeakReference weakRecognizer, GestureManager? eventTracker, View? view)
		{
			if (!weakRecognizer.IsAlive)
				return null;

			if (eventTracker == null || eventTracker._disposed || view == null)
				return null;

			var childGestures = view.GetChildElements(new Point(originPoint.X, originPoint.Y));
			return childGestures;
		}

		static void ProcessRecognizerHandlerTap(
			WeakReference weakEventTracker,
			WeakReference weakRecognizer,
			CGPoint originPoint,
			int uiTapGestureRecognizerNumberOfTapsRequired,
			UITapGestureRecognizer? uITapGestureRecognizer = null)
		{
			var recognizer = weakRecognizer.Target as IGestureRecognizer;
			var eventTracker = weakEventTracker.Target as GestureManager;
			var view = eventTracker?._handler?.VirtualView as View;

			WeakReference? weakPlatformRecognizer = null;
			if (uITapGestureRecognizer != null)
				weakPlatformRecognizer = new WeakReference(uITapGestureRecognizer);

			if (recognizer is TapGestureRecognizer tapGestureRecognizer)
			{
				var childGestures = GetChildGestures(originPoint, weakEventTracker, weakRecognizer, eventTracker, view);

				if (childGestures?.HasChildGesturesFor<TapGestureRecognizer>(x => x.NumberOfTapsRequired == uiTapGestureRecognizerNumberOfTapsRequired) == true)
					return;

				if (view != null)
					tapGestureRecognizer.SendTapped(view, (relativeTo) => CalculatePosition(relativeTo, originPoint, weakPlatformRecognizer, weakEventTracker));
			}
			else if (recognizer is ChildGestureRecognizer childGestureRecognizer)
			{
				var childGestures = GetChildGestures(originPoint, weakEventTracker, weakRecognizer, eventTracker, view);

				var recognizers = childGestures?.GetChildGesturesFor<TapGestureRecognizer>(x => x.NumberOfTapsRequired == uiTapGestureRecognizerNumberOfTapsRequired);

				if (recognizers == null || weakRecognizer.Target == null)
					return;

				var childTapGestureRecognizer = childGestureRecognizer.GestureRecognizer as TapGestureRecognizer;
				foreach (var item in recognizers)
					if (item == childTapGestureRecognizer && view != null)
						childTapGestureRecognizer.SendTapped(view, (relativeTo) => CalculatePosition(relativeTo, originPoint, weakPlatformRecognizer, weakEventTracker));
			}
		}


		static Point? CalculatePosition(IElement? element, CGPoint originPoint, WeakReference? weakPlatformRecognizer, WeakReference weakEventTracker)
		{
			var eventTracker = weakEventTracker.Target as GestureManager;
			var virtualView = eventTracker?._handler?.VirtualView as View;
			var platformRecognizer = weakPlatformRecognizer?.Target as UIGestureRecognizer;

			if (virtualView == null)
				return null;

			if (platformRecognizer == null)
			{
				if (virtualView == element)
					return new Point((int)originPoint.X, (int)originPoint.Y);

				var targetViewScreenLocation = virtualView.GetLocationOnScreen();

				if (!targetViewScreenLocation.HasValue)
					return null;

				var windowX = targetViewScreenLocation.Value.X + originPoint.X;
				var windowY = targetViewScreenLocation.Value.Y + originPoint.Y;

				if (element == null)
					return new Point(windowX, windowY);

				if (element?.Handler?.PlatformView is UIView uiView)
				{
					var location = uiView.GetLocationOnScreen();

					var x = windowX - location.X;
					var y = windowY - location.Y;

					return new Point(x, y);
				}

				return null;
			}

			CGPoint? result = null;
			if (element == null)
				result = platformRecognizer.LocationInView(null);
			else if (element.Handler?.PlatformView is UIView view)
				result = platformRecognizer.LocationInView(view);

			if (result == null)
				return null;

			return new Point((int)result.Value.X, (int)result.Value.Y);

		}

		protected virtual UIGestureRecognizer? GetPlatformRecognizer(IGestureRecognizer recognizer)
		{
			if (recognizer == null)
				return null;

			var weakRecognizer = new WeakReference(recognizer);
			var weakEventTracker = new WeakReference(this);


			var tapGestureRecognizer = CreateTapRecognizer(weakEventTracker, weakRecognizer);

			if (tapGestureRecognizer != null)
			{
				return tapGestureRecognizer;
			}

			var pointerGestureRecognizer = recognizer as PointerGestureRecognizer;
			if (pointerGestureRecognizer != null)
			{
				var uiRecognizer = CreatePointerRecognizer(r =>
				{
					if (weakRecognizer.Target is PointerGestureRecognizer pointerGestureRecognizer &&
						weakEventTracker.Target is GestureManager eventTracker &&
						eventTracker._handler?.VirtualView is View view &&
						eventTracker._handler?.MauiContext?.GetPlatformWindow() is UIWindow window)
					{
						var originPoint = r.LocationInView(eventTracker?._handler?.PlatformView);

						switch (r.State)
						{
							case UIGestureRecognizerState.Began:
								pointerGestureRecognizer.SendPointerEntered(view, (relativeTo) => CalculatePosition(relativeTo, originPoint, weakRecognizer, weakEventTracker));
								break;
							case UIGestureRecognizerState.Changed:
								pointerGestureRecognizer.SendPointerMoved(view, (relativeTo) => CalculatePosition(relativeTo, originPoint, weakRecognizer, weakEventTracker));
								break;
							case UIGestureRecognizerState.Cancelled:
							case UIGestureRecognizerState.Failed:
							case UIGestureRecognizerState.Ended:
								pointerGestureRecognizer.SendPointerExited(view, (relativeTo) => CalculatePosition(relativeTo, originPoint, weakRecognizer, weakEventTracker));
								break;
						}
					}
				});
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
							case UIGestureRecognizerState.Began:
								if (r.NumberOfTouches < 2)
									return;

								pinchGestureRecognizer.SendPinchStarted(view, scaledPoint);
								startingScale = view.Scale;
								break;
							case UIGestureRecognizerState.Changed:
								if (r.NumberOfTouches < 2 && pinchGestureRecognizer.IsPinching)
								{
									r.State = UIGestureRecognizerState.Ended;
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
							case UIGestureRecognizerState.Cancelled:
							case UIGestureRecognizerState.Failed:
								if (pinchGestureRecognizer.IsPinching)
									pinchGestureRecognizer.SendPinchCanceled(view);
								break;
							case UIGestureRecognizerState.Ended:
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
							case UIGestureRecognizerState.Began:
								if (r.NumberOfTouches != panRecognizer.TouchPoints)
									return;
								panGestureRecognizer.SendPanStarted(view, PanGestureRecognizer.CurrentId.Value);
								break;
							case UIGestureRecognizerState.Changed:
								if (r.NumberOfTouches != panRecognizer.TouchPoints)
								{
									r.State = UIGestureRecognizerState.Ended;
									panGestureRecognizer.SendPanCompleted(view, PanGestureRecognizer.CurrentId.Value);
									PanGestureRecognizer.CurrentId.Increment();
									return;
								}
								var translationInView = r.TranslationInView(_platformView);
								panGestureRecognizer.SendPan(view, translationInView.X, translationInView.Y, PanGestureRecognizer.CurrentId.Value);
								break;
							case UIGestureRecognizerState.Cancelled:
							case UIGestureRecognizerState.Failed:
								panGestureRecognizer.SendPanCanceled(view, PanGestureRecognizer.CurrentId.Value);
								PanGestureRecognizer.CurrentId.Increment();
								break;
							case UIGestureRecognizerState.Ended:
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

		CustomHoverGestureRecognizer CreatePointerRecognizer(Action<UIHoverGestureRecognizer> action)
		{
			var result = new CustomHoverGestureRecognizer(action);
			return result;
		}

		UITapGestureRecognizer? CreateTapRecognizer(
			WeakReference weakEventTracker,
			WeakReference weakRecognizer)
		{
			if (!TryGetTapGestureRecognizer(weakRecognizer.Target as IGestureRecognizer, out TapGestureRecognizer? tapGesture))
				return null;

			if (tapGesture == null)
				return null;

			Action<UITapGestureRecognizer> action = new Action<UITapGestureRecognizer>((sender) =>
			{
				var eventTracker = weakEventTracker.Target as GestureManager;
				var originPoint = sender.LocationInView(eventTracker?._handler?.PlatformView);
				ProcessRecognizerHandlerTap(weakEventTracker, weakRecognizer, originPoint, (int)sender.NumberOfTapsRequired, sender);
			});

			var result = new UITapGestureRecognizer(action)
			{
				NumberOfTapsRequired = (uint)tapGesture.NumberOfTapsRequired,
				ShouldRecognizeSimultaneously = ShouldRecognizeTapsTogether
			};

			// For whatever reason the secondary mask doesn't work on catalyst
			// it only works when you have a mouse connected to an iPad
			// so we just ignore setting the mask if the user is running catalyst
			// right click is handled by adding a UIContextMenu interaction
			if (OperatingSystem.IsIOSVersionAtLeast(13, 4) && !OperatingSystem.IsMacCatalyst())
			{
				UIEventButtonMask uIEventButtonMask = (UIEventButtonMask)0;

				if ((tapGesture.Buttons & ButtonsMask.Primary) == ButtonsMask.Primary)
					uIEventButtonMask |= UIEventButtonMask.Primary;

				if ((tapGesture.Buttons & ButtonsMask.Secondary) == ButtonsMask.Secondary)
					uIEventButtonMask |= UIEventButtonMask.Secondary;

				result.ButtonMaskRequired = uIEventButtonMask;
			}

			return result;
		}

		static bool ShouldRecognizeTapsTogether(UIGestureRecognizer gesture, UIGestureRecognizer other)
		{
			// If multiple tap gestures are potentially firing (because multiple tap gesture recognizers have been
			// added to the MAUI Element), we want to allow them to fire simultaneously if they have the same number
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

		bool TryGetTapGestureRecognizer(IGestureRecognizer? recognizer, out TapGestureRecognizer? tapGestureRecognizer)
		{
			tapGestureRecognizer =
					recognizer as TapGestureRecognizer ??
					(recognizer as ChildGestureRecognizer)?.GestureRecognizer as TapGestureRecognizer;

			return tapGestureRecognizer != null;
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
				if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13)
#if TVOS
				|| OperatingSystem.IsTvOSVersionAtLeast(11)
#endif
					)
				{
					_defaultAccessibilityRespondsToUserInteraction = _platformView.AccessibilityRespondsToUserInteraction;
					_platformView.AccessibilityRespondsToUserInteraction = true;
				}
			}

			_interactions.Clear();

			for (int i = 0; i < ElementGestureRecognizers.Count; i++)
			{
				IGestureRecognizer recognizer = ElementGestureRecognizers[i];

				if (_gestureRecognizers.ContainsKey(recognizer))
					continue;

				if (TryGetTapGestureRecognizer(recognizer, out TapGestureRecognizer? tapGestureRecognizer) &&
					tapGestureRecognizer != null)
				{
					tapGestureRecognizer.PropertyChanged += OnTapGestureRecognizerPropertyChanged;
				}

				// AddFakeRightClickForMacCatalyst returns the button mask for the processed tap gesture
				// If a fake gesture wasn't added then it just returns 0
				// If a fake gesture was added then we return the button mask fromm the tap gesture
				// If the user only cares about right click then we'll just exit now
				// so that an additional tap gesture doesn't get added
				if (AddFakeRightClickForMacCatalyst(recognizer) == ButtonsMask.Secondary)
				{
					continue;
				}

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

				if (TryGetTapGestureRecognizer(gestureRecognizer, out TapGestureRecognizer? tapGestureRecognizer) &&
					tapGestureRecognizer != null)
				{
					gestureRecognizer.PropertyChanged -= OnTapGestureRecognizerPropertyChanged;
				}

				uiRecognizer.Dispose();
			}

			if (_platformView != null && OperatingSystem.IsIOSVersionAtLeast(11))
			{
				for (int i = _platformView.Interactions.Length - 1; i >= 0; i--)
				{
					var interaction = (IUIInteraction)_platformView.Interactions[i];
					if (interaction is FakeRightClickContextMenuInteraction && !_interactions.Contains(interaction))
					{
						_platformView.RemoveInteraction(interaction);
					}
				}
			}
		}

		void OnTapGestureRecognizerPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.Is(TapGestureRecognizer.ButtonsProperty))
				LoadRecognizers();
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

				if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13))
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

		ButtonsMask AddFakeRightClickForMacCatalyst(IGestureRecognizer recognizer)
		{
			if (OperatingSystem.IsIOSVersionAtLeast(13, 4) && OperatingSystem.IsMacCatalyst())
			{
				TryGetTapGestureRecognizer(recognizer, out TapGestureRecognizer? tapRecognizer);

				if (tapRecognizer == null || (tapRecognizer.Buttons & ButtonsMask.Secondary) != ButtonsMask.Secondary)
				{
					return (ButtonsMask)0;
				}

				if (_platformView != null)
				{
					foreach (var interaction in _platformView.Interactions)
					{
						// check if this gesture was already added
						if (interaction is FakeRightClickContextMenuInteraction faker &&
							faker.TapGestureRecognizer == tapRecognizer)
						{
							_interactions.Add(faker);
							return tapRecognizer.Buttons;
						}
					}
				}

				var fakeInteraction = new FakeRightClickContextMenuInteraction(tapRecognizer, this);
				_interactions.Add(fakeInteraction);

				_platformView?.AddInteraction(fakeInteraction);

				return tapRecognizer.Buttons;
			}

			return (ButtonsMask)0;
		}

		[SupportedOSPlatform("ios13.0")]
		[SupportedOSPlatform("maccatalyst13.0.0")]
		[UnsupportedOSPlatform("tvos")]
		internal class FakeRightClickContextMenuInteraction : UIContextMenuInteraction
		{
			// Store a reference to the platform delegate so that it is not garbage collected
			FakeRightClickDelegate? _dontCollectMePlease;

			public FakeRightClickContextMenuInteraction(TapGestureRecognizer tapGestureRecognizer, GestureManager gestureManager)
				: base(new FakeRightClickDelegate(tapGestureRecognizer, gestureManager))
			{
				_dontCollectMePlease = Delegate as FakeRightClickDelegate;
			}

			public TapGestureRecognizer? TapGestureRecognizer => _dontCollectMePlease?.TapGestureRecognizer;

			class FakeRightClickDelegate : UIContextMenuInteractionDelegate
			{
				WeakReference _recognizer;
				WeakReference _gestureManager;

				public TapGestureRecognizer? TapGestureRecognizer => _recognizer.Target as TapGestureRecognizer;
				public FakeRightClickDelegate(TapGestureRecognizer tapGestureRecognizer, GestureManager gestureManager)
				{
					_recognizer = new WeakReference(tapGestureRecognizer);
					_gestureManager = new WeakReference(gestureManager);
				}

				public override UIContextMenuConfiguration? GetConfigurationForMenu(UIContextMenuInteraction interaction, CGPoint location)
				{
					ProcessRecognizerHandlerTap(_gestureManager, _recognizer, location, 1);
					return null;
				}
			}
		}
	}
}