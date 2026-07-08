using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using WCompositeTransform = Microsoft.UI.Xaml.Media.CompositeTransform;
using WScaleTransform = Microsoft.UI.Xaml.Media.ScaleTransform;
using WVisibility = Microsoft.UI.Xaml.Visibility;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class VisualElementTracker<TElement, TNativeElement> : IDisposable where TElement : VisualElement where TNativeElement : FrameworkElement
	{
		FrameworkElement _container;
		TNativeElement _control;
		TElement _element;
		bool _invalidateArrangeNeeded;
		bool _isDisposed;
		bool HasClip;

		public VisualElementTracker()
		{
		}

		public FrameworkElement Container
		{
			get { return _container; }
			set
			{
				if (_container == value)
					return;


				_container = value;

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

				_control = value;
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
					_element.BatchCommitted -= OnRedrawNeeded;
					_element.PropertyChanged -= OnPropertyChanged;
				}

				_element = value;

				if (_element != null)
				{
					_element.BatchCommitted += OnRedrawNeeded;
					_element.PropertyChanged += OnPropertyChanged;
				}

				UpdateNativeControl();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public event EventHandler Updated;

		protected virtual void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			_isDisposed = true;

			if (!disposing)
				return;

			if (_element != null)
			{
				_element.BatchCommitted -= OnRedrawNeeded;
				_element.PropertyChanged -= OnPropertyChanged;
			}

			Control = null;
			Element = null;
			Container = null;
		}

		protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (Element.Batched)
			{
				if (e.PropertyName == VisualElement.XProperty.PropertyName || e.PropertyName == VisualElement.YProperty.PropertyName || e.PropertyName == VisualElement.WidthProperty.PropertyName ||
					e.PropertyName == VisualElement.HeightProperty.PropertyName)
				{
					_invalidateArrangeNeeded = true;
				}
				return;
			}

			if (e.PropertyName == VisualElement.XProperty.PropertyName || e.PropertyName == VisualElement.YProperty.PropertyName || e.PropertyName == VisualElement.WidthProperty.PropertyName ||
				e.PropertyName == VisualElement.HeightProperty.PropertyName)
			{
				MaybeInvalidate();
			}
			else if (e.PropertyName == VisualElement.AnchorXProperty.PropertyName || e.PropertyName == VisualElement.AnchorYProperty.PropertyName)
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

		void MaybeInvalidate()
		{
			if (Element.IsInPlatformLayout)
				return;

			var parent = (FrameworkElement)Container.Parent;
			parent?.InvalidateMeasure();
			Container.InvalidateMeasure();
		}

		void OnRedrawNeeded(object sender, EventArgs e)
		{
			UpdateNativeControl();
		}

		void OnUpdated()
		{
			if (Updated != null)
				Updated(this, EventArgs.Empty);
		}

		static void UpdateInputTransparent(VisualElement view, FrameworkElement frameworkElement)
		{
			if (view is Layout)
			{
				// Let VisualElementRenderer handle this
			}

			frameworkElement.IsHitTestVisible = view.IsEnabled && !view.InputTransparent;
		}

		void UpdateClip(VisualElement view, FrameworkElement frameworkElement)
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

		bool ShouldUpdateClip(VisualElement view, FrameworkElement frameworkElement)
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

		[PortHandler]
		static void UpdateOpacity(VisualElement view, FrameworkElement frameworkElement)
		{
			frameworkElement.Opacity = view.Opacity;
		}

		[PortHandler]
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

		[PortHandler]
		static void UpdateScaleAndRotation(VisualElement view, FrameworkElement frameworkElement)
		{
			double anchorX = view.AnchorX;
			double anchorY = view.AnchorY;
			frameworkElement.RenderTransformOrigin = new global::Windows.Foundation.Point(anchorX, anchorY);
			frameworkElement.RenderTransform = new WScaleTransform { ScaleX = view.Scale * view.ScaleX, ScaleY = view.Scale * view.ScaleY };

			UpdateRotation(view, frameworkElement);
		}

		[PortHandler]
		static void UpdateVisibility(VisualElement view, FrameworkElement frameworkElement)
		{
			frameworkElement.Visibility = view.IsVisible ? WVisibility.Visible : WVisibility.Collapsed;
		}
	}
}
