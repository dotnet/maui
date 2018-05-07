using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.WPF
{
	public abstract class VisualElementTracker : IDisposable
	{
		public abstract void Dispose();

		public event EventHandler Updated;

		protected void OnUpdated()
		{
			Updated?.Invoke(this, EventArgs.Empty);
		}
	}

	public class VisualElementTracker<TElement, TNativeElement> : VisualElementTracker where TElement : VisualElement where TNativeElement : FrameworkElement
	{
		bool _disposed;
		TNativeElement _control;
		TElement _element;

		bool _invalidateArrangeNeeded;
		bool _isPanning;
		bool _isPinching;

		bool _touchFrameReportedEventSet;
		int _touchPoints = 1;

		public TNativeElement Control
		{
			get { return _control; }
			set
			{
				if (_control == value)
					return;

				if (_control != null)
				{
					_control.MouseLeftButtonUp -= MouseLeftButtonUp;
					_control.ManipulationDelta -= OnManipulationDelta;
					_control.ManipulationCompleted -= OnManipulationCompleted;
				}

				_control = value;

				if (_control != null)
				{
					_control.MouseLeftButtonUp += MouseLeftButtonUp;
					_control.ManipulationDelta += OnManipulationDelta;
					_control.ManipulationCompleted += OnManipulationCompleted;
				}
				UpdateNativeControl();
			}
		}

		public TElement Element
		{
			get { return _element; }
			set
			{
				if (_element == value)
					return;

				if (_element != null)
				{
					_element.BatchCommitted -= HandleRedrawNeeded;
					_element.PropertyChanged -= HandlePropertyChanged;
				}

				_element = value;

				if (_element != null)
				{
					_element.BatchCommitted += HandleRedrawNeeded;
					_element.PropertyChanged += HandlePropertyChanged;
				}

				UpdateNativeControl();
			}
		}

		private void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			var fe = (sender as FrameworkElement);
			var vr = (sender as DefaultViewRenderer)?.Element;

			if ((fe != null && !fe.IsEnabled) || (vr != null && !vr.IsEnabled))
				return;

			e.Handled = ElementOnTap(e.ClickCount, e.GetPosition(fe));
		}

		protected virtual void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (Element.Batched)
			{
				if (e.PropertyName == VisualElement.XProperty.PropertyName ||
					e.PropertyName == VisualElement.YProperty.PropertyName ||
					e.PropertyName == VisualElement.WidthProperty.PropertyName ||
					e.PropertyName == VisualElement.HeightProperty.PropertyName)
					_invalidateArrangeNeeded = true;
				return;
			}

			if (e.PropertyName == VisualElement.XProperty.PropertyName ||
				e.PropertyName == VisualElement.YProperty.PropertyName ||
				e.PropertyName == VisualElement.WidthProperty.PropertyName ||
				e.PropertyName == VisualElement.HeightProperty.PropertyName)
				MaybeInvalidate();
			else if (e.PropertyName == VisualElement.AnchorXProperty.PropertyName ||
				e.PropertyName == VisualElement.AnchorYProperty.PropertyName ||
				e.PropertyName == VisualElement.ScaleProperty.PropertyName ||
				e.PropertyName == VisualElement.TranslationXProperty.PropertyName ||
				e.PropertyName == VisualElement.TranslationYProperty.PropertyName ||
				e.PropertyName == VisualElement.RotationProperty.PropertyName ||
				e.PropertyName == VisualElement.RotationXProperty.PropertyName ||
				e.PropertyName == VisualElement.RotationYProperty.PropertyName)
				UpdateScaleAndTranslateAndRotation();
			else if (e.PropertyName == VisualElement.IsVisibleProperty.PropertyName)
				UpdateVisibility();
			else if (e.PropertyName == VisualElement.OpacityProperty.PropertyName)
				UpdateOpacity();
			else if (e.PropertyName == VisualElement.InputTransparentProperty.PropertyName)
				UpdateInputTransparent();
		}

		protected virtual void UpdateNativeControl()
		{
			if (Element == null || Control == null)
				return;

			UpdateOpacity();
			UpdateScaleAndTranslateAndRotation();
			UpdateInputTransparent();
			UpdateVisibility();

			if (_invalidateArrangeNeeded)
				MaybeInvalidate();
			_invalidateArrangeNeeded = false;

			UpdateTouchFrameReportedEvent();

			OnUpdated();
		}

		bool ElementOnTap(int numberOfTapsRequired, System.Windows.Point tapPosition)
		{
			var view = Element as View;
			if (view == null)
				return false;

			var handled = false;

			var children = (view as IGestureController)?.GetChildElements(new Point(tapPosition.X, tapPosition.Y));

			if (children != null)
				foreach (var recognizer in children.GetChildGesturesFor<TapGestureRecognizer>().Where(g => g.NumberOfTapsRequired == numberOfTapsRequired))
				{
					recognizer.SendTapped(view);
					handled = true;
				}

			if (handled)
				return handled;

			foreach (var gestureRecognizer in
				view.GestureRecognizers.OfType<TapGestureRecognizer>().Where(g => g.NumberOfTapsRequired == numberOfTapsRequired))
			{
				gestureRecognizer.SendTapped(view);
				handled = true;
			}
			return handled;
		}

		void HandlePan(ManipulationDeltaEventArgs e, View view)
		{
			foreach (PanGestureRecognizer recognizer in
				view.GestureRecognizers.GetGesturesFor<PanGestureRecognizer>().Where(g => g.TouchPoints == _touchPoints))
			{
				if (!_isPanning)
					((IPanGestureController)recognizer).SendPanStarted(view, Application.Current.PanGestureId);

				double totalX = 0;
				double totalY = 0;

				((IPanGestureController)recognizer).SendPan(view, totalX, totalY, Application.Current.PanGestureId);
				_isPanning = true;
			}
		}

		void HandleRedrawNeeded(object sender, EventArgs e)
		{
			UpdateNativeControl();
		}

		void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
		{
			var view = Element as View;
			if (view == null)
				return;

			IEnumerable pinchGestures = view.GestureRecognizers.GetGesturesFor<PinchGestureRecognizer>();
			foreach (var recognizer in pinchGestures)
				((IPinchGestureController)recognizer).SendPinchEnded(view);
			_isPinching = false;

			IEnumerable<PanGestureRecognizer> panGestures = view.GestureRecognizers.GetGesturesFor<PanGestureRecognizer>().Where(g => g.TouchPoints == _touchPoints);
			foreach (PanGestureRecognizer recognizer in panGestures)
				((IPanGestureController)recognizer).SendPanCompleted(view, Application.Current.PanGestureId);
			Application.Current.PanGestureId++;
			_isPanning = false;
		}

		void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
		{
			var view = Element as View;
			if (view == null)
				return;

			HandlePan(e, view);
		}

		void Touch_FrameReported(object sender, TouchFrameEventArgs e)
		{
			_touchPoints = e.GetTouchPoints(Control).Count;
		}

		void MaybeInvalidate()
		{
			if (Element.IsInNativeLayout)
				return;
			var parent = (FrameworkElement)Control.Parent;
			parent?.InvalidateMeasure();
			Control.InvalidateMeasure();
		}

		void UpdateInputTransparent()
		{
			Control.IsHitTestVisible = !Element.InputTransparent;
		}

		void UpdateOpacity()
		{
			Control.Opacity = Element.Opacity;
		}

		void UpdateScaleAndTranslateAndRotation()
		{
			// TODO : Implement plane projection - Don't exist in WPF framework :( 
			double anchorX = Element.AnchorX;
			double anchorY = Element.AnchorY;
			double rotationX = Element.RotationX;
			double rotationY = Element.RotationY;
			double rotation = Element.Rotation;
			double translationX = Element.TranslationX;
			double translationY = Element.TranslationY;
			double scale = Element.Scale;

			double offsetX = scale == 0 ? 0 : translationX / scale;
			double offsetY = scale == 0 ? 0 : translationY / scale;

			Control.RenderTransformOrigin = new System.Windows.Point(anchorX, anchorY);
			Control.RenderTransform = new TransformGroup()
			{
				Children = new TransformCollection()
				{
					new ScaleTransform
					{
						ScaleX = scale,
						ScaleY = scale
					},
					new TranslateTransform()
					{
						X = offsetX,
						Y = offsetY
					},
					new RotateTransform()
					{

						CenterX = anchorX,
						CenterY = anchorY,
						Angle = Element.Rotation,
					}
				}
			};
		}

		void UpdateTouchFrameReportedEvent()
		{
			if (_touchFrameReportedEventSet)
				return;

			Touch.FrameReported -= Touch_FrameReported;
			_touchFrameReportedEventSet = false;

			var view = Element as View;
			if (view == null)
				return;

			if (!view.GestureRecognizers.GetGesturesFor<PanGestureRecognizer>().Any(g => g.TouchPoints > 1))
				return;

			Touch.FrameReported += Touch_FrameReported;
			_touchFrameReportedEventSet = true;
		}

		void UpdateVisibility()
		{
			Control.Visibility = Element.IsVisible ? Visibility.Visible : Visibility.Collapsed;
		}

		public override void Dispose()
		{
			if (_disposed)
				return;
			_disposed = true;

			if (_control != null)
			{
				_control.MouseLeftButtonUp -= MouseLeftButtonUp;
				_control.ManipulationDelta -= OnManipulationDelta;
				_control.ManipulationCompleted -= OnManipulationCompleted;
			}

			if (_element != null)
			{
				_element.BatchCommitted -= HandleRedrawNeeded;
				_element.PropertyChanged -= HandlePropertyChanged;
			}

			Element = null;
			Control = null;
		}
	}
}
