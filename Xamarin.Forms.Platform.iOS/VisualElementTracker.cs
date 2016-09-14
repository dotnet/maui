using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using CoreAnimation;

namespace Xamarin.Forms.Platform.iOS
{
	public class VisualElementTracker : IDisposable
	{
		readonly EventHandler<EventArg<VisualElement>> _batchCommittedHandler;

		readonly PropertyChangedEventHandler _propertyChangedHandler;
		readonly EventHandler _sizeChangedEventHandler;
		bool _disposed;
		VisualElement _element;

		// Track these by hand because the calls down into iOS are too expensive
		bool _isInteractive;
		Rectangle _lastBounds;

		CALayer _layer;
		int _updateCount;

		public VisualElementTracker(IVisualElementRenderer renderer)
		{
			if (renderer == null)
				throw new ArgumentNullException("renderer");

			_propertyChangedHandler = HandlePropertyChanged;
			_sizeChangedEventHandler = HandleSizeChanged;
			_batchCommittedHandler = HandleRedrawNeeded;

			Renderer = renderer;
			renderer.ElementChanged += OnRendererElementChanged;
			SetElement(null, renderer.Element);
		}

		IVisualElementRenderer Renderer { get; set; }

		public void Dispose()
		{
			Dispose(true);
		}

		public event EventHandler NativeControlUpdated;

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				SetElement(_element, null);

				if (_layer != null)
				{
					_layer.Dispose();
					_layer = null;
				}

