using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using UwpScrollBarVisibility = Windows.UI.Xaml.Controls.ScrollBarVisibility;

namespace Xamarin.Forms.Platform.UWP
{
	public class ScrollViewRenderer : ViewRenderer<ScrollView, ScrollViewer>
	{
		VisualElement _currentView;
		bool _checkedForRtlScroll = false;

		public ScrollViewRenderer()
		{
			AutoPackage = false;
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
			CleanUp(Element, Control);
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

		void CleanUp(ScrollView scrollView, ScrollViewer scrollViewer)
		{
			if (scrollView != null)
				scrollView.ScrollToRequested -= OnScrollToRequested;

			if (scrollViewer != null)
			{
				scrollViewer.ViewChanged -= OnViewChanged;
				if (scrollViewer.Content is FrameworkElement element)
				{
					element.LayoutUpdated -= SetInitialRtlPosition;
				}
			}
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ScrollView> e)
		{
			base.OnElementChanged(e);
			CleanUp(e.OldElement, Control);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					SetNativeControl(new ScrollViewer
					{
						HorizontalScrollBarVisibility = ScrollBarVisibilityToUwp(e.NewElement.HorizontalScrollBarVisibility),
						VerticalScrollBarVisibility = ScrollBarVisibilityToUwp(e.NewElement.VerticalScrollBarVisibility)
					});

					Control.ViewChanged += OnViewChanged;
				}

				Element.ScrollToRequested += OnScrollToRequested;

				UpdateOrientation();

				UpdateContent();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == "Content")
				UpdateContent();
			else if (e.PropertyName == Layout.PaddingProperty.PropertyName)
				UpdateMargins();
			else if (e.PropertyName == ScrollView.OrientationProperty.PropertyName)
				UpdateOrientation();
			else if (e.PropertyName == ScrollView.VerticalScrollBarVisibilityProperty.PropertyName)
				UpdateVerticalScrollBarVisibiilty();
			else if (e.PropertyName == ScrollView.HorizontalScrollBarVisibilityProperty.PropertyName)
				UpdateHorizontalScrollBarVisibility();
		}

		void UpdateContent()
		{
			if (_currentView != null)
				_currentView.Cleanup();

			if (Control?.Content is FrameworkElement frameworkElement)
				frameworkElement.LayoutUpdated -= SetInitialRtlPosition;

			_currentView = Element.Content;

			IVisualElementRenderer renderer = null;
			if (_currentView != null)
				renderer = _currentView.GetOrCreateRenderer();

			Control.Content = renderer != null ? renderer.ContainerElement : null;

			UpdateMargins();
			if(renderer.ContainerElement != null)
				renderer.ContainerElement.LayoutUpdated += SetInitialRtlPosition;
		}

		async void OnScrollToRequested(object sender, ScrollToRequestedEventArgs e)
		{
			ClearRtlScrollCheck();

			// Adding items into the view while scrolling to the end can cause it to fail, as
			// the items have not actually been laid out and return incorrect scroll position
			// values. The ScrollViewRenderer for Android does something similar by waiting up
			// to 10ms for layout to occur.
			int cycle = 0;
			while (Element != null && !Element.IsInNativeLayout)
			{
				await Task.Delay(TimeSpan.FromMilliseconds(1));
				cycle++;

				if (cycle >= 10)
					break;
			}

			if (Element == null)
				return;

			double x = e.ScrollX, y = e.ScrollY;

			ScrollToMode mode = e.Mode;
			if (mode == ScrollToMode.Element)
			{
				Point pos = Element.GetScrollPositionForElement((VisualElement)e.Element, e.Position);
				x = pos.X;
				y = pos.Y;
				mode = ScrollToMode.Position;
			}

			if (mode == ScrollToMode.Position)
			{
				Control.ChangeView(x, y, null, !e.ShouldAnimate);
			}
			Element.SendScrollFinished();
		}
				
		void SetInitialRtlPosition(object sender, object e)
		{
			if (Control == null) return;

			if (Control.ActualWidth <= 0 || _checkedForRtlScroll || Control.Content == null)
				return;

			if (Element is IVisualElementController controller && controller.EffectiveFlowDirection.IsLeftToRight())
			{
				ClearRtlScrollCheck();
				return;
			}

			var element = (Control.Content as FrameworkElement);
			if (element.ActualWidth == Control.ActualWidth)
				return;

			ClearRtlScrollCheck();
			Control.ChangeView(element.ActualWidth, 0, null, true);
		}

		void ClearRtlScrollCheck()
		{
			_checkedForRtlScroll = true;
			var element = (Control.Content as FrameworkElement);
			if (element != null)
				element.LayoutUpdated -= SetInitialRtlPosition;
		}

		void OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
		{
			ClearRtlScrollCheck();
			Element.SetScrolledPosition(Control.HorizontalOffset, Control.VerticalOffset);

			if (!e.IsIntermediate)
				Element.SendScrollFinished();
		}

		Windows.UI.Xaml.Thickness AddMargin(Windows.UI.Xaml.Thickness original, double left, double top, double right, double bottom)
		{
			return new Windows.UI.Xaml.Thickness(original.Left + left, original.Top + top, original.Right + right, original.Bottom + bottom);
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
					element.Margin = AddMargin(element.Margin, Element.Padding.Left, 0, Element.Padding.Right, 0);
					break;
				case ScrollOrientation.Vertical:
					// need to add top/bottom margins
					element.Margin = AddMargin(element.Margin, 0, Element.Padding.Top, 0, Element.Padding.Bottom);
					break;
				case ScrollOrientation.Both:
					// need to add all margins
					element.Margin = AddMargin(element.Margin, Element.Padding.Left, Element.Padding.Top, Element.Padding.Right, Element.Padding.Bottom);
					break;
			}
		}

		void UpdateOrientation()
		{
			if (Element.Orientation == ScrollOrientation.Horizontal || Element.Orientation == ScrollOrientation.Both)
			{
				Control.HorizontalScrollBarVisibility = UwpScrollBarVisibility.Auto;
			}
			else
			{
				Control.HorizontalScrollBarVisibility = UwpScrollBarVisibility.Disabled;
			}
		}

		UwpScrollBarVisibility ScrollBarVisibilityToUwp(ScrollBarVisibility visibility)
		{
			switch (visibility)
			{
				case ScrollBarVisibility.Always:
					return UwpScrollBarVisibility.Visible;
				case ScrollBarVisibility.Default:
					return UwpScrollBarVisibility.Auto;
				case ScrollBarVisibility.Never:
					return UwpScrollBarVisibility.Hidden;
				default:
					return UwpScrollBarVisibility.Auto;
			}
		}

		void UpdateVerticalScrollBarVisibiilty()
		{
			Control.VerticalScrollBarVisibility = ScrollBarVisibilityToUwp(Element.VerticalScrollBarVisibility);
		}

		void UpdateHorizontalScrollBarVisibility()
		{
			var orientation = Element.Orientation;
			if (orientation == ScrollOrientation.Horizontal || orientation == ScrollOrientation.Both)
				Control.HorizontalScrollBarVisibility = ScrollBarVisibilityToUwp(Element.HorizontalScrollBarVisibility);
		}
	}
}
