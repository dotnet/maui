#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Maui.Controls.Internals;
using WCompositeTransform = Microsoft.UI.Xaml.Media.CompositeTransform;
using WScaleTransform = Microsoft.UI.Xaml.Media.ScaleTransform;
using Microsoft.Maui.Graphics;
using WVisibility = Microsoft.UI.Xaml.Visibility;

namespace Microsoft.Maui.Controls.Platform
{
	// TODO MAUI: can we convert this over to using IView
	public class VisualElementTracker<TElement, TNativeElement> : IDisposable where TElement : VisualElement where TNativeElement : FrameworkElement
	{
		readonly NotifyCollectionChangedEventHandler _collectionChangedHandler;
		readonly List<uint> _fingers = new List<uint>();
		FrameworkElement? _container;
		TNativeElement? _control;
		TElement? _element;

		bool _invalidateArrangeNeeded = false;

		bool _isDisposed = false;
		bool _isPanning = false;
		bool _isSwiping = false;
		bool _isPinching = false;
		bool _wasPanGestureStartedSent = false;
		bool _wasPinchGestureStartedSent = false;
		public event EventHandler? Updated;

		static bool HasClip;

		public VisualElementTracker()
		{
			_collectionChangedHandler = ModelGestureRecognizersOnCollectionChanged;
		}

		public FrameworkElement? Container
		{
			get { return _container; }
			set
			{
				if (_container == value)
					return;

				ClearContainerEventHandlers();

				_container = value;

				UpdatingGestureRecognizers();

				UpdateNativeControl();
			}
		}

		public bool PreventGestureBubbling { get; set; }

		public TNativeElement? Control
		{
			get { return _control; }
			set
			{
				if (_control == value)
					return;

				if (_control != null)
				{
					_control.Tapped -= HandleTapped;
					_control.DoubleTapped -= HandleDoubleTapped;
				}

				_control = value;
				UpdateNativeControl();

				if (PreventGestureBubbling)
				{
					UpdatingGestureRecognizers();
				}
			}
		}

		void SendEventArgs<TRecognizer>(Action<TRecognizer> func)
		{
			if (_container == null && _control == null)
				return;

			var view = Element as View;
			var gestures =
				view?
					.GestureRecognizers?
					.OfType<TRecognizer>();

			if (gestures == null)
				return;

			foreach (var gesture in gestures)
			{
				func(gesture);
			}
		}

