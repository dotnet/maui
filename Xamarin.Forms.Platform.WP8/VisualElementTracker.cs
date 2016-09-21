using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Xamarin.Forms.Platform.WinPhone
{
	public abstract class VisualElementTracker : IDisposable
	{
		public abstract FrameworkElement Child { get; set; }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		public event EventHandler Updated;

		protected void OnUpdated()
		{
			if (Updated != null)
				Updated(this, EventArgs.Empty);
		}
	}

	public class VisualElementTracker<TModel, TElement> : VisualElementTracker where TModel : VisualElement where TElement : FrameworkElement
	{
		FrameworkElement _child;
		bool _disposed;
		TElement _element;

		bool _invalidateArrangeNeeded;
		bool _isPanning;
		bool _isPinching;

		TModel _model;
		bool _touchFrameReportedEventSet;
		int _touchPoints = 1;

		public override FrameworkElement Child
		{
			get { return _child; }
			set
			{
				if (_child == value)
					return;
				_child = value;
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
					_element.Tap -= ElementOnTap;
					_element.DoubleTap -= ElementOnDoubleTap;
					_element.ManipulationDelta -= OnManipulationDelta;
					_element.ManipulationCompleted -= OnManipulationCompleted;
				}

				_element = value;

				if (_element != null)
				{
					_element.Tap += ElementOnTap;
					_element.DoubleTap += ElementOnDoubleTap;
					_element.ManipulationDelta += OnManipulationDelta;
					_element.ManipulationCompleted += OnManipulationCompleted;
				}

				UpdateNativeControl();
			}
		}

		public TModel Model
		{
			get { return _model; }
			set
			{
				if (_model == value)
					return;

				if (_model != null)
				{
					_model.BatchCommitted -= HandleRedrawNeeded;
					_model.PropertyChanged -= HandlePropertyChanged;
				}

				_model = value;

				if (_model != null)
				{
					_model.BatchCommitted += HandleRedrawNeeded;
					_model.PropertyChanged += HandlePropertyChanged;
				}

				UpdateNativeControl();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				if (_element != null)
				{
					_element.Tap -= ElementOnTap;
					_element.DoubleTap -= ElementOnDoubleTap;
					_element.ManipulationDelta -= OnManipulationDelta;
					_element.ManipulationCompleted -= OnManipulationCompleted;
				}

				if (_model != null)
				{
					_model.BatchCommitted -= HandleRedrawNeeded;
					_model.PropertyChanged -= HandlePropertyChanged;
				}

				Child = null;
				Model = null;
				Element = null;
			}

			base.Dispose(disposing);
		}

		protected virtual void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (Model.Batched)
			{
				if (e.PropertyName == VisualElement.XProperty.PropertyName || e.PropertyName == VisualElement.YProperty.PropertyName || e.PropertyName == VisualElement.WidthProperty.PropertyName ||
					e.PropertyName == VisualElement.HeightProperty.PropertyName)
					_invalidateArrangeNeeded = true;
				return;
			}

			if (e.PropertyName == VisualElement.XProperty.PropertyName || e.PropertyName == VisualElement.YProperty.PropertyName || e.PropertyName == VisualElement.WidthProperty.PropertyName ||
				e.PropertyName == VisualElement.HeightProperty.PropertyName)
				MaybeInvalidate();
			else if (e.PropertyName == VisualElement.AnchorXProperty.PropertyName || e.PropertyName == VisualElement.AnchorYProperty.PropertyName)
				UpdateScaleAndRotation(Model, Element);
			else if (e.PropertyName == VisualElement.ScaleProperty.PropertyName)
				UpdateScaleAndRotation(Model, Element);
			else if (e.PropertyName == VisualElement.TranslationXProperty.PropertyName || e.PropertyName == VisualElement.TranslationYProperty.PropertyName ||
					 e.PropertyName == VisualElement.RotationProperty.PropertyName || e.PropertyName == VisualElement.RotationXProperty.PropertyName || e.PropertyName == VisualElement.RotationYProperty.PropertyName)
				UpdateRotation(Model, Element);
			else if (e.PropertyName == VisualElement.IsVisibleProperty.PropertyName)
				UpdateVisibility(Model, Element);
			else if (e.PropertyName == VisualElement.OpacityProperty.PropertyName)
				UpdateOpacity(Model, Element);
			else if (e.PropertyName == VisualElement.InputTransparentProperty.PropertyName)
				UpdateInputTransparent(Model, Element);
		}

		protected virtual void UpdateNativeControl()
		{
			if (Model == null || Element == null)
				return;

			UpdateOpacity(_model, _element);
			UpdateScaleAndRotation(_model, _element);
			UpdateInputTransparent(_model, _element);

			if (_invalidateArrangeNeeded)
				MaybeInvalidate();
			_invalidateArrangeNeeded = false;

			UpdateTouchFrameReportedEvent(_model);

			OnUpdated();
		}

		void ElementOnDoubleTap(object sender, GestureEventArgs gestureEventArgs)
		{
			var view = Model as View;
			if (view == null)
				return;

			foreach (TapGestureRecognizer gestureRecognizer in
				view.GestureRecognizers.OfType<TapGestureRecognizer>().Where(g => g.NumberOfTapsRequired == 2))
			{
				gestureRecognizer.SendTapped(view);
				gestureEventArgs.Handled = true;
			}
		}

		void ElementOnTap(object sender, GestureEventArgs gestureEventArgs)
		{
			var view = Model as View;
			if (view == null)
				return;

			foreach (TapGestureRecognizer gestureRecognizer in
				view.GestureRecognizers.OfType<TapGestureRecognizer>().Where(g => g.NumberOfTapsRequired == 1))
			{
				gestureRecognizer.SendTapped(view);
				gestureEventArgs.Handled = true;
			}
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

				// Translation and CumulativeManipulation will be 0 if we have more than one touch point because it thinks we're pinching,
				// so we'll just go ahead and use the center point of the pinch gesture to figure out how much we're panning.
				if (_touchPoints > 1 && e.PinchManipulation != null)
				{
					totalX = e.PinchManipulation.Current.Center.X - e.PinchManipulation.Original.Center.X;
					totalY = e.PinchManipulation.Current.Center.Y - e.PinchManipulation.Original.Center.Y;
				}
				else
				{
					totalX = e.DeltaManipulation.Translation.X + e.CumulativeManipulation.Translation.X;
					totalY = e.DeltaManipulation.Translation.Y + e.CumulativeManipulation.Translation.Y;
				}

				((IPanGestureController)recognizer).SendPan(view, totalX, totalY, Application.Current.PanGestureId);
				_isPanning = true;
			}
		}

		void HandlePinch(ManipulationDeltaEventArgs e, View view)
		{
			if (e.PinchManipulation == null)
				return;

			IEnumerable<PinchGestureRecognizer> pinchGestures = view.GestureRecognizers.GetGesturesFor<PinchGestureRecognizer>();
			System.Windows.Point translationPoint = e.ManipulationContainer.TransformToVisual(Element).Transform(e.PinchManipulation.Current.Center);
			var scaleOriginPoint = new Point(translationPoint.X / view.Width, translationPoint.Y / view.Height);
			foreach (var recognizer in pinchGestures)
			{
				if (!_isPinching)
					((IPinchGestureController)recognizer).SendPinchStarted(view, scaleOriginPoint);
				((IPinchGestureController)recognizer).SendPinch(view, e.PinchManipulation.DeltaScale, scaleOriginPoint);
			}
			_isPinching = true;
		}

		void HandleRedrawNeeded(object sender, EventArgs e)
		{
			UpdateNativeControl();
		}

		void MaybeInvalidate()
		{
			if (Model.IsInNativeLayout)
				return;
			var parent = (FrameworkElement)Element.Parent;
			parent?.InvalidateMeasure();
			Element.InvalidateMeasure();
		}

		void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
		{
			var view = Model as View;
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
			var view = Model as View;
			if (view == null)
				return;

			HandlePinch(e, view);

			HandlePan(e, view);
		}

		void Touch_FrameReported(object sender, TouchFrameEventArgs e)
		{
			_touchPoints = e.GetTouchPoints(Child).Count;
		}

		static void UpdateInputTransparent(VisualElement view, FrameworkElement frameworkElement)
		{
			frameworkElement.IsHitTestVisible = !view.InputTransparent;
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
			double scale = view.Scale;

			frameworkElement.Projection = new PlaneProjection
			{
				CenterOfRotationX = anchorX,
				CenterOfRotationY = anchorY,
				GlobalOffsetX = scale == 0 ? 0 : translationX / scale,
				GlobalOffsetY = scale == 0 ? 0 : translationY / scale,
				RotationX = -rotationX,
				RotationY = -rotationY,
				RotationZ = -rotation
			};
		}

		static void UpdateScaleAndRotation(VisualElement view, FrameworkElement frameworkElement)
		{
			double anchorX = view.AnchorX;
			double anchorY = view.AnchorY;
			double scale = view.Scale;
			frameworkElement.RenderTransformOrigin = new System.Windows.Point(anchorX, anchorY);
			frameworkElement.RenderTransform = new ScaleTransform { ScaleX = scale, ScaleY = scale };

			UpdateRotation(view, frameworkElement);
		}

		void UpdateTouchFrameReportedEvent(VisualElement model)
		{
			if (_touchFrameReportedEventSet)
				return;

			Touch.FrameReported -= Touch_FrameReported;
			_touchFrameReportedEventSet = false;

			var view = model as View;
			if (view == null)
				return;

			if (!view.GestureRecognizers.GetGesturesFor<PanGestureRecognizer>().Any(g => g.TouchPoints > 1))
				return;

			Touch.FrameReported += Touch_FrameReported;
			_touchFrameReportedEventSet = true;
		}

		static void UpdateVisibility(VisualElement view, FrameworkElement frameworkElement)
		{
			frameworkElement.Visibility = view.IsVisible ? Visibility.Visible : Visibility.Collapsed;
		}
	}
}