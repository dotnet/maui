using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;

namespace Microsoft.Maui.Controls.Platform
{
	class GesturePlatformManager : IDisposable
	{
		readonly IPlatformViewHandler _handler;
		readonly NotifyCollectionChangedEventHandler _collectionChangedHandler;
		readonly List<uint> _fingers = new List<uint>();
		FrameworkElement? _container;
		FrameworkElement? _control;
		VisualElement? _element;
		TappedEventHandler? _tappedEventHandler;
		DoubleTappedEventHandler? _doubleTappedEventHandler;

		SubscriptionFlags _subscriptionFlags = SubscriptionFlags.None;

		bool _isDisposed;
		bool _isPanning;
		bool _isSwiping;
		bool _isPinching;
		bool _wasPanGestureStartedSent;
		bool _wasPinchGestureStartedSent;
		const string _doNotUsePropertyString = "_XFPropertes_DONTUSE";

		// Multi-tap gesture support for taps > 2
		int _currentTapCount = 0;
		DateTime _lastTapTime = DateTime.MinValue;
		Point _lastTapPosition = new Point(-1, -1);
		const double _maxTapInterval = 500; // milliseconds
		const double _maxTapDistance = 10; // pixels

		public GesturePlatformManager(IViewHandler handler)
		{
			_handler = (IPlatformViewHandler)handler;
			_collectionChangedHandler = ModelGestureRecognizersOnCollectionChanged;

			if (_handler.VirtualView == null)
			{
				throw new ArgumentNullException(nameof(handler.VirtualView));
			}

			if (_handler.PlatformView == null)
			{
				throw new ArgumentNullException(nameof(handler.PlatformView));
			}

			Element = (VisualElement)_handler.VirtualView;
			Control = _handler.PlatformView;

			if (_handler.ContainerView != null)
			{
				Container = _handler.ContainerView;
			}
			else
			{
				Container = _handler.PlatformView;
			}
		}

		public FrameworkElement? Container
		{
			get { return _container; }
			set
			{
				if (_container == value)
				{
					return;
				}

				ClearContainerEventHandlers();

				_container = value;

				UpdatingGestureRecognizers();
			}
		}

		ObservableCollection<IGestureRecognizer>? ElementGestureRecognizers =>
			(_handler.VirtualView as Element)?.GetCompositeGestureRecognizers() as ObservableCollection<IGestureRecognizer>;

		// TODO MAUI
		// Do we need to provide a hook for this in the handlers?
		// For now I just built this ugly matching statement
		// to replicate our handlers where we are setting this to true
		public bool PreventGestureBubbling
		{
			get
			{
				return Element switch
				{
					Button => true,
					CheckBox => true,
					DatePicker => true,
					Stepper => true,
					Slider => true,
					Switch => true,
					TimePicker => true,
					ImageButton => true,
					RadioButton => true,
					_ => false,
				};
			}
		}

		public FrameworkElement? Control
		{
			get { return _control; }
			set
			{
				if (_control == value)
				{
					return;
				}

				if (_control is not null)
				{
					if ((_subscriptionFlags & SubscriptionFlags.ControlTapEventSubscribed) != 0)
					{
						_control.Tapped -= HandleTapped;
					}

					if ((_subscriptionFlags & SubscriptionFlags.ControlDoubleTapEventSubscribed) != 0)
					{
						_control.DoubleTapped -= HandleDoubleTapped;
					}
				}

				_control = value;

				if (PreventGestureBubbling)
				{
					UpdatingGestureRecognizers();
				}
			}
		}

		void SendEventArgs<TRecognizer>(Action<TRecognizer> func)
		{
			if (_container is null && _control is null)
			{
				return;
			}

			var view = Element as View;
			var gestures = view?.GestureRecognizers;

			if (gestures is null)
			{
				return;
			}

			foreach (var gesture in gestures)
			{
				if (gesture is TRecognizer recognizer)
				{
					func(recognizer);
				}
			}
		}

