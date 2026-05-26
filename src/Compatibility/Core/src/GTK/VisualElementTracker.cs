using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Gtk;
using Microsoft.Maui.Controls.Compatibility.Internals;
using Microsoft.Maui.Controls.Compatibility.Platform.GTK.Extensions;

namespace Microsoft.Maui.Controls.Compatibility.Platform.GTK
{
	public class VisualElementTracker<TElement, TNativeElement> : IDisposable where TElement : VisualElement where TNativeElement : Widget
	{
		private bool _isDisposed;
		private TNativeElement _control;
		private TElement _element;
		private GtkFormsContainer _container;
		private bool _invalidateArrangeNeeded;

		private readonly NotifyCollectionChangedEventHandler _collectionChangedHandler;

		public event EventHandler Updated;

		public VisualElementTracker()
		{
			_collectionChangedHandler = ModelGestureRecognizersOnCollectionChanged;
		}

		public GtkFormsContainer Container
		{
			get { return _container; }
			set
			{
				if (_container == value)
					return;

				if (_container != null)
				{
					_container.ButtonPressEvent -= OnContainerButtonPressEvent;
				}

				_container = value;

				UpdatingGestureRecognizers();

				UpdateNativeControl();
			}
		}

		public TNativeElement Control
		{
			get { return _control; }
			set
			{
				if (_control == value)
					return;

				if (_control != null)
				{
					_control.ButtonPressEvent -= OnControlButtonPressEvent;
				}

				_control = value;
				UpdateNativeControl();

				if (PreventGestureBubbling)
				{
					UpdatingGestureRecognizers();
				}
			}
		}

		public bool PreventGestureBubbling { get; set; }

		public TElement Element
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
					}
				}

				UpdateNativeControl();
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

			if (_invalidateArrangeNeeded)
			{
				MaybeInvalidate();
			}
			_invalidateArrangeNeeded = false;

			OnUpdated();
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			_isDisposed = true;

			if (!disposing)
				return;

			if (_container != null)
			{
				_container.ButtonPressEvent -= OnContainerButtonPressEvent;
			}

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
				_control.ButtonPressEvent -= OnControlButtonPressEvent;
			}

			Container.Destroy();
			Container = null;
		}

		protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (Element.Batched)
			{
				if (e.PropertyName == VisualElement.XProperty.PropertyName ||
					e.PropertyName == VisualElement.YProperty.PropertyName ||
					e.PropertyName == VisualElement.WidthProperty.PropertyName ||
					e.PropertyName == VisualElement.HeightProperty.PropertyName)
				{
					_invalidateArrangeNeeded = true;
				}
				return;
			}

			if (e.PropertyName == VisualElement.AnchorXProperty.PropertyName ||
				e.PropertyName == VisualElement.AnchorYProperty.PropertyName)
			{
				UpdateScaleAndRotation(Element, Container);
			}
			else if (e.PropertyName == VisualElement.ScaleProperty.PropertyName)
			{
				UpdateScaleAndRotation(Element, Container);
			}
			else if (e.PropertyName == VisualElement.TranslationXProperty.PropertyName ||
				e.PropertyName == VisualElement.TranslationYProperty.PropertyName ||
					 e.PropertyName == VisualElement.RotationProperty.PropertyName ||
					 e.PropertyName == VisualElement.RotationXProperty.PropertyName ||
					 e.PropertyName == VisualElement.RotationYProperty.PropertyName)
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
		}

		private void ModelGestureRecognizersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			UpdatingGestureRecognizers();
		}

		private void OnUpdated()
		{
			Updated?.Invoke(this, EventArgs.Empty);
		}

		private void OnRedrawNeeded(object sender, EventArgs e)
		{
			UpdateNativeControl();
		}

		private void UpdatingGestureRecognizers()
		{
			var view = Element as View;
			IList<IGestureRecognizer> gestures = view?.GestureRecognizers;

			if (_container == null || gestures == null)
				return;

			_container.ButtonPressEvent -= OnContainerButtonPressEvent;

			if (gestures.GetGesturesFor<TapGestureRecognizer>().Any())
			{
				_container.ButtonPressEvent += OnContainerButtonPressEvent;
			}
			else
			{
				if (_control != null && PreventGestureBubbling)
				{
					_control.ButtonPressEvent += OnControlButtonPressEvent;
				}
			}

			bool hasPinchGesture = gestures.GetGesturesFor<PinchGestureRecognizer>().GetEnumerator().MoveNext();
			bool hasPanGesture = gestures.GetGesturesFor<PanGestureRecognizer>().GetEnumerator().MoveNext();

			if (!hasPinchGesture && !hasPanGesture)
				return;
		}

		private void MaybeInvalidate()
		{
			if (Element.IsInNativeLayout)
				return;

			var parent = Container.Parent;
			parent?.QueueDraw();
			Container.QueueDraw();
		}

		// TODO: Implement Scale
		private static void UpdateScaleAndRotation(VisualElement view, Gtk.Widget eventBox)
		{
			double anchorX = view.AnchorX;
			double anchorY = view.AnchorY;
			double scale = view.Scale;

			UpdateRotation(view, eventBox);
		}

		// TODO: Implement Rotation
		private static void UpdateRotation(VisualElement view, Gtk.Widget eventBox)
		{
			if (view == null)
				return;

			double anchorX = view.AnchorX;
			double anchorY = view.AnchorY;
			double rotationX = view.RotationX;
			double rotationY = view.RotationY;
			double rotation = view.Rotation;
			double translationX = view.TranslationX;
			double translationY = view.TranslationY;
			double scale = view.Scale;

			var viewRenderer = Platform.GetRenderer(view) as Widget;

			if (viewRenderer == null)
				return;

			if (rotationX % 360 == 0 &&
				rotationY % 360 == 0 &&
				rotation % 360 == 0 &&
				translationX == 0 &&
				translationY == 0 &&
				scale == 1)
			{
				return;
			}
			else
			{
				viewRenderer.MoveTo(
					scale == 0 ? 0 : translationX / scale,
					scale == 0 ? 0 : translationY / scale);
			}
		}

		private static void UpdateVisibility(VisualElement view, Gtk.Widget eventBox)
		{
			eventBox.Visible = view.IsVisible;
		}

		// TODO: Implement Opacity
		private static void UpdateOpacity(VisualElement view, Gtk.Widget eventBox)
		{

		}

		// TODO: Implement InputTransparent
		private static void UpdateInputTransparent(VisualElement view, Gtk.Widget eventBox)
		{

		}

		private void OnContainerButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			var button = args.Event.Button;
			if (button != 1 && button != 3)
			{
				return;
			}

			var view = Element as View;

			if (view == null)
				return;

			int numClicks = 0;
			switch (args.Event.Type)
			{
				case Gdk.EventType.ThreeButtonPress:
					numClicks = 3;
					break;
				case Gdk.EventType.TwoButtonPress:
					numClicks = 2;
					break;
				case Gdk.EventType.ButtonPress:
					numClicks = 1;
					break;
				default:
					return;
			}

			// Taps or Clicks
			if (button == 1)
			{
				IEnumerable<TapGestureRecognizer> tapGestures = view.GestureRecognizers
					.GetGesturesFor<TapGestureRecognizer>(g => g.NumberOfTapsRequired == numClicks);

				foreach (TapGestureRecognizer recognizer in tapGestures)
					recognizer.SendTapped(view);
			}
			else
			{
				// right click - no equivalent for TapGestureRecognizer
			}
		}

		private void OnControlButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			args.RetVal = true;
		}
	}
}
