using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Xamarin.Forms.Platform.WPF
{
	public class ScrollViewRenderer : ViewRenderer<ScrollView, ScrollViewer>
	{
		VisualElement _currentView;
		Animatable _animatable;
		
		protected IScrollViewController Controller
		{
			get { return Element; }
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ScrollView> e)
		{
			if (e.OldElement != null) // Clear old element event
			{
				((IScrollViewController)e.OldElement).ScrollToRequested -= OnScrollToRequested;
			}

			if (e.NewElement != null)
			{
				if (Control == null) // construct and SetNativeControl and suscribe control event
				{
					SetNativeControl(new ScrollViewer() { IsManipulationEnabled = true, PanningMode = PanningMode.Both });
					Control.LayoutUpdated += NativeLayoutUpdated;
				}

				// Update control property 
				UpdateOrientation();
				LoadContent();

				// Suscribe element event
				Controller.ScrollToRequested += OnScrollToRequested;
			}

			base.OnElementChanged(e);
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

		void NativeLayoutUpdated(object sender, EventArgs e)
		{
			UpdateScrollPosition();
		}

		static double GetDistance(double start, double position, double v)
		{
			return start + (position - start) * v;
		}

		void LoadContent()
		{
			if (_currentView != null)
			{
				_currentView.Cleanup(); // cleanup old view
			}

			_currentView = Element.Content;

			if(_currentView != null)
			{
				/*
				 * Wrap Content in a DockPanel : The goal is to reduce ce Measure Cycle on scolling
				 */
				DockPanel dockPanel = new DockPanel();
				dockPanel.Children.Add(Platform.GetOrCreateRenderer(_currentView).GetNativeElement());
				Control.Content = dockPanel;
			}
			else
			{
				Control.Content = null;
			}
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
				Point itemPosition = Controller.GetScrollPositionForElement(e.Element as VisualElement, e.Position);
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
					Controller.SendScrollFinished();
				});
			}
			else
			{
				UpdateScrollOffset(x, y);
				Controller.SendScrollFinished();
			}
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
					element.Margin = new System.Windows.Thickness(Element.Padding.Left, 0, 10, 0);
					break;
				case ScrollOrientation.Vertical:
					// need to add top/bottom margins
					element.Margin = new System.Windows.Thickness(0, Element.Padding.Top, 0, Element.Padding.Bottom);
					break;
				case ScrollOrientation.Both:
					// need to add all margins
					element.Margin = new System.Windows.Thickness(Element.Padding.Left, Element.Padding.Top, Element.Padding.Right, Element.Padding.Bottom);
					break;
			}
		}

		void UpdateOrientation()
		{
			if (Element.Orientation == ScrollOrientation.Horizontal || Element.Orientation == ScrollOrientation.Both)
				Control.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
			else
				Control.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;

			if (Element.Orientation == ScrollOrientation.Vertical || Element.Orientation == ScrollOrientation.Both)
				Control.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
			else
				Control.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
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
				Controller.SetScrolledPosition(Control.HorizontalOffset, Control.VerticalOffset);
		}

		bool _isDisposed;

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				if (Control != null)
				{
					Control.LayoutUpdated -= NativeLayoutUpdated;
				}

				if (Element != null)
				{
					Controller.ScrollToRequested -= OnScrollToRequested;
				}
			}

			_isDisposed = true;
			base.Dispose(disposing);
		}
	}
}