		void HandleDragLeave(object sender, Microsoft.UI.Xaml.DragEventArgs e)
		{
			var dragEventArgs = ToDragEventArgs(e, new PlatformDragEventArgs(sender as UIElement, e));

			dragEventArgs.AcceptedOperation = (DataPackageOperation)((int)dragEventArgs.AcceptedOperation);
			SendEventArgs<DropGestureRecognizer>(rec =>
			{
				if (!rec.AllowDrop)
				{
					return;
				}

				var operationPriorToSend = dragEventArgs.AcceptedOperation;
				rec.SendDragLeave(dragEventArgs);

				// If you set the AcceptedOperation to a value it was already set to
				// it causes the related animation to remain visible when the dragging component leaves
				// for example
				// e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
				// Even if AcceptedOperation is already set to Copy it will cause the copy animation
				// to remain even after the dragged element has left
				if (!dragEventArgs.PlatformArgs?.Handled ?? true && operationPriorToSend != dragEventArgs.AcceptedOperation)
				{
					var result = (int)dragEventArgs.AcceptedOperation;
					e.AcceptedOperation = (global::Windows.ApplicationModel.DataTransfer.DataPackageOperation)result;
				}
			});
		}

		void HandleDragOver(object sender, Microsoft.UI.Xaml.DragEventArgs e)
		{
			var dragEventArgs = ToDragEventArgs(e, new PlatformDragEventArgs(sender as UIElement, e));

			SendEventArgs<DropGestureRecognizer>(rec =>
			{
				if (!rec.AllowDrop)
				{
					e.AcceptedOperation = global::Windows.ApplicationModel.DataTransfer.DataPackageOperation.None;
					return;
				}

				rec.SendDragOver(dragEventArgs);
				if (!dragEventArgs.PlatformArgs?.Handled ?? true)
				{
					var result = (int)dragEventArgs.AcceptedOperation;
					e.AcceptedOperation = (global::Windows.ApplicationModel.DataTransfer.DataPackageOperation)result;
				}
			});
		}

		void HandleDropCompleted(UIElement sender, Microsoft.UI.Xaml.DropCompletedEventArgs e)
		{
			var args = new DropCompletedEventArgs(new PlatformDropCompletedEventArgs(sender, e));
			SendEventArgs<DragGestureRecognizer>(rec => rec.SendDropCompleted(args));
		}

		void HandleDrop(object sender, Microsoft.UI.Xaml.DragEventArgs e)
		{
			var datapackage = e.DataView.Properties[_doNotUsePropertyString] as DataPackage;
			VisualElement? element = null;

			if (sender is IViewHandler handler &&
				handler.VirtualView is VisualElement ve)
			{
				element = ve;
			}

			var args = new DropEventArgs(datapackage?.View, (relativeTo) => GetPosition(relativeTo, e), new PlatformDropEventArgs(sender as UIElement, e));
			SendEventArgs<DropGestureRecognizer>(async rec =>
			{
				if (!rec.AllowDrop)
				{
					return;
				}

				try
				{
					await rec.SendDrop(args);
				}
				catch (Exception dropExc)
				{
					Application.Current?.FindMauiContext()?.CreateLogger<DropGestureRecognizer>()?.LogWarning(dropExc, "Error sending event");
				}
			});
		}

		void HandleDragStarting(UIElement sender, Microsoft.UI.Xaml.DragStartingEventArgs e)
		{
			SendEventArgs<DragGestureRecognizer>(rec =>
			{
				if (!rec.CanDrag || Element is not View view)
				{
					e.Cancel = true;
					return;
				}

				var args = rec.SendDragStarting(view, (relativeTo) => GetPosition(relativeTo, e), new PlatformDragStartingEventArgs(sender, e));

				e.Data.Properties[_doNotUsePropertyString] = args.Data;

#pragma warning disable CS0618 // Type or member is obsolete
				if ((!args.Handled || (!args.PlatformArgs?.Handled ?? true)) && sender is IViewHandler handler)
#pragma warning restore CS0618 // Type or member is obsolete
				{
					if (handler?.PlatformView is UI.Xaml.Controls.Image nativeImage &&
						nativeImage.Source is BitmapImage bi && bi.UriSource != null)
					{
						e.Data.SetBitmap(RandomAccessStreamReference.CreateFromUri(bi.UriSource));
					}
					else if (!String.IsNullOrWhiteSpace(args.Data.Text))
					{
						Uri? uri;
						if (Uri.TryCreate(args.Data.Text, UriKind.Absolute, out uri))
						{
							if (args.Data.Text.StartsWith("http", StringComparison.OrdinalIgnoreCase))
							{
								e.Data.SetWebLink(uri);
							}
							else
							{
								e.Data.SetApplicationLink(uri);
							}
						}
						else
						{
							e.Data.SetText(args.Data.Text);
						}
					}

					e.AllowedOperations = global::Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
				}

				e.Cancel = args.Cancel;
			});
		}

