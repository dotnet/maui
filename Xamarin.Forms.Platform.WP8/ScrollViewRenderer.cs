using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class ScrollViewRenderer : ViewRenderer<ScrollView, ScrollViewer>
	{
		Animatable _animatable;

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

		protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
		{
			if (Element == null)
				return finalSize;

			Element.IsInNativeLayout = true;

			if (Control != null)
			{
				Control.Measure(finalSize);
				Control.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
			}

			Element.IsInNativeLayout = false;

			return finalSize;
		}

		protected override System.Windows.Size MeasureOverride(System.Windows.Size availableSize)
		{
			if (Element == null)
				return new System.Windows.Size(0, 0);

			double width = Math.Max(0, Element.Width);
			double height = Math.Max(0, Element.Height);
			return new System.Windows.Size(width, height);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ScrollView> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
				e.OldElement.ScrollToRequested -= OnScrollToRequested;

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					SetNativeControl(new ScrollViewer { ManipulationMode = ManipulationMode.Control });
					Control.LayoutUpdated += (sender, args) => { UpdateScrollPosition(); };
				}
				e.NewElement.ScrollToRequested += OnScrollToRequested;
			}

			SizeChanged += (sender, args) =>
			{
				Control.Width = ActualWidth;
				Control.Height = ActualHeight;
			};

			UpdateOrientation();

			LoadContent();
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

		static double GetDistance(double start, double position, double v)
		{
			return start + (position - start) * v;
		}

		void LoadContent()
		{
			var lastContent = Control.Content as FrameworkElement;
			if (lastContent != null)
				lastContent.Margin = new System.Windows.Thickness(); //undo any damage we may have done to this renderer

			View view = Element.Content;

			if (view != null)
				Platform.SetRenderer(view, Platform.CreateRenderer(view));

			Control.Content = view != null ? Platform.GetRenderer(view) : null;

			UpdateMargins();
		}

		void OnScrollToRequested(object sender, ScrollToRequestedEventArgs e)
		{
			if (_animatable == null && e.ShouldAnimate)
				_animatable = new Animatable();

			ScrollToPosition position = e.Position;
			double x = e.ScrollX;
			double y = e.ScrollY;

			if (e.Mode == ScrollToMode.Element)
			{
				Point itemPosition = Element.GetScrollPositionForElement(e.Element as VisualElement, e.Position);
				x = itemPosition.X;
				y = itemPosition.Y;
			}

			if (Control.VerticalOffset == y && Control.HorizontalOffset == x)
				return;

			if (e.ShouldAnimate)
			{
				var animation = new Animation(v => { UpdateScrollOffset(GetDistance(Control.ViewportWidth, x, v), GetDistance(Control.ViewportHeight, y, v)); });

				animation.Commit(_animatable, "ScrollTo", length: 500, easing: Easing.CubicInOut, finished: (v, d) =>
				{
					UpdateScrollOffset(x, y);
					Element.SendScrollFinished();
				});
			}
			else
			{
				UpdateScrollOffset(x, y);
				Element.SendScrollFinished();
			}
		}

		void UpdateMargins()
		{
			var element = Control.Content as FrameworkElement;
			if (element == null)
				return;

			if (Element.Orientation == ScrollOrientation.Horizontal)
			{
				// need to add left/right margins
				element.Margin = new System.Windows.Thickness(Element.Padding.Left, 0, Element.Padding.Right, 0);
			}
			else
			{
				// need to add top/bottom margins
				element.Margin = new System.Windows.Thickness(0, Element.Padding.Top, 0, Element.Padding.Bottom);
			}
		}

		void UpdateOrientation()
		{
			if (Element.Orientation == ScrollOrientation.Horizontal)
				Control.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
			else
				Control.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
		}

		void UpdateScrollOffset(double x, double y)
		{
			if (Element.Orientation == ScrollOrientation.Horizontal)
				Control.ScrollToHorizontalOffset(x);
			else
				Control.ScrollToVerticalOffset(y);
		}

		void UpdateScrollPosition()
		{
			if (Element != null)
				Element.SetScrolledPosition(Control.HorizontalOffset, Control.VerticalOffset);
		}
	}
}