		void HandleDragLeave(object sender, Microsoft.UI.Xaml.DragEventArgs e)
		{
			var package = e.DataView.Properties["_XFPropertes_DONTUSE"] as DataPackage;
			var dragEventArgs = new DragEventArgs(package);

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
				// to remain even after the the dragged element has left
				if (operationPriorToSend != dragEventArgs.AcceptedOperation)
				{
					var result = (int)dragEventArgs.AcceptedOperation;
					e.AcceptedOperation = (Windows.ApplicationModel.DataTransfer.DataPackageOperation)result;
				}
			});
		}

		void HandleDragOver(object sender, Microsoft.UI.Xaml.DragEventArgs e)
		{
			var package = e.DataView.Properties["_XFPropertes_DONTUSE"] as DataPackage;
			var dragEventArgs = new DragEventArgs(package);

			SendEventArgs<DropGestureRecognizer>(rec =>
			{
				if(!rec.AllowDrop)
				{
					e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.None;
					return;
				}

				rec.SendDragOver(dragEventArgs);
				var result = (int)dragEventArgs.AcceptedOperation;
				e.AcceptedOperation = (Windows.ApplicationModel.DataTransfer.DataPackageOperation)result;
			});
		}

		void HandleDropCompleted(UIElement sender, Microsoft.UI.Xaml.DropCompletedEventArgs e)
		{
			var args = new DropCompletedEventArgs();
			SendEventArgs<DragGestureRecognizer>(rec => rec.SendDropCompleted(args));
		}

		void HandleDrop(object sender, Microsoft.UI.Xaml.DragEventArgs e)
		{
			var datapackage = e.DataView.Properties["_XFPropertes_DONTUSE"] as DataPackage;

			var args = new DropEventArgs(datapackage?.View);
			SendEventArgs<DropGestureRecognizer>(async rec =>
			{
				if (!rec.AllowDrop)
					return;

				try
				{
					await rec.SendDrop(args);
				}
				catch (Exception dropExc)
				{
					Internals.Log.Warning(nameof(DropGestureRecognizer), $"{dropExc}");
				}
			});
		}

		void HandleDragStarting(UIElement sender, Microsoft.UI.Xaml.DragStartingEventArgs e)
		{
			SendEventArgs<DragGestureRecognizer>(rec =>
			{
				if (!rec.CanDrag)
				{
					e.Cancel = true;
					return;
				}

				var renderer = sender as IViewHandler;
				var args = rec.SendDragStarting(renderer?.VirtualView);
				e.Data.Properties["_XFPropertes_DONTUSE"] = args.Data;

				if (!args.Handled && renderer != null)
				{
					if (renderer.NativeView is Microsoft.UI.Xaml.Controls.Image nativeImage &&
						nativeImage.Source is BitmapImage bi && bi.UriSource != null)
					{
						e.Data.SetBitmap(RandomAccessStreamReference.CreateFromUri(bi.UriSource));
					}
					else if(!String.IsNullOrWhiteSpace(args.Data.Text))
					{
						Uri? uri;
						if (Uri.TryCreate(args.Data.Text, UriKind.Absolute, out uri))
						{
							if (args.Data.Text.StartsWith("http", StringComparison.OrdinalIgnoreCase))
								e.Data.SetWebLink(uri);
							else
								e.Data.SetApplicationLink(uri);
						}
						else
						{
							e.Data.SetText(args.Data.Text);
						}
					}
				}

				e.Cancel = args.Cancel;
				e.AllowedOperations = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
			});
		}

		public TElement? Element
		{
			get { return _element; }
			set
			{
				if (_element == value)
					return;

				if (_element != null)
				{
					_element.BatchCommitted -= OnRedrawNeeded;
					_element.PropertyChanged -= OnPropertyChanged;

					var view = _element as View;
					if (view != null)
					{
						var oldRecognizers = (ObservableCollection<IGestureRecognizer>)view.GestureRecognizers;
						oldRecognizers.CollectionChanged -= _collectionChangedHandler;
						var gestures = (view as IGestureController)?.CompositeGestureRecognizers as ObservableCollection<IGestureRecognizer>;

						if(gestures != null)
							gestures.CollectionChanged -= _collectionChangedHandler;
					}
				}

				_element = value;

				if (_element != null)
				{
					_element.BatchCommitted += OnRedrawNeeded;
					_element.PropertyChanged += OnPropertyChanged;

					var view = _element as View;
					if (view != null)
					{
						var newRecognizers = (ObservableCollection<IGestureRecognizer>)view.GestureRecognizers;
						newRecognizers.CollectionChanged += _collectionChangedHandler;

						var gestures = (view as IGestureController)?.CompositeGestureRecognizers as ObservableCollection<IGestureRecognizer>;
						if (gestures != null)
							gestures.CollectionChanged += _collectionChangedHandler;
					}
				}

				UpdateNativeControl();
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
				_container.DragStarting -= HandleDragStarting;
				_container.DropCompleted -= HandleDropCompleted;
				_container.DragOver -= HandleDragOver;
				_container.Drop -= HandleDrop;
				_container.Tapped -= OnTap;
				_container.DoubleTapped -= OnDoubleTap;
				_container.ManipulationDelta -= OnManipulationDelta;
				_container.ManipulationStarted -= OnManipulationStarted;
				_container.ManipulationCompleted -= OnManipulationCompleted;
				_container.PointerPressed -= OnPointerPressed;
				_container.PointerExited -= OnPointerExited;
				_container.PointerReleased -= OnPointerReleased;
				_container.PointerCanceled -= OnPointerCanceled;
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			_isDisposed = true;

			if (!disposing)
				return;

			ClearContainerEventHandlers();

			if (_element != null)
			{
				_element.BatchCommitted -= OnRedrawNeeded;
				_element.PropertyChanged -= OnPropertyChanged;

				var view = _element as View;
				if (view != null)
				{
					var oldRecognizers = (ObservableCollection<IGestureRecognizer>)view.GestureRecognizers;
					oldRecognizers.CollectionChanged -= _collectionChangedHandler;
				}
			}

			if (_control != null)
			{
				_control.Tapped -= HandleTapped;
				_control.DoubleTapped -= HandleDoubleTapped;
			}

			Control = null;
			Element = null;
			Container = null;
		}

		protected virtual void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (Element == null)
				return;

			if (Element.Batched)
			{
				if (e.PropertyName == VisualElement.XProperty.PropertyName || e.PropertyName == VisualElement.YProperty.PropertyName || e.PropertyName == VisualElement.WidthProperty.PropertyName ||
					e.PropertyName == VisualElement.HeightProperty.PropertyName)
				{
					_invalidateArrangeNeeded = true;
				}
				return;
			}

			if (Container == null)
				return;

			if (e.PropertyName == VisualElement.XProperty.PropertyName || e.PropertyName == VisualElement.YProperty.PropertyName || e.PropertyName == VisualElement.WidthProperty.PropertyName ||
				e.PropertyName == VisualElement.HeightProperty.PropertyName)
			{
				MaybeInvalidate();
			}


			if (e.PropertyName == VisualElement.AnchorXProperty.PropertyName || e.PropertyName == VisualElement.AnchorYProperty.PropertyName)
			{
				UpdateScaleAndRotation(Element, Container);
			}
			else if (e.PropertyName == VisualElement.ScaleProperty.PropertyName || e.PropertyName == VisualElement.ScaleXProperty.PropertyName || e.PropertyName == VisualElement.ScaleYProperty.PropertyName)
			{
				UpdateScaleAndRotation(Element, Container);
			}
			else if (e.PropertyName == VisualElement.TranslationXProperty.PropertyName || e.PropertyName == VisualElement.TranslationYProperty.PropertyName ||
					 e.PropertyName == VisualElement.RotationProperty.PropertyName || e.PropertyName == VisualElement.RotationXProperty.PropertyName || e.PropertyName == VisualElement.RotationYProperty.PropertyName)
			{
				UpdateRotation(Element, Container);
			}
			else if (e.PropertyName == VisualElement.IsVisibleProperty.PropertyName)
			{
				UpdateVisibility(Element, Container);
			}
			else if (e.PropertyName == VisualElement.OpacityProperty.PropertyName)
			{
				UpdateOpacity(Element, Container);
			}
			else if (e.PropertyName == VisualElement.InputTransparentProperty.PropertyName)
			{
				UpdateInputTransparent(Element, Container);
			}
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
			{
				UpdateInputTransparent(Element, Container);
			}
			else if (e.PropertyName == VisualElement.ClipProperty.PropertyName)
			{
				UpdateClip(Element, Container);
			}
		}

		protected virtual void UpdateNativeControl()
		{
			if (Element == null || Container == null)
				return;

			UpdateVisibility(Element, Container);
			UpdateOpacity(Element, Container);
			UpdateScaleAndRotation(Element, Container);
			UpdateInputTransparent(Element, Container);
			UpdateClip(Element, Container);

			if (_invalidateArrangeNeeded)
			{
				MaybeInvalidate();
			}
			_invalidateArrangeNeeded = false;

			OnUpdated();
		}

		void HandleSwipe(ManipulationDeltaRoutedEventArgs e, View view)
		{
			if (_fingers.Count > 1 || view == null)
				return;

			_isSwiping = true;

			foreach (SwipeGestureRecognizer recognizer in view.GestureRecognizers.GetGesturesFor<SwipeGestureRecognizer>())
			{
				((ISwipeGestureController)recognizer).SendSwipe(view, e.Delta.Translation.X + e.Cumulative.Translation.X, e.Delta.Translation.Y + e.Cumulative.Translation.Y);
			}
		}

		void HandlePan(ManipulationDeltaRoutedEventArgs e, View view)
		{
			if (view == null)
				return;

			_isPanning = true;

			foreach (PanGestureRecognizer recognizer in view.GestureRecognizers.GetGesturesFor<PanGestureRecognizer>().Where(g => g.TouchPoints == _fingers.Count))
			{
				if (!_wasPanGestureStartedSent)
				{
					recognizer.SendPanStarted(view, Application.Current.PanGestureId);
				}
				recognizer.SendPan(view, e.Delta.Translation.X + e.Cumulative.Translation.X, e.Delta.Translation.Y + e.Cumulative.Translation.Y, Application.Current.PanGestureId);
			}
			_wasPanGestureStartedSent = true;
		}

		void HandlePinch(ManipulationDeltaRoutedEventArgs e, View view)
		{
			if (_fingers.Count < 2 || view == null)
				return;

			_isPinching = true;

			Windows.Foundation.Point translationPoint = e.Container.TransformToVisual(Container).TransformPoint(e.Position);

			var scaleOriginPoint = new Point(translationPoint.X / view.Width, translationPoint.Y / view.Height);
			IEnumerable<PinchGestureRecognizer> pinchGestures = view.GestureRecognizers.GetGesturesFor<PinchGestureRecognizer>();
			foreach (PinchGestureRecognizer recognizer in pinchGestures)
			{
				if (!_wasPinchGestureStartedSent)
				{
					recognizer.SendPinchStarted(view, scaleOriginPoint);
				}
				recognizer.SendPinch(view, e.Delta.Scale, scaleOriginPoint);
			}
			_wasPinchGestureStartedSent = true;
		}

		void MaybeInvalidate()
		{
			if (Element?.IsInNativeLayout == true)
				return;

			var parent = (FrameworkElement?)Container?.Parent;
			parent?.InvalidateMeasure();
			Container?.InvalidateMeasure();
		}

		void ModelGestureRecognizersOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			UpdatingGestureRecognizers();
		}

		void OnDoubleTap(object sender, DoubleTappedRoutedEventArgs e)
		{
			var view = Element as View;
			if (view == null)
				return;

			var tapPosition = e.GetPosition(Control);
			var children = (view as IGestureController)?.GetChildElements(new Point(tapPosition.X, tapPosition.Y));

			if (children != null)
				foreach (var recognizer in children.GetChildGesturesFor<TapGestureRecognizer>(g => g.NumberOfTapsRequired == 1 || g.NumberOfTapsRequired == 2))
				{
					recognizer.SendTapped(view);
					e.Handled = true;
				}

			if (e.Handled)
				return;

			IEnumerable<TapGestureRecognizer> doubleTapGestures = view.GestureRecognizers.GetGesturesFor<TapGestureRecognizer>(g => g.NumberOfTapsRequired == 1 || g.NumberOfTapsRequired == 2);
			foreach (TapGestureRecognizer recognizer in doubleTapGestures)
			{
				recognizer.SendTapped(view);
				e.Handled = true;
			}
		}

		void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
		{
			SwipeComplete(true);
			PinchComplete(true);
			PanComplete(true);
		}

		void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
		{
			var view = Element as View;
			if (view == null)
				return;
			HandleSwipe(e, view);
			HandlePinch(e, view);
			HandlePan(e, view);
		}

		void OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
		{
			var view = Element as View;
			if (view == null)
				return;
			_wasPinchGestureStartedSent = false;
			_wasPanGestureStartedSent = false;
		}

		void OnPointerCanceled(object sender, PointerRoutedEventArgs e)
		{
			uint id = e.Pointer.PointerId;
			if (_fingers.Contains(id))
				_fingers.Remove(id);
			SwipeComplete(false);
			PinchComplete(false);
			PanComplete(false);
		}

		void OnPointerExited(object sender, PointerRoutedEventArgs e)
		{
			uint id = e.Pointer.PointerId;
			if (_fingers.Contains(id))
				_fingers.Remove(id);
			SwipeComplete(true);
			PinchComplete(true);
			PanComplete(true);
		}

		void OnPointerPressed(object sender, PointerRoutedEventArgs e)
		{
			uint id = e.Pointer.PointerId;
			if (!_fingers.Contains(id))
				_fingers.Add(id);
		}

		void OnPointerReleased(object? sender, PointerRoutedEventArgs e)
		{
			uint id = e.Pointer.PointerId;
			if (_fingers.Contains(id))
				_fingers.Remove(id);
			SwipeComplete(true);
			PinchComplete(true);
			PanComplete(true);
		}

		void OnRedrawNeeded(object? sender, EventArgs e)
		{
			UpdateNativeControl();
		}

		void OnTap(object? sender, TappedRoutedEventArgs e)
		{
			var view = Element as View;
			if (view == null)
				return;

			var tapPosition = e.GetPosition(Control);
			var children = (view as IGestureController)?.GetChildElements(new Point(tapPosition.X, tapPosition.Y));

			if (children != null)
				foreach (var recognizer in children.GetChildGesturesFor<TapGestureRecognizer>(g => g.NumberOfTapsRequired == 1))
				{
					recognizer.SendTapped(view);
					e.Handled = true;
				}

			if (e.Handled)
				return;

			IEnumerable<TapGestureRecognizer> tapGestures = view.GestureRecognizers.GetGesturesFor<TapGestureRecognizer>(g => g.NumberOfTapsRequired == 1);
			foreach (var recognizer in tapGestures)
			{
				recognizer.SendTapped(view);
				e.Handled = true;
			}
		}

		void SwipeComplete(bool success)
		{
			var view = Element as View;
			if (view == null || !_isSwiping)
				return;

			if (success)
			{
				foreach (SwipeGestureRecognizer recognizer in view.GestureRecognizers.GetGesturesFor<SwipeGestureRecognizer>())
				{
					((ISwipeGestureController)recognizer).DetectSwipe(view, recognizer.Direction);
				}
			}

			_isSwiping = false;
		}

		void OnUpdated()
		{
			if (Updated != null)
				Updated(this, EventArgs.Empty);
		}

		void PanComplete(bool success)
		{
			var view = Element as View;
			if (view == null || !_isPanning)
				return;

			foreach (PanGestureRecognizer recognizer in view.GestureRecognizers.GetGesturesFor<PanGestureRecognizer>().Where(g => g.TouchPoints == _fingers.Count))
			{
				if (success)
				{
					recognizer.SendPanCompleted(view, Application.Current.PanGestureId);
				}
				else
				{
					recognizer.SendPanCanceled(view, Application.Current.PanGestureId);
				}
			}

			Application.Current.PanGestureId++;
			_isPanning = false;
		}

		void PinchComplete(bool success)
		{
			var view = Element as View;
			if (view == null || !_isPinching)
				return;

			IEnumerable<PinchGestureRecognizer> pinchGestures = view.GestureRecognizers.GetGesturesFor<PinchGestureRecognizer>();
			foreach (PinchGestureRecognizer recognizer in pinchGestures)
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

		static void UpdateInputTransparent(VisualElement view, FrameworkElement frameworkElement)
		{
			if (view is Layout)
			{
				// Let VisualElementRenderer handle this
			}

			frameworkElement.IsHitTestVisible = view.IsEnabled && !view.InputTransparent;
		}

		static void UpdateClip(VisualElement view, FrameworkElement frameworkElement)
		{
			if (!ShouldUpdateClip(view, frameworkElement))
				return;

			var geometry = view.Clip;

			HasClip = geometry != null;

			if (CompositionHelper.IsCompositionGeometryTypePresent)
				frameworkElement.ClipVisual(geometry);
			else
				frameworkElement.Clip(geometry);
		}

		static bool ShouldUpdateClip(VisualElement view, FrameworkElement frameworkElement)
		{
			if (view == null || frameworkElement == null)
				return false;

			var formsGeometry = view.Clip;

			if (formsGeometry != null)
				return true;

			if (formsGeometry == null && HasClip)
				return true;

			return false;
		}

		static void UpdateOpacity(VisualElement view, FrameworkElement frameworkElement)
		{
			frameworkElement.Opacity = view.Opacity;
		}

		static void UpdateRotation(VisualElement view, FrameworkElement frameworkElement)
		{
			double anchorX = view.AnchorX;
			double anchorY = view.AnchorY;
			double rotationX = view.RotationX;
			double rotationY = view.RotationY;
			double rotation = view.Rotation;
			double translationX = view.TranslationX;
			double translationY = view.TranslationY;
			double scaleX = view.Scale * view.ScaleX;
			double scaleY = view.Scale * view.ScaleY;

			if (rotationX % 360 == 0 && rotationY % 360 == 0 && rotation % 360 == 0 && translationX == 0 && translationY == 0 && scaleX == 1 && scaleY == 1)
			{
				frameworkElement.Projection = null;
				frameworkElement.RenderTransform = null;
			}
			else
			{
				// PlaneProjection removes touch and scrollwheel functionality on scrollable views such
				// as ScrollView, ListView, and TableView. If neither RotationX or RotationY are set
				// (i.e. their absolute value is 0), a CompositeTransform is instead used to allow for
				// rotation of the control on a 2D plane, and the other values are set. Otherwise, the
				// rotation values are set, but the aforementioned functionality will be lost.
				if (Math.Abs(view.RotationX) != 0 || Math.Abs(view.RotationY) != 0)
				{
					frameworkElement.Projection = new PlaneProjection
					{
						CenterOfRotationX = anchorX,
						CenterOfRotationY = anchorY,
						GlobalOffsetX = translationX,
						GlobalOffsetY = translationY,
						RotationX = -rotationX,
						RotationY = -rotationY,
						RotationZ = -rotation
					};
				}
				else
				{
					frameworkElement.RenderTransform = new WCompositeTransform
					{
						CenterX = anchorX,
						CenterY = anchorY,
						Rotation = rotation,
						ScaleX = scaleX,
						ScaleY = scaleY,
						TranslateX = translationX,
						TranslateY = translationY
					};
				}
			}
		}

		static void UpdateScaleAndRotation(VisualElement view, FrameworkElement frameworkElement)
		{
			double anchorX = view.AnchorX;
			double anchorY = view.AnchorY;
			frameworkElement.RenderTransformOrigin = new Windows.Foundation.Point(anchorX, anchorY);
			frameworkElement.RenderTransform = new WScaleTransform { ScaleX = view.Scale * view.ScaleX, ScaleY = view.Scale * view.ScaleY };

			UpdateRotation(view, frameworkElement);
		}

		static void UpdateVisibility(VisualElement view, FrameworkElement frameworkElement)
		{
			frameworkElement.Visibility = view.IsVisible ? WVisibility.Visible : WVisibility.Collapsed;
		}

		void UpdateDragAndDropGestureRecognizers()
		{
			if (_container == null)
				return;

			var view = Element as View;
			IList<IGestureRecognizer>? gestures = view?.GestureRecognizers;

			if (gestures == null)
				return;

			_container.CanDrag = gestures.GetGesturesFor<DragGestureRecognizer>()
				.FirstOrDefault()?.CanDrag ?? false;

			_container.AllowDrop = gestures.GetGesturesFor<DropGestureRecognizer>()
				.FirstOrDefault()?.AllowDrop ?? false;

			if (_container.CanDrag)
			{
				_container.DragStarting += HandleDragStarting;
				_container.DropCompleted += HandleDropCompleted;
			}

			if(_container.AllowDrop)
			{
				_container.DragOver += HandleDragOver;
				_container.Drop += HandleDrop;
				_container.DragLeave += HandleDragLeave;
			}
		}

		void UpdatingGestureRecognizers()
		{
			var view = Element as View;
			IList<IGestureRecognizer>? gestures = view?.GestureRecognizers;

			if (_container == null || gestures == null)
				return;

			ClearContainerEventHandlers();
			UpdateDragAndDropGestureRecognizers();

			var children = (view as IGestureController)?.GetChildElements(Point.Zero);
			IList<TapGestureRecognizer>? childGestures = children?.GetChildGesturesFor<TapGestureRecognizer>().ToList();

			if (gestures.GetGesturesFor<TapGestureRecognizer>(g => g.NumberOfTapsRequired == 1).Any()
				|| children?.GetChildGesturesFor<TapGestureRecognizer>(g => g.NumberOfTapsRequired == 1).Any() == true)
			{
				_container.Tapped += OnTap;
			}
			else
			{
				if (_control != null && PreventGestureBubbling)
				{
					_control.Tapped += HandleTapped;
				}
			}

			if (gestures.GetGesturesFor<TapGestureRecognizer>(g => g.NumberOfTapsRequired == 1 || g.NumberOfTapsRequired == 2).Any()
				|| children?.GetChildGesturesFor<TapGestureRecognizer>(g => g.NumberOfTapsRequired == 1 || g.NumberOfTapsRequired == 2).Any() == true)
			{
				_container.DoubleTapped += OnDoubleTap;
			}
			else
			{
				if (_control != null && PreventGestureBubbling)
				{
					_control.DoubleTapped += HandleDoubleTapped;
				}
			}

			bool hasSwipeGesture = gestures.GetGesturesFor<SwipeGestureRecognizer>().GetEnumerator().MoveNext();
			bool hasPinchGesture = gestures.GetGesturesFor<PinchGestureRecognizer>().GetEnumerator().MoveNext();
			bool hasPanGesture = gestures.GetGesturesFor<PanGestureRecognizer>().GetEnumerator().MoveNext();
			if (!hasSwipeGesture && !hasPinchGesture && !hasPanGesture)
				return;

			//We can't handle ManipulationMode.Scale and System , so we don't support pinch/pan on a scrollview 
			if (Element is ScrollView)
			{
				if (hasPinchGesture)
					Log.Warning("Gestures", "PinchGestureRecognizer is not supported on a ScrollView in Windows Platforms");
				if (hasPanGesture)
					Log.Warning("Gestures", "PanGestureRecognizer is not supported on a ScrollView in Windows Platforms");
				if (hasSwipeGesture)
					Log.Warning("Gestures", "SwipeGestureRecognizer is not supported on a ScrollView in Windows Platforms");
				return;
			}

			_container.ManipulationMode = ManipulationModes.Scale | ManipulationModes.TranslateX | ManipulationModes.TranslateY;
			_container.ManipulationDelta += OnManipulationDelta;
			_container.ManipulationStarted += OnManipulationStarted;
			_container.ManipulationCompleted += OnManipulationCompleted;
			_container.PointerPressed += OnPointerPressed;
			_container.PointerExited += OnPointerExited;
			_container.PointerReleased += OnPointerReleased;
			_container.PointerCanceled += OnPointerCanceled;
		}

		void HandleTapped(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
		{
			tappedRoutedEventArgs.Handled = true;
		}

		void HandleDoubleTapped(object sender, DoubleTappedRoutedEventArgs doubleTappedRoutedEventArgs)
		{
			doubleTappedRoutedEventArgs.Handled = true;
		}
	}
}
