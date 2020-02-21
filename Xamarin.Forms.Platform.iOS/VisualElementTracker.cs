using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using CoreAnimation;
using CoreGraphics;
using Xamarin.Forms.Internals;
#if __MOBILE__
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Xamarin.Forms.Platform.iOS
#else

namespace Xamarin.Forms.Platform.MacOS
#endif
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
#if !__MOBILE__
		Rectangle _lastParentBounds;
#endif
		CALayer _layer;
		CGPoint _originalAnchor;
		int _updateCount;

		public VisualElementTracker(IVisualElementRenderer renderer) : this(renderer, true)
		{
		}

		public VisualElementTracker(IVisualElementRenderer renderer, bool trackFrame)
		{
			Renderer = renderer ?? throw new ArgumentNullException("renderer");

			_propertyChangedHandler = HandlePropertyChanged;
			_sizeChangedEventHandler = HandleSizeChanged;
			_batchCommittedHandler = HandleRedrawNeeded;

			TrackFrame = trackFrame;
			renderer.ElementChanged += OnRendererElementChanged;
			SetElement(null, renderer.Element);
		}

		bool TrackFrame { get; set; }

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
			if (TrackFrame && (e.PropertyName == VisualElement.XProperty.PropertyName ||
							   e.PropertyName == VisualElement.YProperty.PropertyName ||
							   e.PropertyName == VisualElement.WidthProperty.PropertyName ||
							   e.PropertyName == VisualElement.HeightProperty.PropertyName))
			{
				UpdateNativeControl();
			}
			else if (e.PropertyName == VisualElement.AnchorXProperty.PropertyName ||
				e.PropertyName == VisualElement.AnchorYProperty.PropertyName ||
				e.PropertyName == VisualElement.TranslationXProperty.PropertyName ||
				e.PropertyName == VisualElement.TranslationYProperty.PropertyName ||
				e.PropertyName == VisualElement.ScaleProperty.PropertyName ||
				e.PropertyName == VisualElement.ScaleXProperty.PropertyName ||
				e.PropertyName == VisualElement.ScaleYProperty.PropertyName ||
				e.PropertyName == VisualElement.RotationProperty.PropertyName ||
				e.PropertyName == VisualElement.RotationXProperty.PropertyName ||
				e.PropertyName == VisualElement.RotationYProperty.PropertyName ||
				e.PropertyName == VisualElement.IsVisibleProperty.PropertyName ||
				e.PropertyName == VisualElement.IsEnabledProperty.PropertyName ||
				e.PropertyName == VisualElement.InputTransparentProperty.PropertyName ||
				e.PropertyName == VisualElement.OpacityProperty.PropertyName ||
				e.PropertyName == Layout.CascadeInputTransparentProperty.PropertyName)
			{
				UpdateNativeControl(); // poorly optimized
			}
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

			bool shouldInteract;

			if (view is Layout layout)
			{
				if (layout.InputTransparent)
				{
					shouldInteract = !layout.CascadeInputTransparent;
				}
				else
				{
					shouldInteract = layout.IsEnabled;
				}
			}
			else
			{
				shouldInteract = !view.InputTransparent && view.IsEnabled;
			}

			if (_isInteractive != shouldInteract)
			{
#if __MOBILE__
				uiview.UserInteractionEnabled = shouldInteract;
#endif
				_isInteractive = shouldInteract;
			}

			var boundsChanged = _lastBounds != view.Bounds && TrackFrame;
#if !__MOBILE__
			var viewParent = view.RealParent as VisualElement;
			var parentBoundsChanged = _lastParentBounds != (viewParent == null ? Rectangle.Zero : viewParent.Bounds);
#else
			var thread = !boundsChanged && !caLayer.Frame.IsEmpty && Application.Current?.OnThisPlatform()?.GetHandleControlUpdatesOnMainThread() == false;
