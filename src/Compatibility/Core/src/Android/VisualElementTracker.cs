using System;
using System.Collections.Generic;
using System.ComponentModel;
using Android.Content;
using Android.Views;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Fragment.App;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using AView = Android.Views.View;
using Object = Java.Lang.Object;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public class VisualElementTracker : IDisposable
	{
		readonly EventHandler<EventArg<VisualElement>> _batchCommittedHandler;
		readonly IList<string> _batchedProperties = new List<string>();
		readonly PropertyChangedEventHandler _propertyChangedHandler;
		Context _context;

		bool _disposed;

		VisualElement _element;
		bool _initialUpdateNeeded = true;
		bool _layoutNeeded;
		IVisualElementRenderer _renderer;
		AttachTracker _attachTracker;
		public VisualElementTracker(IVisualElementRenderer renderer)
		{
			_batchCommittedHandler = HandleRedrawNeeded;
			_propertyChangedHandler = HandlePropertyChanged;

			_renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
			_context = renderer.View.Context;
			_renderer.ElementChanged += RendererOnElementChanged;

			VisualElement view = renderer.Element;
			SetElement(null, view);

			renderer.View.SetCameraDistance(3600);
			_attachTracker = AttachTracker.Instance;
			renderer.View.AddOnAttachStateChangeListener(_attachTracker);

		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				SetElement(_element, null);

				if (_renderer != null)
				{
					_renderer.ElementChanged -= RendererOnElementChanged;

					if (_renderer.View.IsAlive() && _attachTracker.IsAlive())
					{
						_renderer.View.RemoveOnAttachStateChangeListener(_attachTracker);
						_attachTracker = null;
					}

					_renderer = null;
					_context = null;
				}
			}
		}

		public void UpdateLayout()
		{
			VisualElement view = _renderer.Element;
			AView aview = _renderer.View;

			var headlessOffset = CompressedLayout.GetHeadlessOffset(view);
			var x = (int)_context.ToPixels(view.X + headlessOffset.X);
			var y = (int)_context.ToPixels(view.Y + headlessOffset.Y);
			var width = Math.Max(0, (int)_context.ToPixels(view.Width));
			var height = Math.Max(0, (int)_context.ToPixels(view.Height));


			if (aview is MauiViewGroup formsViewGroup)
			{
				formsViewGroup.MeasureAndLayout(MeasureSpecFactory.MakeMeasureSpec(width, MeasureSpecMode.Exactly), MeasureSpecFactory.MakeMeasureSpec(height, MeasureSpecMode.Exactly), x, y, x + width, y + height);
			}
			else if ((aview is LayoutViewGroup || aview is ContentViewGroup || aview is CoordinatorLayout || aview is FragmentContainerView) && width == 0 && height == 0)
			{
				// Nothing to do here; just chill.
			}
			else
			{
				aview.Measure(MeasureSpecFactory.MakeMeasureSpec(width, MeasureSpecMode.Exactly), MeasureSpecFactory.MakeMeasureSpec(height, MeasureSpecMode.Exactly));

				aview.Layout(x, y, x + width, y + height);
			}

			// We have to make sure to update the ClipBounds to match the new size of the ViewGroup
			UpdateClipToBounds();
			UpdateClip();

			//On Width or Height changes, the anchors needs to be updated
			UpdateAnchorX();
			UpdateAnchorY();
		}

		void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (_renderer == null || !_renderer.View.IsAlive())
			{
				return;
			}

			if (e.PropertyName == Layout.IsClippedToBoundsProperty.PropertyName)
			{
				UpdateClipToBounds();
				return;
			}

			if (_renderer.Element.Batched)
			{
				if (e.PropertyName == VisualElement.XProperty.PropertyName || e.PropertyName == VisualElement.YProperty.PropertyName || e.PropertyName == VisualElement.WidthProperty.PropertyName ||
					e.PropertyName == VisualElement.HeightProperty.PropertyName)
					_layoutNeeded = true;
				else if (e.PropertyName == VisualElement.AnchorXProperty.PropertyName || e.PropertyName == VisualElement.AnchorYProperty.PropertyName || e.PropertyName == VisualElement.ScaleProperty.PropertyName || e.PropertyName == VisualElement.ScaleXProperty.PropertyName || e.PropertyName == VisualElement.ScaleYProperty.PropertyName ||
						 e.PropertyName == VisualElement.RotationProperty.PropertyName || e.PropertyName == VisualElement.RotationXProperty.PropertyName || e.PropertyName == VisualElement.RotationYProperty.PropertyName ||
						 e.PropertyName == VisualElement.IsVisibleProperty.PropertyName || e.PropertyName == VisualElement.OpacityProperty.PropertyName ||
						 e.PropertyName == VisualElement.TranslationXProperty.PropertyName || e.PropertyName == VisualElement.TranslationYProperty.PropertyName)
				{
					if (!_batchedProperties.Contains(e.PropertyName))
						_batchedProperties.Add(e.PropertyName);
				}
				return;
			}

			if (e.PropertyName == VisualElement.XProperty.PropertyName || e.PropertyName == VisualElement.YProperty.PropertyName || e.PropertyName == VisualElement.WidthProperty.PropertyName ||
				e.PropertyName == VisualElement.HeightProperty.PropertyName)
				_renderer.View.MaybeRequestLayout();
			else if (e.PropertyName == VisualElement.AnchorXProperty.PropertyName)
				UpdateAnchorX();
			else if (e.PropertyName == VisualElement.AnchorYProperty.PropertyName)
				UpdateAnchorY();
			else if (e.PropertyName == VisualElement.ScaleProperty.PropertyName
					 || e.PropertyName == VisualElement.ScaleXProperty.PropertyName
					 || e.PropertyName == VisualElement.ScaleYProperty.PropertyName)
				UpdateScale();
			else if (e.PropertyName == VisualElement.RotationProperty.PropertyName)
				UpdateRotation();
			else if (e.PropertyName == VisualElement.RotationXProperty.PropertyName)
				UpdateRotationX();
			else if (e.PropertyName == VisualElement.RotationYProperty.PropertyName)
				UpdateRotationY();
			else if (e.PropertyName == VisualElement.IsVisibleProperty.PropertyName)
				UpdateIsVisible();
			else if (e.PropertyName == VisualElement.OpacityProperty.PropertyName)
				UpdateOpacity();
			else if (e.PropertyName == VisualElement.TranslationXProperty.PropertyName)
				UpdateTranslationX();
			else if (e.PropertyName == VisualElement.TranslationYProperty.PropertyName)
				UpdateTranslationY();
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateIsEnabled();
			else if (e.PropertyName == VisualElement.ClipProperty.PropertyName)
				UpdateClip();
		}

		void HandleRedrawNeeded(object sender, EventArg<VisualElement> e)
		{
			foreach (string propertyName in _batchedProperties)
				HandlePropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			_batchedProperties.Clear();

			if (_layoutNeeded)
				_renderer.View.MaybeRequestLayout();
			_layoutNeeded = false;
		}

		void HandleViewAttachedToWindow()
		{
			if (_initialUpdateNeeded)
			{
				UpdateNativeView(this, EventArgs.Empty);
				_initialUpdateNeeded = false;
			}

			UpdateClipToBounds();
			UpdateClip();
		}

		void RendererOnElementChanged(object sender, VisualElementChangedEventArgs args)
		{
			SetElement(args.OldElement, args.NewElement);
		}

		void SetElement(VisualElement oldElement, VisualElement newElement)
		{
			if (oldElement != null)
			{
				oldElement.BatchCommitted -= _batchCommittedHandler;
				oldElement.PropertyChanged -= _propertyChangedHandler;
				_context = null;
			}

			_element = newElement;
			if (newElement != null)
			{
				newElement.BatchCommitted += _batchCommittedHandler;
				newElement.PropertyChanged += _propertyChangedHandler;
				_context = _renderer.View.Context;

				if (oldElement != null)
				{
					AView view = _renderer.View;

					// ReSharper disable CompareOfFloatsByEqualityOperator
					if (oldElement.AnchorX != newElement.AnchorX)
						UpdateAnchorX();
					if (oldElement.AnchorY != newElement.AnchorY)
						UpdateAnchorY();
					if (oldElement.IsVisible != newElement.IsVisible)
						UpdateIsVisible();
					if (oldElement.IsEnabled != newElement.IsEnabled)
						view.Enabled = newElement.IsEnabled;
					if (oldElement.Opacity != newElement.Opacity)
						UpdateOpacity();
					if (oldElement.Rotation != newElement.Rotation)
						UpdateRotation();
					if (oldElement.RotationX != newElement.RotationX)
						UpdateRotationX();
					if (oldElement.RotationY != newElement.RotationY)
						UpdateRotationY();
					if (oldElement.Scale != newElement.Scale || oldElement.ScaleX != newElement.ScaleX || oldElement.ScaleY != newElement.ScaleY)
						UpdateScale();
					// ReSharper restore CompareOfFloatsByEqualityOperator

					_initialUpdateNeeded = false;
				}
			}
		}

		[PortHandler]
		void UpdateAnchorX()
		{
			VisualElement view = _renderer.Element;
			AView aview = _renderer.View;

			float currentPivot = aview.PivotX;
			var target = (float)(view.AnchorX * _context.ToPixels(view.Width));
			if (currentPivot != target)
				aview.PivotX = target;
		}

		[PortHandler]
		void UpdateAnchorY()
		{
			VisualElement view = _renderer.Element;
			AView aview = _renderer.View;

			float currentPivot = aview.PivotY;
			var target = (float)(view.AnchorY * _context.ToPixels(view.Height));
			if (currentPivot != target)
				aview.PivotY = target;
		}

		void UpdateClipToBounds()
		{
			if (!(_renderer.Element is Layout layout))
			{
				return;
			}

			bool shouldClip = layout.IsClippedToBounds;

			if (!(_renderer.View is ViewGroup viewGroup))
			{
				return;
			}

			// Forms layouts should not impose clipping on their children	
			viewGroup.SetClipChildren(false);

			// But if IsClippedToBounds is true, they _should_ enforce clipping at their own edges	
			viewGroup.ClipBounds = shouldClip ? new global::Android.Graphics.Rect(0, 0, viewGroup.Width, viewGroup.Height) : null;
		}

		void UpdateClip()
		{
			var aView = _renderer.View;
			aView?.Invalidate();
		}

		[PortHandler]
		void UpdateIsVisible()
		{
			VisualElement view = _renderer.Element;
			AView aview = _renderer.View;

			if (view.IsVisible && aview.Visibility != ViewStates.Visible)
				aview.Visibility = ViewStates.Visible;
			if (!view.IsVisible && aview.Visibility != ViewStates.Gone)
				aview.Visibility = ViewStates.Gone;

			aview.Invalidate();
			aview.RequestLayout();
		}

		void UpdateNativeView(object sender, EventArgs e)
		{
			VisualElement view = _renderer.Element;
			AView aview = _renderer.View;

			if (aview is MauiViewGroup formsViewGroup)
			{
				formsViewGroup.SendBatchUpdate((float)(view.AnchorX * _context.ToPixels(view.Width)),
											   (float)(view.AnchorY * _context.ToPixels(view.Height)),
											   (int)(view.IsVisible ? ViewStates.Visible : ViewStates.Invisible),
											   view.IsEnabled,
											   (float)view.Opacity,
											   (float)view.Rotation,
											   (float)view.RotationX,
											   (float)view.RotationY,
											   (float)view.Scale * (float)view.ScaleX,
											   (float)view.Scale * (float)view.ScaleY,
											   _context.ToPixels(view.TranslationX),
											   _context.ToPixels(view.TranslationY));
			}
			else
			{
				MauiViewGroup.SendViewBatchUpdate(aview,
												   (float)(view.AnchorX * _context.ToPixels(view.Width)),
												   (float)(view.AnchorY * _context.ToPixels(view.Height)),
												   (int)(view.IsVisible ? ViewStates.Visible : ViewStates.Invisible),
												   view.IsEnabled,
												   (float)view.Opacity,
												   (float)view.Rotation,
												   (float)view.RotationX,
												   (float)view.RotationY,
												   (float)view.Scale * (float)view.ScaleX,
												   (float)view.Scale * (float)view.ScaleY,
												   _context.ToPixels(view.TranslationX),
												   _context.ToPixels(view.TranslationY));
			}
		}

		[PortHandler]
		void UpdateOpacity()
		{
			VisualElement view = _renderer.Element;
			AView aview = _renderer.View;

			aview.Alpha = (float)view.Opacity;
		}

		[PortHandler]
		void UpdateRotation()
		{
			VisualElement view = _renderer.Element;
			AView aview = _renderer.View;

			aview.Rotation = (float)view.Rotation;
		}

		[PortHandler]
		void UpdateRotationX()
		{
			VisualElement view = _renderer.Element;
			AView aview = _renderer.View;

			aview.RotationX = (float)view.RotationX;
		}

		[PortHandler]
		void UpdateRotationY()
		{
			VisualElement view = _renderer.Element;
			AView aview = _renderer.View;

			aview.RotationY = (float)view.RotationY;
		}

		[PortHandler]
		void UpdateScale()
		{
			VisualElement view = _renderer.Element;
			AView aview = _renderer.View;

			var scale = view.Scale;

			if (double.IsNaN(scale))
				return;

			aview.ScaleX = (float)scale * (float)view.ScaleX;
			aview.ScaleY = (float)scale * (float)view.ScaleY;
		}

		[PortHandler]
		void UpdateTranslationX()
		{
			VisualElement view = _renderer.Element;
			AView aview = _renderer.View;

			aview.TranslationX = _context.ToPixels(view.TranslationX);
		}

		[PortHandler]
		void UpdateTranslationY()
		{
			VisualElement view = _renderer.Element;
			AView aview = _renderer.View;

			aview.TranslationY = _context.ToPixels(view.TranslationY);
		}

		void UpdateIsEnabled()
		{
			_renderer.View.Enabled = _renderer.Element.IsEnabled;
		}

		class AttachTracker : Object, AView.IOnAttachStateChangeListener
		{
			public static readonly AttachTracker Instance = new AttachTracker();

			public void OnViewAttachedToWindow(AView attachedView)
			{
				var renderer = attachedView as IVisualElementRenderer;
				if (renderer == null || renderer.Tracker == null)
					return;

				renderer.Tracker.HandleViewAttachedToWindow();
			}

			public void OnViewDetachedFromWindow(AView detachedView)
			{
			}
		}
	}
}