				Renderer.ElementChanged -= OnRendererElementChanged;
				Renderer = null;
			}
		}

		void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == VisualElement.XProperty.PropertyName || e.PropertyName == VisualElement.YProperty.PropertyName || e.PropertyName == VisualElement.WidthProperty.PropertyName ||
				e.PropertyName == VisualElement.HeightProperty.PropertyName || e.PropertyName == VisualElement.AnchorXProperty.PropertyName || e.PropertyName == VisualElement.AnchorYProperty.PropertyName ||
				e.PropertyName == VisualElement.TranslationXProperty.PropertyName || e.PropertyName == VisualElement.TranslationYProperty.PropertyName || e.PropertyName == VisualElement.ScaleProperty.PropertyName ||
				e.PropertyName == VisualElement.RotationProperty.PropertyName || e.PropertyName == VisualElement.RotationXProperty.PropertyName || e.PropertyName == VisualElement.RotationYProperty.PropertyName ||
				e.PropertyName == VisualElement.IsVisibleProperty.PropertyName || e.PropertyName == VisualElement.IsEnabledProperty.PropertyName ||
				e.PropertyName == VisualElement.InputTransparentProperty.PropertyName || e.PropertyName == VisualElement.OpacityProperty.PropertyName)
				UpdateNativeControl(); // poorly optimized
		}

		void HandleRedrawNeeded(object sender, EventArgs e)
		{
			UpdateNativeControl();
		}

		void HandleSizeChanged(object sender, EventArgs e)
		{
			UpdateNativeControl();
		}

		void OnRendererElementChanged(object s, VisualElementChangedEventArgs e)
		{
			if (_element == e.NewElement)
				return;

			SetElement(_element, e.NewElement);
		}

		void OnUpdateNativeControl(CALayer caLayer)
		{
			var view = Renderer.Element;
			var uiview = Renderer.NativeView;

			if (view == null || view.Batched)
				return;

			var shouldInteract = !view.InputTransparent && view.IsEnabled;
			if (_isInteractive != shouldInteract)
			{
				uiview.UserInteractionEnabled = shouldInteract;
				_isInteractive = shouldInteract;
			}

			var boundsChanged = _lastBounds != view.Bounds;

			var thread = !boundsChanged && !caLayer.Frame.IsEmpty;

			var anchorX = (float)view.AnchorX;
			var anchorY = (float)view.AnchorY;
			var translationX = (float)view.TranslationX;
			var translationY = (float)view.TranslationY;
			var rotationX = (float)view.RotationX;
			var rotationY = (float)view.RotationY;
			var rotation = (float)view.Rotation;
			var scale = (float)view.Scale;
			var width = (float)view.Width;
			var height = (float)view.Height;
			var x = (float)view.X;
			var y = (float)view.Y;
			var opacity = (float)view.Opacity;
			var isVisible = view.IsVisible;

			var updateTarget = Interlocked.Increment(ref _updateCount);

			Action update = () =>
			{
				if (updateTarget != _updateCount)
					return;

				var visualElement = view;
				var parent = view.RealParent;

				var shouldRelayoutSublayers = false;
				if (isVisible && caLayer.Hidden)
				{
					caLayer.Hidden = false;
					if (!caLayer.Frame.IsEmpty)
						shouldRelayoutSublayers = true;
				}

				if (!isVisible && !caLayer.Hidden)
				{
					caLayer.Hidden = true;
					shouldRelayoutSublayers = true;
				}

				// ripe for optimization
				var transform = CATransform3D.Identity;

				// Dont ever attempt to actually change the layout of a Page unless it is a ContentPage
				// iOS is a really big fan of you not actually modifying the View's of the UIViewControllers
				if ((!(visualElement is Page) || visualElement is ContentPage) && width > 0 && height > 0 && parent != null && boundsChanged)
				{
					var target = new RectangleF(x, y, width, height);
					// must reset transform prior to setting frame...
					caLayer.Transform = transform;
					uiview.Frame = target;
					if (shouldRelayoutSublayers)
						caLayer.LayoutSublayers();
				}
				else if (width <= 0 || height <= 0)
				{
					caLayer.Hidden = true;
					return;
				}

				caLayer.AnchorPoint = new PointF(anchorX, anchorY);
				caLayer.Opacity = opacity;
				const double epsilon = 0.001;

				// position is relative to anchor point
				if (Math.Abs(anchorX - .5) > epsilon)
					transform = transform.Translate((anchorX - .5f) * width, 0, 0);
				if (Math.Abs(anchorY - .5) > epsilon)
					transform = transform.Translate(0, (anchorY - .5f) * height, 0);

				if (Math.Abs(translationX) > epsilon || Math.Abs(translationY) > epsilon)
					transform = transform.Translate(translationX, translationY, 0);

				if (Math.Abs(scale - 1) > epsilon)
					transform = transform.Scale(scale);

				// not just an optimization, iOS will not "pixel align" a view which has m34 set
				if (Math.Abs(rotationY % 180) > epsilon || Math.Abs(rotationX % 180) > epsilon)
					transform.m34 = 1.0f / -400f;

				if (Math.Abs(rotationX % 360) > epsilon)
					transform = transform.Rotate(rotationX * (float)Math.PI / 180.0f, 1.0f, 0.0f, 0.0f);
				if (Math.Abs(rotationY % 360) > epsilon)
					transform = transform.Rotate(rotationY * (float)Math.PI / 180.0f, 0.0f, 1.0f, 0.0f);

				transform = transform.Rotate(rotation * (float)Math.PI / 180.0f, 0.0f, 0.0f, 1.0f);
				caLayer.Transform = transform;
			};

			if (thread)
				CADisplayLinkTicker.Default.Invoke(update);
			else
				update();

			_lastBounds = view.Bounds;
		}

		void SetElement(VisualElement oldElement, VisualElement newElement)
		{
			if (oldElement != null)
			{
				oldElement.PropertyChanged -= _propertyChangedHandler;
				oldElement.SizeChanged -= _sizeChangedEventHandler;
				oldElement.BatchCommitted -= _batchCommittedHandler;
			}

			_element = newElement;

			if (newElement != null)
			{
				newElement.BatchCommitted += _batchCommittedHandler;
				newElement.SizeChanged += _sizeChangedEventHandler;
				newElement.PropertyChanged += _propertyChangedHandler;

				UpdateNativeControl();
			}
		}

		void UpdateNativeControl()
		{
			if (_disposed)
				return;

			if (_layer == null)
			{
				_layer = Renderer.NativeView.Layer;
				_isInteractive = Renderer.NativeView.UserInteractionEnabled;
			}

			OnUpdateNativeControl(_layer);

			if (NativeControlUpdated != null)
				NativeControlUpdated(this, EventArgs.Empty);
		}
	}
}