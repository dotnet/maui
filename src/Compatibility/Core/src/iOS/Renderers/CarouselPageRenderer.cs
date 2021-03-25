using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using UIKit;
using Microsoft.Maui.Controls.Compatibility.Internals;
using PointF = CoreGraphics.CGPoint;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public class CarouselPageRenderer : UIViewController, IVisualElementRenderer
	{
		bool _appeared;
		Dictionary<Page, UIView> _containerMap;
		bool _disposed;
		EventTracker _events;
		bool _ignoreNativeScrolling;
		UIScrollView _scrollView;
		VisualElementTracker _tracker;
		Page _previousPage;

		[Preserve(Conditional = true)]
		public CarouselPageRenderer()
		{
		}

		IElementController ElementController => Element as IElementController;


		protected CarouselPage Carousel
		{
			get { return (CarouselPage)Element; }
		}

		IPageController PageController => (IPageController)Element;

		protected int SelectedIndex
		{
			get { return (int)(_scrollView.ContentOffset.X / _scrollView.Frame.Width); }
			set { ScrollToPage(value); }
		}

		public VisualElement Element { get; private set; }

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return NativeView.GetSizeRequest(widthConstraint, heightConstraint);
		}

		public UIView NativeView
		{
			get { return View; }
		}

		public void SetElement(VisualElement element)
		{
			VisualElement oldElement = Element;
			Element = element;
			_containerMap = new Dictionary<Page, UIView>();

			OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));

			if (element != null)
				element.SendViewInitialized(NativeView);

			_previousPage = Carousel?.CurrentPage;
		}

		public void SetElementSize(Size size)
		{
			Element.Layout(new Rectangle(Element.X, Element.Y, size.Width, size.Height));
		}

		public UIViewController ViewController
		{
			get { return this; }
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			_ignoreNativeScrolling = false;
			View.SetNeedsLayout();
		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);

			if (_appeared || _disposed)
				return;

			_appeared = true;
			PageController.SendAppearing();
		}

		public override void ViewDidDisappear(bool animated)
		{
			base.ViewDidDisappear(animated);

			if (!_appeared || _disposed)
				return;

			_appeared = false;
			PageController.SendDisappearing();
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			View.Frame = View.Superview.Bounds;
			_scrollView.Frame = View.Bounds;

			PositionChildren();
			UpdateCurrentPage(false);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			_tracker = new VisualElementTracker(this);
			_events = new EventTracker(this);
			_events.LoadEvents(View);

			_scrollView = new UIScrollView { ShowsHorizontalScrollIndicator = false };

			_scrollView.DecelerationEnded += OnDecelerationEnded;

			UpdateBackground();

			View.Add(_scrollView);

			for (var i = 0; i < ElementController.LogicalChildren.Count; i++)
			{
				Element element = ElementController.LogicalChildren[i];
				var child = element as ContentPage;
				if (child != null)
					InsertPage(child, i);
			}

			PositionChildren();

			Carousel.PropertyChanged += OnPropertyChanged;
			Carousel.PagesChanged += OnPagesChanged;
		}

		public override void ViewDidUnload()
		{
			base.ViewDidUnload();

			if (_scrollView != null)
				_scrollView.DecelerationEnded -= OnDecelerationEnded;

			if (Carousel != null)
			{
				Carousel.PropertyChanged -= OnPropertyChanged;
				Carousel.PagesChanged -= OnPagesChanged;
			}
		}

		public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			_ignoreNativeScrolling = true;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_previousPage = null;

				if (_scrollView != null)
					_scrollView.DecelerationEnded -= OnDecelerationEnded;

				if (Carousel != null)
				{
					Carousel.PropertyChanged -= OnPropertyChanged;
					Carousel.PagesChanged -= OnPagesChanged;
				}

				Platform.SetRenderer(Element, null);

				Clear();

				if (_scrollView != null)
				{
					_scrollView.DecelerationEnded -= OnDecelerationEnded;
					_scrollView.RemoveFromSuperview();
					_scrollView = null;
				}

				if (_appeared)
				{
					_appeared = false;
					PageController?.SendDisappearing();
				}

				if (_events != null)
				{
					_events.Dispose();
					_events = null;
				}

				if (_tracker != null)
				{
					_tracker.Dispose();
					_tracker = null;
				}

				Element = null;
				_disposed = true;
			}

			base.Dispose(disposing);
		}

		protected virtual void OnElementChanged(VisualElementChangedEventArgs e)
		{
			EventHandler<VisualElementChangedEventArgs> changed = ElementChanged;
			if (changed != null)
				changed(this, e);
		}

		void Clear()
		{
			foreach (KeyValuePair<Page, UIView> kvp in _containerMap)
			{
				kvp.Value.RemoveFromSuperview();
				IVisualElementRenderer renderer = Platform.GetRenderer(kvp.Key);
				if (renderer != null)
				{
					renderer.ViewController.RemoveFromParentViewController();
					renderer.NativeView.RemoveFromSuperview();
					Platform.SetRenderer(kvp.Key, null);
				}
			}
			_containerMap.Clear();
		}

		void InsertPage(ContentPage page, int index)
		{
			IVisualElementRenderer renderer = Platform.GetRenderer(page);
			if (renderer == null)
			{
				renderer = Platform.CreateRenderer(page);
				Platform.SetRenderer(page, renderer);
			}

			UIView container = new CarouselPageContainer(page);

			UIView view = renderer.NativeView;

			container.AddSubview(view);
			_containerMap[page] = container;

			AddChildViewController(renderer.ViewController);
			_scrollView.InsertSubview(container, index);

			if ((index == 0 && SelectedIndex == 0) || (index < SelectedIndex))
				ScrollToPage(SelectedIndex + 1, false);
		}

		void OnDecelerationEnded(object sender, EventArgs eventArgs)
		{
			if (_ignoreNativeScrolling || SelectedIndex >= ElementController.LogicalChildren.Count)
				return;
						
			var currentPage = (ContentPage)ElementController.LogicalChildren[SelectedIndex];
			if (_previousPage != currentPage)
			{
				_previousPage?.SendDisappearing();
				_previousPage = currentPage;
			}
			Carousel.CurrentPage = currentPage;
			currentPage.SendAppearing();
		}

		void OnPagesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_ignoreNativeScrolling = true;

			NotifyCollectionChangedAction action = e.Apply((o, i, c) => InsertPage((ContentPage)o, i), (o, i) => RemovePage((ContentPage)o, i), Reset);
			PositionChildren();

			_ignoreNativeScrolling = false;

			if (action == NotifyCollectionChangedAction.Reset)
			{
				int index = Carousel.CurrentPage != null ? CarouselPage.GetIndex(Carousel.CurrentPage) : 0;
				if (index < 0)
					index = 0;

				ScrollToPage(index);
			}
		}

		void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "CurrentPage")
				UpdateCurrentPage();
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName || e.PropertyName == VisualElement.BackgroundProperty.PropertyName)
				UpdateBackground();
			else if (e.PropertyName == Page.BackgroundImageSourceProperty.PropertyName)
				UpdateBackground();
		}

		void PositionChildren()
		{
			nfloat x = 0;
			RectangleF bounds = View.Bounds;
			foreach (ContentPage child in ((CarouselPage)Element).Children)
			{
				UIView container = _containerMap[child];

				container.Frame = new RectangleF(x, bounds.Y, bounds.Width, bounds.Height);
				x += bounds.Width;
			}

			_scrollView.PagingEnabled = true;
			_scrollView.ContentSize = new SizeF(bounds.Width * ((CarouselPage)Element).Children.Count, bounds.Height);
		}

		void RemovePage(ContentPage page, int index)
		{
			UIView container = _containerMap[page];
			container.RemoveFromSuperview();
			_containerMap.Remove(page);

			IVisualElementRenderer renderer = Platform.GetRenderer(page);
			if (renderer == null)
				return;

			renderer.ViewController.RemoveFromParentViewController();
			renderer.NativeView.RemoveFromSuperview();
		}

		void Reset()
		{
			Clear();

			for (var i = 0; i < ElementController.LogicalChildren.Count; i++)
			{
				Element element = ElementController.LogicalChildren[i];
				var child = element as ContentPage;
				if (child != null)
					InsertPage(child, i);
			}
		}

		void ScrollToPage(int index, bool animated = true)
		{
			if (_scrollView.ContentOffset.X == index * _scrollView.Frame.Width)
				return;

			_scrollView.SetContentOffset(new PointF(index * _scrollView.Frame.Width, 0), animated);
		}

		void UpdateBackground()
		{
			this.ApplyNativeImageAsync(Page.BackgroundImageSourceProperty, bgImage =>
			{
				if (bgImage != null)
					View.BackgroundColor = UIColor.FromPatternImage(bgImage);
				else if (Element.BackgroundColor.IsDefault)
					View.BackgroundColor = ColorExtensions.BackgroundColor;
				else
				{
					if (Element.BackgroundColor.IsDefault)
						View.BackgroundColor = UIColor.White;
					else
						View.BackgroundColor = Element.BackgroundColor.ToUIColor();

					Brush background = Element.Background;
					View.UpdateBackground(background);
				}
			});
		}

		void UpdateCurrentPage(bool animated = true)
		{
			ContentPage current = Carousel.CurrentPage;
			if (current != null)
				ScrollToPage(CarouselPage.GetIndex(current), animated);
		}

		class CarouselPageContainer : UIView
		{
			public CarouselPageContainer(VisualElement element)
			{
				Element = element;
			}

			public VisualElement Element { get; }

			public override void LayoutSubviews()
			{
				base.LayoutSubviews();

				if (Subviews.Length > 0)
					Subviews[0].Frame = new RectangleF(0, 0, (float)Element.Width, (float)Element.Height);
			}
		}

		public void RegisterEffect(Effect effect)
		{
			VisualElementRenderer<VisualElement>.RegisterEffect(effect, View);
		}
	}
}