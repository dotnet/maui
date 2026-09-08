using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using UwpScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility;
using WRect = Windows.Foundation.Rect;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public partial class ScrollViewRenderer : ViewRenderer<ScrollView, ScrollViewer>
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

		protected override global::Windows.Foundation.Size ArrangeOverride(global::Windows.Foundation.Size finalSize)
		{
			if (Element == null || Control == null)
				return finalSize;

			Element.IsInPlatformLayout = true;

			Control?.Arrange(new WRect(0, 0, finalSize.Width, finalSize.Height));

			Element.IsInPlatformLayout = false;

			return finalSize;
		}

		protected override void Dispose(bool disposing)
		{
			CleanUp(Element, Control);
			base.Dispose(disposing);
		}

		protected override global::Windows.Foundation.Size MeasureOverride(global::Windows.Foundation.Size availableSize)
		{
			if (Element == null)
				return new global::Windows.Foundation.Size(0, 0);

			double width = Math.Max(0, Element.Width);
			double height = Math.Max(0, Element.Height);
			var result = new global::Windows.Foundation.Size(width, height);

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

			_currentView?.Cleanup();
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
				UpdateContentMargins();
			else if (e.PropertyName == ScrollView.OrientationProperty.PropertyName)
				UpdateOrientation();
			else if (e.PropertyName == ScrollView.VerticalScrollBarVisibilityProperty.PropertyName)
				UpdateVerticalScrollBarVisibility();
			else if (e.PropertyName == ScrollView.HorizontalScrollBarVisibilityProperty.PropertyName)
				UpdateHorizontalScrollBarVisibility();
		}

		protected void OnContentElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == View.MarginProperty.PropertyName)
				UpdateContentMargins();
		}

		void UpdateContent()
		{
			_currentView?.Cleanup();

			if (Control?.Content is FrameworkElement oldElement)
			{
				oldElement.LayoutUpdated -= SetInitialRtlPosition;

				if (oldElement is IVisualElementRenderer oldRenderer
					&& oldRenderer.Element is View oldContentView)
					oldContentView.PropertyChanged -= OnContentElementPropertyChanged;
			}

			_currentView = Element.Content;

			IVisualElementRenderer renderer = null;
			if (_currentView != null)
				renderer = _currentView.GetOrCreateRenderer();

			Control.Content = renderer?.ContainerElement;

			UpdateContentMargins();
			if (renderer?.Element != null)
				renderer.Element.PropertyChanged += OnContentElementPropertyChanged;

			if (renderer?.ContainerElement != null)
				renderer.ContainerElement.LayoutUpdated += SetInitialRtlPosition;
		}

		async void OnScrollToRequested(object sender, ScrollToRequestedEventArgs e)
		{
			ClearRtlScrollCheck();

			// Adding items into the view while scrolling to the end can cause it to fail, as
			// the items have not actually been arranged and return incorrect scroll position
			// values. The ScrollViewRenderer for Android does something similar by waiting up
			// to 10ms for layout to occur.
			int cycle = 0;
			while (Element != null && !Element.IsInPlatformLayout)
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
			if (Control == null)
				return;

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
			if (Control.Content is FrameworkElement element)
				element.LayoutUpdated -= SetInitialRtlPosition;
		}

		void OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
		{
			ClearRtlScrollCheck();
			Element.SetScrolledPosition(Control.HorizontalOffset, Control.VerticalOffset);

			if (!e.IsIntermediate)
				Element.SendScrollFinished();
		}

		Microsoft.UI.Xaml.Thickness AddMargin(Thickness original, double left, double top, double right, double bottom)
		{
			return WinUIHelpers.CreateThickness(original.Left + left, original.Top + top, original.Right + right, original.Bottom + bottom);
		}

		// UAP ScrollView forces Content origin to be the same as the ScrollView origin.
		// This prevents Forms layout from emulating Padding and Margin by offsetting the origin. 
		// So we must actually set the UAP Margin property instead of emulating it with an origin offset. 
		// Not only that, but in UAP Padding and Margin are aliases with
		// the former living on the parent and the latter on the child. 
		// So that's why the UAP Margin is set to the sum of the Forms Padding and Forms Margin.
		void UpdateContentMargins()
		{
			if (!(Control.Content is FrameworkElement element
				&& element is IVisualElementRenderer renderer
				&& renderer.Element is View contentView))
				return;

			var margin = contentView.Margin;
			var padding = Element.Padding;
			switch (Element.Orientation)
			{
				case ScrollOrientation.Horizontal:
					// need to add left/right margins
					element.Margin = AddMargin(margin, padding.Left, 0, padding.Right, 0);
					break;
				case ScrollOrientation.Vertical:
					// need to add top/bottom margins
					element.Margin = AddMargin(margin, 0, padding.Top, 0, padding.Bottom);
					break;
				case ScrollOrientation.Both:
					// need to add all margins
					element.Margin = AddMargin(margin, padding.Left, padding.Top, padding.Right, padding.Bottom);
					break;
			}
		}

		void UpdateOrientation()
		{
			//Only update the horizontal scroll bar visibility if the user has not set a desired state.
			if (Element.HorizontalScrollBarVisibility != ScrollBarVisibility.Default)
				return;

			var orientation = Element.Orientation;
			if (orientation == ScrollOrientation.Horizontal || orientation == ScrollOrientation.Both)
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

		void UpdateVerticalScrollBarVisibility()
		{
			Control.VerticalScrollBarVisibility = ScrollBarVisibilityToUwp(Element.VerticalScrollBarVisibility);
		}

		void UpdateHorizontalScrollBarVisibility()
		{
			var horizontalVisibility = Element.HorizontalScrollBarVisibility;

			if (horizontalVisibility == ScrollBarVisibility.Default)
			{
				UpdateOrientation();
				return;
			}

			var orientation = Element.Orientation;
			if (orientation == ScrollOrientation.Horizontal || orientation == ScrollOrientation.Both)
				Control.HorizontalScrollBarVisibility = ScrollBarVisibilityToUwp(horizontalVisibility);
		}
	}
}