		public VisualElement? Element
		{
			get { return _element; }
			set
			{
				if (_element == value)
					return;

				if (_element is View && ElementGestureRecognizers is { } gestureRecognizersBefore)
				{
					gestureRecognizersBefore.CollectionChanged -= _collectionChangedHandler;
				}

				_element = value;

				if (_element is View && ElementGestureRecognizers is { } gestureRecognizersAfter)
				{
					gestureRecognizersAfter.CollectionChanged += _collectionChangedHandler;
				}
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void ClearContainerEventHandlers()
		{
			if (_container != null)
			{
				if ((_subscriptionFlags & SubscriptionFlags.ContainerDragEventsSubscribed) != 0)
				{
					_subscriptionFlags &= ~SubscriptionFlags.ContainerDragEventsSubscribed;

					_container.CanDrag = false;
					_container.DragStarting -= HandleDragStarting;
					_container.DropCompleted -= HandleDropCompleted;
				}

				if ((_subscriptionFlags & SubscriptionFlags.ContainerDropEventsSubscribed) != 0)
				{
					_subscriptionFlags &= ~SubscriptionFlags.ContainerDropEventsSubscribed;

					_container.AllowDrop = false;
					_container.DragOver -= HandleDragOver;
					_container.Drop -= HandleDrop;
					_container.DragLeave -= HandleDragLeave;
				}

				if ((_subscriptionFlags & SubscriptionFlags.ContainerTapAndRightTabEventSubscribed) != 0)
				{
					_subscriptionFlags &= ~SubscriptionFlags.ContainerTapAndRightTabEventSubscribed;

					if (_control is TextBox)
					{
						_container.RemoveHandler(FrameworkElement.TappedEvent, _tappedEventHandler);
						_tappedEventHandler = null;
					}
					else
					{
						_container.Tapped -= OnTap;
					}

					_container.RightTapped -= OnTap;
				}

				if ((_subscriptionFlags & SubscriptionFlags.ContainerDoubleTapEventSubscribed) != 0)
				{
					_subscriptionFlags &= ~SubscriptionFlags.ContainerDoubleTapEventSubscribed;

					if (_control is TextBox)
					{
						_container.RemoveHandler(FrameworkElement.DoubleTappedEvent, _doubleTappedEventHandler);
						_doubleTappedEventHandler = null;
					}
					else
					{
						_container.DoubleTapped -= OnTap;
					}
				}

				if ((_subscriptionFlags & SubscriptionFlags.ContainerPgrPointerEventsSubscribed) != 0)
				{
					_subscriptionFlags &= ~SubscriptionFlags.ContainerPgrPointerEventsSubscribed;

					_container.PointerEntered -= OnPgrPointerEntered;
					_container.PointerExited -= OnPgrPointerExited;
					_container.PointerMoved -= OnPgrPointerMoved;
					_container.PointerPressed -= OnPgrPointerPressed;
					_container.PointerReleased -= OnPgrPointerReleased;
				}

				if ((_subscriptionFlags & SubscriptionFlags.ContainerManipulationAndPointerEventsSubscribed) != 0)
				{
					_subscriptionFlags &= ~SubscriptionFlags.ContainerManipulationAndPointerEventsSubscribed;

					_container.ManipulationDelta -= OnManipulationDelta;
					_container.ManipulationStarted -= OnManipulationStarted;
					_container.ManipulationCompleted -= OnManipulationCompleted;
					_container.PointerCanceled -= OnPointerCanceled;
				}

				if (Element is View && ElementGestureRecognizers is { } gestureRecognizers)
				{
					if (gestureRecognizers.FirstGestureOrDefault<DragGestureRecognizer>() is { } dragGesture)
					{
						dragGesture.PropertyChanged -= HandleDragAndDropGesturePropertyChanged;
					}
					if (gestureRecognizers.FirstGestureOrDefault<DropGestureRecognizer>() is { } dropGesture)
					{
						dropGesture.PropertyChanged -= HandleDragAndDropGesturePropertyChanged;
					}
				}
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_isDisposed)
			{
				return;
			}

			_isDisposed = true;

			if (!disposing)
			{
				return;
			}

			ClearContainerEventHandlers();

			if (_element is View && ElementGestureRecognizers is { } gestureRecognizers)
			{
				gestureRecognizers.CollectionChanged -= _collectionChangedHandler;
			}

			if (_control is not null)
			{
				if ((_subscriptionFlags & SubscriptionFlags.ControlTapEventSubscribed) != 0)
				{
					_control.Tapped -= HandleTapped;
				}

				if ((_subscriptionFlags & SubscriptionFlags.ControlDoubleTapEventSubscribed) != 0)
				{
					_control.DoubleTapped -= HandleDoubleTapped;
				}
			}

			Control = null;
			Element = null;
			Container = null;
		}

		void HandleSwipe(ManipulationDeltaRoutedEventArgs e, View view)
		{
			if (_fingers.Count > 1 || view == null)
			{
				return;
			}

			_isSwiping = true;

			foreach (SwipeGestureRecognizer recognizer in view.GestureRecognizers.GetGesturesFor<SwipeGestureRecognizer>())
			{
				((ISwipeGestureController)recognizer).SendSwipe(view, e.Delta.Translation.X + e.Cumulative.Translation.X, e.Delta.Translation.Y + e.Cumulative.Translation.Y);
			}
		}

		void HandlePan(ManipulationDeltaRoutedEventArgs e, View view)
		{
			if (view == null)
			{
				return;
			}

			_isPanning = true;

			foreach (IPanGestureController recognizer in view.GestureRecognizers.GetGesturesFor<PanGestureRecognizer>().Where(g => g.TouchPoints == _fingers.Count))
			{
				if (!_wasPanGestureStartedSent)
				{
					recognizer.SendPanStarted(view, PanGestureRecognizer.CurrentId.Value);
				}
				recognizer.SendPan(view, e.Delta.Translation.X + e.Cumulative.Translation.X, e.Delta.Translation.Y + e.Cumulative.Translation.Y, PanGestureRecognizer.CurrentId.Value);
			}
			_wasPanGestureStartedSent = true;
		}

		void HandlePinch(ManipulationDeltaRoutedEventArgs e, View view)
		{
			if (_fingers.Count < 2 || view == null)
			{
				return;
			}

			if (e.OriginalSource is UIElement container)
			{
				global::Windows.Foundation.Point translationPoint = container.TransformToVisual(Container).TransformPoint(e.Position);
				var scaleOriginPoint = new Point(translationPoint.X / view.Width, translationPoint.Y / view.Height);
				IEnumerable<PinchGestureRecognizer> pinchGestures = view.GestureRecognizers.GetGesturesFor<PinchGestureRecognizer>();

				foreach (IPinchGestureController recognizer in pinchGestures)
				{
					if (!_wasPinchGestureStartedSent)
					{
						recognizer.SendPinchStarted(view, scaleOriginPoint);
					}

					recognizer.SendPinch(view, e.Delta.Scale, scaleOriginPoint);
				}

				_wasPinchGestureStartedSent = true;
			}
		}

		void ModelGestureRecognizersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			UpdatingGestureRecognizers();
		}

		void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
		{
			SwipeComplete(true);
			PinchComplete(true);
			PanComplete(true);

			_fingers.Clear();
		}

		void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
		{
			if (Element is not View view)
			{
				return;
			}

			HandleSwipe(e, view);
			HandlePinch(e, view);
			HandlePan(e, view);
		}

		void OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
		{
			if (Element is not View view)
			{
				return;
			}

			_isPinching = true;
			_wasPinchGestureStartedSent = false;
			_wasPanGestureStartedSent = false;
		}

		void OnPointerCanceled(object sender, PointerRoutedEventArgs e)
		{
			SwipeComplete(false);
			PinchComplete(false);
			PanComplete(false);

			_fingers.Clear();
		}

		void OnPointerExited(object sender, PointerRoutedEventArgs e)
		{
			SwipeComplete(true);

			if (!_isPanning)
			{
				_fingers.Remove(e.Pointer.PointerId);
			}
		}

		void OnPointerPressed(object sender, PointerRoutedEventArgs e)
		{
			uint id = e.Pointer.PointerId;
			if (!_fingers.Contains(id))
			{
				_fingers.Add(id);
			}
		}

		void OnPointerReleased(object sender, PointerRoutedEventArgs e)
		{
			SwipeComplete(true);
			PanComplete(true);

			_fingers.Remove(e.Pointer.PointerId);
		}

		void OnPgrPointerEntered(object sender, PointerRoutedEventArgs e)
		{
			HandlePgrPointerEvent(e, (view, recognizer)
						=> recognizer.SendPointerEntered(view, (relativeTo)
							=> GetPosition(relativeTo, e), _control is null ? null : new PlatformPointerEventArgs(_control, e)));
		}

		void OnPgrPointerExited(object sender, PointerRoutedEventArgs e)
		{
			HandlePgrPointerEvent(e, (view, recognizer)
						=> recognizer.SendPointerExited(view, (relativeTo)
							=> GetPosition(relativeTo, e), _control is null ? null : new PlatformPointerEventArgs(_control, e)));

			if ((_subscriptionFlags & SubscriptionFlags.ContainerManipulationAndPointerEventsSubscribed) != 0)
			{
				OnPointerExited(sender, e);
			}
		}

		void OnPgrPointerMoved(object sender, PointerRoutedEventArgs e)
			=> HandlePgrPointerEvent(e, (view, recognizer)
				=> recognizer.SendPointerMoved(view, (relativeTo)
					=> GetPosition(relativeTo, e), _control is null ? null : new PlatformPointerEventArgs(_control, e)));

		void OnPgrPointerPressed(object sender, PointerRoutedEventArgs e)
		{
			HandlePgrPointerEvent(e, (view, recognizer)
						=> recognizer.SendPointerPressed(view, (relativeTo)
							=> GetPosition(relativeTo, e), _control is null ? null : new PlatformPointerEventArgs(_control, e)));

			if ((_subscriptionFlags & SubscriptionFlags.ContainerManipulationAndPointerEventsSubscribed) != 0)
			{
				OnPointerPressed(sender, e);
			}
		}

		void OnPgrPointerReleased(object sender, PointerRoutedEventArgs e)
		{
			HandlePgrPointerEvent(e, (view, recognizer)
						=> recognizer.SendPointerReleased(view, (relativeTo)
							=> GetPosition(relativeTo, e), _control is null ? null : new PlatformPointerEventArgs(_control, e)));

			if ((_subscriptionFlags & SubscriptionFlags.ContainerManipulationAndPointerEventsSubscribed) != 0)
			{
				OnPointerReleased(sender, e);
			}
		}

		private void HandlePgrPointerEvent(PointerRoutedEventArgs e, Action<View, PointerGestureRecognizer> SendPointerEvent)
		{
			if (Element is not View view)
			{
				return;
			}

			var pointerGestures = ElementGestureRecognizers.GetGesturesFor<PointerGestureRecognizer>();
			foreach (var recognizer in pointerGestures)
			{
				SendPointerEvent.Invoke(view, recognizer);
			}
		}
		Point? GetPosition(IElement? relativeTo, RoutedEventArgs e)
		{
			var result = e.GetPositionRelativeToElement(relativeTo);

			if (result is null)
			{
				return null;
			}

			return result.Value.ToPoint();
		}

		void OnTap(object sender, RoutedEventArgs e)
		{
			if (Element is not View view)
			{
				return;
			}

			if (!view.IsEnabled)
			{
				return;
			}

			var tapPosition = e.GetPositionRelativeToPlatformElement(Control);

			if (tapPosition == null)
			{
				return;
			}

			var currentPosition = new Point(tapPosition.Value.X, tapPosition.Value.Y);
			var currentTime = DateTime.Now;

			// Check if this tap is part of a multi-tap sequence
			bool isSequentialTap = false;
			if (_lastTapTime != DateTime.MinValue)
			{
				var timeDiff = (currentTime - _lastTapTime).TotalMilliseconds;
				var distance = Math.Sqrt(Math.Pow(currentPosition.X - _lastTapPosition.X, 2) + 
				                       Math.Pow(currentPosition.Y - _lastTapPosition.Y, 2));

				if (timeDiff <= _maxTapInterval && distance <= _maxTapDistance)
				{
					isSequentialTap = true;
				}
			}

			if (isSequentialTap)
			{
				_currentTapCount++;
			}
			else
			{
				_currentTapCount = 1;
			}

			_lastTapTime = currentTime;
			_lastTapPosition = currentPosition;

			var children =
				(view as IGestureController)?.GetChildElements(currentPosition)?.
				GetChildGesturesFor<TapGestureRecognizer>(g => ValidateGesture(g, _currentTapCount, e));

			if (ProcessGestureRecognizers(children))
			{
				return;
			}

			IEnumerable<TapGestureRecognizer> tapGestures = view.GestureRecognizers.GetGesturesFor<TapGestureRecognizer>(g => ValidateGesture(g, _currentTapCount, e));
			ProcessGestureRecognizers(tapGestures);

			bool ProcessGestureRecognizers(IEnumerable<TapGestureRecognizer>? tapGestures)
			{
				bool handled = false;
				if (tapGestures == null)
				{
					return handled;
				}

				foreach (var recognizer in tapGestures)
				{
					if (recognizer.NumberOfTapsRequired == _currentTapCount)
					{
						recognizer.SendTapped(view, (relativeTo) => GetPosition(relativeTo, e));
						e.SetHandled(true);
						handled = true;
						
						// Reset tap count after successful gesture
						_currentTapCount = 0;
						_lastTapTime = DateTime.MinValue;
					}
				}

				return handled;
			}
		}

		bool ValidateGesture(TapGestureRecognizer g, int tapCount, RoutedEventArgs e)
		{
			if (e is RightTappedRoutedEventArgs)
			{
				// Currently we only support single right clicks
				if ((g.Buttons & ButtonsMask.Secondary) == ButtonsMask.Secondary)
				{
					return g.NumberOfTapsRequired == 1;
				}
				else
				{
					return false;
				}
			}

			if ((g.Buttons & ButtonsMask.Primary) != ButtonsMask.Primary)
				return false;

			// For multi-tap gestures, we validate based on the current tap count
			return g.NumberOfTapsRequired == tapCount;
		}


		void SwipeComplete(bool success)
		{
			if (Element is not View view || !_isSwiping)
			{
				return;
			}

			if (success)
			{
				foreach (SwipeGestureRecognizer recognizer in view.GestureRecognizers.GetGesturesFor<SwipeGestureRecognizer>())
				{
					((ISwipeGestureController)recognizer).DetectSwipe(view, recognizer.Direction);
				}
			}

			_isSwiping = false;
		}

		void PanComplete(bool success)
		{
			if (Element is not View view || !_isPanning)
			{
				return;
			}

			foreach (IPanGestureController recognizer in view.GestureRecognizers.GetGesturesFor<PanGestureRecognizer>().Where(g => g.TouchPoints == _fingers.Count))
			{
				if (success)
				{
					recognizer.SendPanCompleted(view, PanGestureRecognizer.CurrentId.Value);
				}
				else
				{
					recognizer.SendPanCanceled(view, PanGestureRecognizer.CurrentId.Value);
				}
			}

			PanGestureRecognizer.CurrentId.Increment();
			_isPanning = false;
		}

		void PinchComplete(bool success)
		{
			if (Element is not View view || !_isPinching)
			{
				return;
			}

			IEnumerable<PinchGestureRecognizer> pinchGestures = view.GestureRecognizers.GetGesturesFor<PinchGestureRecognizer>();
			foreach (IPinchGestureController recognizer in pinchGestures)
			{
				if (success)
				{
					recognizer.SendPinchEnded(view);
				}
				else
				{
					recognizer.SendPinchCanceled(view);
				}
			}

			_isPinching = false;
		}

		void UpdateDragAndDropGestureRecognizers()
		{
			if (_container is null || Element is not View view || view.GestureRecognizers is not IList<IGestureRecognizer> gestures)
			{
				return;
			}

			DragGestureRecognizer? dragGesture = gestures.FirstGestureOrDefault<DragGestureRecognizer>();
			DropGestureRecognizer? dropGesture = gestures.FirstGestureOrDefault<DropGestureRecognizer>();

			if (dragGesture is not null)
			{
				dragGesture.PropertyChanged -= HandleDragAndDropGesturePropertyChanged;
				dragGesture.PropertyChanged += HandleDragAndDropGesturePropertyChanged;

				if (dragGesture.CanDrag && ((_subscriptionFlags & SubscriptionFlags.ContainerDragEventsSubscribed) == 0))
				{
					_subscriptionFlags |= SubscriptionFlags.ContainerDragEventsSubscribed;
					_container.CanDrag = true;
					_container.DragStarting += HandleDragStarting;
					_container.DropCompleted += HandleDropCompleted;
				}
			}

			if (dropGesture is not null)
			{
				dropGesture.PropertyChanged -= HandleDragAndDropGesturePropertyChanged;
				dropGesture.PropertyChanged += HandleDragAndDropGesturePropertyChanged;

				if (dropGesture.AllowDrop && ((_subscriptionFlags & SubscriptionFlags.ContainerDropEventsSubscribed) == 0))
				{
					_subscriptionFlags |= SubscriptionFlags.ContainerDropEventsSubscribed;
					_container.AllowDrop = true;
					_container.DragOver += HandleDragOver;
					_container.Drop += HandleDrop;
					_container.DragLeave += HandleDragLeave;
				}
			}
		}

		void UpdatingGestureRecognizers()
		{
			var view = Element as View;
			IList<IGestureRecognizer>? gestures = view?.GestureRecognizers;

			if (_container is null || gestures is null)
			{
				return;
			}

			ClearContainerEventHandlers();
			UpdateDragAndDropGestureRecognizers();

			var children = (view as IGestureController)?.GetChildElements(Point.Zero);

			// Subscribe to tap events for all tap gesture recognizers
			if (gestures.HasAnyGesturesFor<TapGestureRecognizer>()
				|| children?.GetChildGesturesFor<TapGestureRecognizer>().Any() == true)
			{
				_subscriptionFlags |= SubscriptionFlags.ContainerTapAndRightTabEventSubscribed;

				if (_control is TextBox)
				{
					_tappedEventHandler = new TappedEventHandler(OnTap);
					_container.AddHandler(FrameworkElement.TappedEvent, _tappedEventHandler, true);
				}
				else
				{
					_container.Tapped += OnTap;
				}

				_container.RightTapped += OnTap;
			}
			else
			{
				if (_control is not null && PreventGestureBubbling)
				{
					_subscriptionFlags |= SubscriptionFlags.ControlTapEventSubscribed;
					_control.Tapped += HandleTapped;
				}
			}

			// Subscribe to double tap events for gestures that require it (for legacy support)
			if (gestures.HasAnyGesturesFor<TapGestureRecognizer>(g => g.NumberOfTapsRequired >= 2)
				|| children?.GetChildGesturesFor<TapGestureRecognizer>(g => g.NumberOfTapsRequired >= 2).Any() == true)
			{
				_subscriptionFlags |= SubscriptionFlags.ContainerDoubleTapEventSubscribed;

				if (_control is TextBox)
				{
					_doubleTappedEventHandler = new DoubleTappedEventHandler(OnTap);
					_container.AddHandler(FrameworkElement.DoubleTappedEvent, _doubleTappedEventHandler, true);
				}
				else
				{
					_container.DoubleTapped += OnTap;
				}
			}
			else
			{
				if (_control is not null && PreventGestureBubbling)
				{
					_subscriptionFlags |= SubscriptionFlags.ControlDoubleTapEventSubscribed;
					_control.DoubleTapped += HandleDoubleTapped;
				}
			}

			bool hasPointerGesture = ElementGestureRecognizers.HasAnyGesturesFor<PointerGestureRecognizer>();

			if (hasPointerGesture)
			{
				SubscribePointerEvents(_container);
			}

			bool hasSwipeGesture = gestures.HasAnyGesturesFor<SwipeGestureRecognizer>();
			bool hasPinchGesture = gestures.HasAnyGesturesFor<PinchGestureRecognizer>();
			bool hasPanGesture = gestures.HasAnyGesturesFor<PanGestureRecognizer>();

			if (!hasSwipeGesture && !hasPinchGesture && !hasPanGesture)
			{
				return;
			}

			// We can't handle ManipulationMode.Scale and System, so we don't support pinch/pan on a scroll view
			if (Element is ScrollView)
			{
				var logger = Application.Current?.FindMauiContext()?.CreateLogger<GesturePlatformManager>();
				if (hasPinchGesture)
				{
					logger?.LogWarning("PinchGestureRecognizer is not supported on a ScrollView in Windows Platforms");
				}

				if (hasPanGesture)
				{
					logger?.LogWarning("PanGestureRecognizer is not supported on a ScrollView in Windows Platforms");
				}

				if (hasSwipeGesture)
				{
					logger?.LogWarning("SwipeGestureRecognizer is not supported on a ScrollView in Windows Platforms");
				}

				return;
			}

			// Pan, pinch, and swipe gestures need pointer events if not subscribed yet.
			if (!hasPointerGesture)
			{
				SubscribePointerEvents(_container);
			}

			_subscriptionFlags |= SubscriptionFlags.ContainerManipulationAndPointerEventsSubscribed;
			_container.ManipulationMode = ManipulationModes.Scale | ManipulationModes.TranslateX | ManipulationModes.TranslateY;
			_container.ManipulationDelta += OnManipulationDelta;
			_container.ManipulationStarted += OnManipulationStarted;
			_container.ManipulationCompleted += OnManipulationCompleted;
			_container.PointerCanceled += OnPointerCanceled;
		}

		void SubscribePointerEvents(FrameworkElement container)
		{
			_subscriptionFlags |= SubscriptionFlags.ContainerPgrPointerEventsSubscribed;

			container.PointerEntered += OnPgrPointerEntered;
			container.PointerExited += OnPgrPointerExited;
			container.PointerMoved += OnPgrPointerMoved;
			container.PointerPressed += OnPgrPointerPressed;
			container.PointerReleased += OnPgrPointerReleased;
		}

		void HandleTapped(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
		{
			tappedRoutedEventArgs.Handled = true;
		}

		void HandleDoubleTapped(object sender, DoubleTappedRoutedEventArgs doubleTappedRoutedEventArgs)
		{
			doubleTappedRoutedEventArgs.Handled = true;
		}

		void HandleDragAndDropGesturePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == DragGestureRecognizer.CanDragProperty.PropertyName ||
				e.PropertyName == DropGestureRecognizer.AllowDropProperty.PropertyName)
			{
				UpdateDragAndDropGestureRecognizers();
			}
		}

		DragEventArgs ToDragEventArgs(UI.Xaml.DragEventArgs e, PlatformDragEventArgs platformArgs)
		{
			// The package should never be null here since the UI.Xaml.DragEventArgs have already been initialized
			var package = e.DataView.Properties[_doNotUsePropertyString] as DataPackage;

			return new DragEventArgs(package!, (relativeTo) => GetPosition(relativeTo, e), platformArgs);
		}

		[Flags]
		enum SubscriptionFlags : byte
		{
			None = 0,
			ContainerDragEventsSubscribed = 1,
			ContainerDropEventsSubscribed = 1 << 1,
			ContainerPgrPointerEventsSubscribed = 1 << 2,
			ContainerManipulationAndPointerEventsSubscribed = 1 << 3,
			ContainerTapAndRightTabEventSubscribed = 1 << 4,
			ContainerDoubleTapEventSubscribed = 1 << 5,
			ControlTapEventSubscribed = 1 << 6,
			ControlDoubleTapEventSubscribed = 1 << 7
		}
	}
}