#endif
			var anchorX = (float)view.AnchorX;
			var anchorY = (float)view.AnchorY;
			var translationX = (float)view.TranslationX;
			var translationY = (float)view.TranslationY;
			var rotationX = (float)view.RotationX;
			var rotationY = (float)view.RotationY;
			var rotation = (float)view.Rotation;
			var scale = (float)view.Scale;
			var scaleX = (float)view.ScaleX * scale;
			var scaleY = (float)view.ScaleY * scale;
			var width = (float)view.Width;
			var height = (float)view.Height;
			var x = (float)view.X + (float)CompressedLayout.GetHeadlessOffset(view).X;
			var y = (float)view.Y + (float)CompressedLayout.GetHeadlessOffset(view).Y;
			var opacity = (float)view.Opacity;
			var isVisible = view.IsVisible;

			var updateTarget = Interlocked.Increment(ref _updateCount);

			void update()
			{
				if (updateTarget != _updateCount)
					return;
#if __MOBILE__
				var visualElement = view;
#endif
				var parent = view.RealParent;

				var shouldRelayoutSublayers = false;
				if (isVisible && caLayer.Hidden)
				{
#if !__MOBILE__
					uiview.Hidden = false;
#endif
					caLayer.Hidden = false;
					if (!caLayer.Frame.IsEmpty)
						shouldRelayoutSublayers = true;
				}

				if (!isVisible && !caLayer.Hidden)
				{
#if !__MOBILE__
					uiview.Hidden = true;
#endif
					caLayer.Hidden = true;
					shouldRelayoutSublayers = true;
				}

				// ripe for optimization
				var transform = CATransform3D.Identity;

#if __MOBILE__
				bool shouldUpdate = (!(visualElement is Page) || visualElement is ContentPage) && width > 0 && height > 0 && parent != null && boundsChanged;
#else
				// We don't care if it's a page or not since bounds of the window can change
				// TODO: Find why it doesn't work to check if the parentsBounds changed  and remove true;
				parentBoundsChanged = true;
				bool shouldUpdate = width > 0 && height > 0 && parent != null && (boundsChanged || parentBoundsChanged);
#endif
				// Dont ever attempt to actually change the layout of a Page unless it is a ContentPage
				// iOS is a really big fan of you not actually modifying the View's of the UIViewControllers
				if (shouldUpdate && TrackFrame)
				{
#if __MOBILE__
					var target = new RectangleF(x, y, width, height);
#else
					var visualParent = parent as VisualElement;
					float newY = visualParent == null ? y : Math.Max(0, (float)(visualParent.Height - y - view.Height));
					var target = new RectangleF(x, newY, width, height);
#endif

					// must reset transform prior to setting frame...
					if(caLayer.AnchorPoint != _originalAnchor)
						caLayer.AnchorPoint = _originalAnchor;

					caLayer.Transform = transform;
					uiview.Frame = target;
					if (shouldRelayoutSublayers)
						caLayer.LayoutSublayers();
				}
				else if (width <= 0 || height <= 0)
				{
					//TODO: FInd why it doesn't work
#if __MOBILE__
					caLayer.Hidden = true;
#endif
					return;
				}
#if !__MOBILE__
				// Y-axe on macos is inverted				
				translationY = -translationY;
				anchorY = 1 - anchorY;

				// rotation direction on macos also inverted
				rotationX = -rotationX;
				rotationY = -rotationY;
				rotation = -rotation;

				//otherwise scaled/rotated image clipped by parent bounds
				caLayer.MasksToBounds = false;
#endif
				caLayer.AnchorPoint = new PointF(anchorX, anchorY);
				caLayer.Opacity = opacity;
				const double epsilon = 0.001;

#if !__MOBILE__
				// fix position, position in macos is also relative to anchor point
				// but it's (0,0) by default, so we don't need to substract 0.5
				transform = transform.Translate(anchorX * width, 0, 0);
				transform = transform.Translate(0, anchorY * height, 0);
#else
				// position is relative to anchor point
				if (Math.Abs(anchorX - .5) > epsilon)
					transform = transform.Translate((anchorX - .5f) * width, 0, 0);
				if (Math.Abs(anchorY - .5) > epsilon)
					transform = transform.Translate(0, (anchorY - .5f) * height, 0);
#endif

				if (Math.Abs(translationX) > epsilon || Math.Abs(translationY) > epsilon)
					transform = transform.Translate(translationX, translationY, 0);

				if (Math.Abs(scaleX - 1) > epsilon || Math.Abs(scaleY - 1) > epsilon)
					transform = transform.Scale(scaleX, scaleY, scale);

				// not just an optimization, iOS will not "pixel align" a view which has m34 set
				if (Math.Abs(rotationY % 180) > epsilon || Math.Abs(rotationX % 180) > epsilon)
					transform.m34 = 1.0f / -400f;

				if (Math.Abs(rotationX % 360) > epsilon)
					transform = transform.Rotate(rotationX * (float)Math.PI / 180.0f, 1.0f, 0.0f, 0.0f);
				if (Math.Abs(rotationY % 360) > epsilon)
					transform = transform.Rotate(rotationY * (float)Math.PI / 180.0f, 0.0f, 1.0f, 0.0f);

				transform = transform.Rotate(rotation * (float)Math.PI / 180.0f, 0.0f, 0.0f, 1.0f);

				if (Foundation.NSThread.IsMain)
				{
					caLayer.Transform = transform;
					return;
				}
				CoreFoundation.DispatchQueue.MainQueue.DispatchAsync(() =>
				{
					caLayer.Transform = transform;
				});
			}

#if __MOBILE__
			if (thread)
				CADisplayLinkTicker.Default.Invoke(update);
			else
				update();
#else
			update();
#endif

			_lastBounds = view.Bounds;
#if !__MOBILE__
			_lastParentBounds = viewParent?.Bounds ?? Rectangle.Zero;
#endif
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
			Performance.Start(out string reference);

			if (_disposed)
				return;

			if (_layer == null)
			{
#if !__MOBILE__
				Renderer.NativeView.WantsLayer = true;
#endif
				_layer = Renderer.NativeView.Layer;
#if __MOBILE__
				_isInteractive = Renderer.NativeView.UserInteractionEnabled;
#endif

				_originalAnchor = _layer.AnchorPoint;
			}

			OnUpdateNativeControl(_layer);

			NativeControlUpdated?.Invoke(this, EventArgs.Empty);
			Performance.Stop(reference);
		}
	}
}
