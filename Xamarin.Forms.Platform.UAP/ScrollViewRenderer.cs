using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Xamarin.Forms.Platform.UWP
{
	public class ScrollViewRenderer : ViewRenderer<ScrollView, ScrollViewer>
	{
		VisualElement _currentView;

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
				e.OldElement.ScrollToRequested -= OnScrollToRequested;
			}

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					SetNativeControl(new ScrollViewer { HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Auto });

					Control.ViewChanged += OnViewChanged;
				}

				Element.ScrollToRequested += OnScrollToRequested;

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

		async void OnScrollToRequested(object sender, ScrollToRequestedEventArgs e)
		{
			// Adding items into the view while scrolling to the end can cause it to fail, as
			// the items have not actually been laid out and return incorrect scroll position
			// values. The ScrollViewRenderer for Android does something similar by waiting up
			// to 10ms for layout to occur.
			int cycle = 0;
			while (!Element.IsInNativeLayout)
			{
				await Task.Delay(TimeSpan.FromMilliseconds(1));
				cycle++;

				if (cycle >= 10)
					break;
			}

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

		void OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
		{
			Element.SetScrolledPosition(Control.HorizontalOffset, Control.VerticalOffset);

			if (!e.IsIntermediate)
				Element.SendScrollFinished();
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