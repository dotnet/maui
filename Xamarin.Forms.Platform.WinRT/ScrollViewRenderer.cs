using System;
using System.ComponentModel;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class ScrollViewRenderer : ViewRenderer<ScrollView, ScrollViewer>
	{
		VisualElement _currentView;

		public ScrollViewRenderer()
		{
			AutoPackage = false;
		}

		protected IScrollViewController Controller
		{
			get { return Element; }
		}

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			SizeRequest result = base.GetDesiredSize(widthConstraint, heightConstraint);
			result.Minimum = new Size(40, 40);
			return result;
		}

		protected override Windows.Foundation.Size ArrangeOverride(Windows.Foundation.Size finalSize)
		{
			if (Element == null)
				return finalSize;

			Element.IsInNativeLayout = true;

			Control?.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));

			Element.IsInNativeLayout = false;

			return finalSize;
		}

		protected override void Dispose(bool disposing)
		{
			if (Control != null)
			{
				Control.ViewChanged -= OnViewChanged;
			}

			base.Dispose(disposing);
		}

		protected override Windows.Foundation.Size MeasureOverride(Windows.Foundation.Size availableSize)
		{
			if (Element == null)
				return new Windows.Foundation.Size(0, 0);

			double width = Math.Max(0, Element.Width);
			double height = Math.Max(0, Element.Height);
			var result = new Windows.Foundation.Size(width, height);

			Control?.Measure(result);

			return result;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ScrollView> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
			{
				((IScrollViewController)e.OldElement).ScrollToRequested -= OnScrollToRequested;
			}

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					SetNativeControl(new ScrollViewer { HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Auto });

					Control.ViewChanged += OnViewChanged;
				}

				Controller.ScrollToRequested += OnScrollToRequested;

				UpdateOrientation();

				LoadContent();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == "Content")
				LoadContent();
			else if (e.PropertyName == Layout.PaddingProperty.PropertyName)
				UpdateMargins();
			else if (e.PropertyName == ScrollView.OrientationProperty.PropertyName)
				UpdateOrientation();
		}

		void LoadContent()
		{
			if (_currentView != null)
			{
				_currentView.Cleanup();
			}

			_currentView = Element.Content;

			IVisualElementRenderer renderer = null;
			if (_currentView != null)
			{
				renderer = _currentView.GetOrCreateRenderer();
			}

			Control.Content = renderer != null ? renderer.ContainerElement : null;

			UpdateMargins();
		}

		void OnScrollToRequested(object sender, ScrollToRequestedEventArgs e)
		{
			double x = e.ScrollX, y = e.ScrollY;

			ScrollToMode mode = e.Mode;
			if (mode == ScrollToMode.Element)
			{
				Point pos = Controller.GetScrollPositionForElement((VisualElement)e.Element, e.Position);
				x = pos.X;
				y = pos.Y;
				mode = ScrollToMode.Position;
			}

			if (mode == ScrollToMode.Position)
			{
				Control.ChangeView(x, y, null, !e.ShouldAnimate);
			}
			Controller.SendScrollFinished();
		}

		void OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
		{
			Controller.SetScrolledPosition(Control.HorizontalOffset, Control.VerticalOffset);

			if (!e.IsIntermediate)
				Controller.SendScrollFinished();
		}

		void UpdateMargins()
		{
			var element = Control.Content as FrameworkElement;
			if (element == null)
				return;

			switch (Element.Orientation)
			{
				case ScrollOrientation.Horizontal:
					// need to add left/right margins
					element.Margin = new Windows.UI.Xaml.Thickness(Element.Padding.Left, 0, Element.Padding.Right, 0);
					break;
				case ScrollOrientation.Vertical:
					// need to add top/bottom margins
					element.Margin = new Windows.UI.Xaml.Thickness(0, Element.Padding.Top, 0, Element.Padding.Bottom);
					break;
				case ScrollOrientation.Both:
					// need to add all margins
					element.Margin = new Windows.UI.Xaml.Thickness(Element.Padding.Left, Element.Padding.Top, Element.Padding.Right, Element.Padding.Bottom);
					break;
			}
		}

		void UpdateOrientation()
		{
			if (Element.Orientation == ScrollOrientation.Horizontal || Element.Orientation == ScrollOrientation.Both)
			{
				Control.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
			}
			else
			{
				Control.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
			}
		}
	}
}