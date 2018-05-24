using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using AppKit;
using Foundation;

namespace Xamarin.Forms.Platform.MacOS
{
	[Register("CarouselPageRenderer")]
	public class CarouselPageRenderer : NSPageController, IVisualElementRenderer
	{
		bool _appeared;
		bool _disposed;
		EventTracker _events;
		VisualElementTracker _tracker;

		public CarouselPageRenderer()
		{
			View = new NSView
			{
				WantsLayer = true,
				Layer = { BackgroundColor = NSColor.White.CGColor }
			};
		}

		public CarouselPageRenderer(IntPtr handle) : base(handle)
		{
		}

		Page Page => (Page)Element;

		public override nint SelectedIndex
		{
			get { return base.SelectedIndex; }
			set
			{
				if (base.SelectedIndex == value)
					return;
				base.SelectedIndex = value;
				if (Carousel != null)
					Carousel.CurrentPage = (ContentPage)Element.LogicalChildren[(int)SelectedIndex];
			}
		}

		public VisualElement Element { get; private set; }

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return NativeView.GetSizeRequest(widthConstraint, heightConstraint);
		}

		public NSView NativeView => View;

		public void SetElement(VisualElement element)
		{
			VisualElement oldElement = Element;
			Element = element;

			Init();

			RaiseElementChanged(new VisualElementChangedEventArgs(oldElement, element));
		}

		public void SetElementSize(Size size)
		{
			Element.Layout(new Rectangle(Element.X, Element.Y, size.Width, size.Height));
		}

		public NSViewController ViewController => this;

		public override void ViewDidAppear()
		{
			base.ViewDidAppear();
			if (_appeared || _disposed)
				return;

			_appeared = true;
			Page.SendAppearing();
		}

		public override void ViewDidDisappear()
		{
			base.ViewDidDisappear();

			if (!_appeared || _disposed)
				return;

			_appeared = false;
			Page.SendDisappearing();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				if (Carousel != null)
				{
					Carousel.PropertyChanged -= OnElementPropertyChanged;
					Carousel.PagesChanged -= OnPagesChanged;
				}

				Platform.SetRenderer(Element, null);

				if (_appeared)
				{
					_appeared = false;
					Page?.SendDisappearing();
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

		void RaiseElementChanged(VisualElementChangedEventArgs e)
		{
			OnElementChanged(e);
			ElementChanged?.Invoke(this, e);
		}

		protected virtual void OnElementChanged(VisualElementChangedEventArgs e)
		{
		}

		void ConfigureNSPageController()
		{
			TransitionStyle = NSPageControllerTransitionStyle.HorizontalStrip;
		}

		CarouselPage Carousel => Element as CarouselPage;

		void Init()
		{
			Delegate = new PageControllerDelegate();

			_tracker = new VisualElementTracker(this);
			_events = new EventTracker(this);
			_events.LoadEvents(View);

			ConfigureNSPageController();

			UpdateBackground();
			UpdateSource();

			Carousel.PropertyChanged += OnElementPropertyChanged;
			Carousel.PagesChanged += OnPagesChanged;
		}

		void UpdateSource()
		{
			if (Element.LogicalChildren.Count == 0 && ArrangedObjects.Length == 0)
				return;
			
			var pages = new List<NSPageContainer>();
			for (var i = 0; i < Element.LogicalChildren.Count; i++)
			{
				Element element = Element.LogicalChildren[i];
				var child = element as ContentPage;
				if (child != null)
					pages.Add(new NSPageContainer(child, i));
			}

			ArrangedObjects = pages.ToArray();
			UpdateCurrentPage(false);
		}

		void OnPagesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateSource();
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(TabbedPage.CurrentPage))
				UpdateCurrentPage();
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackground();
			else if (e.PropertyName == Page.BackgroundImageProperty.PropertyName)
				UpdateBackground();
		}

		void UpdateBackground()
		{
			if (View.Layer == null)
				return;

			string bgImage = ((Page)Element).BackgroundImage;

			if (!string.IsNullOrEmpty(bgImage))
			{
				View.Layer.BackgroundColor = NSColor.FromPatternImage(NSImage.ImageNamed(bgImage)).CGColor;
				return;
			}

			Color bgColor = Element.BackgroundColor;
			View.Layer.BackgroundColor = bgColor.IsDefault ? NSColor.White.CGColor : bgColor.ToCGColor();
		}

		void UpdateCurrentPage(bool animated = true)
		{
			ContentPage current = Carousel.CurrentPage;
			if (current != null)
			{
				int index = Carousel.CurrentPage != null ? CarouselPage.GetIndex(Carousel.CurrentPage) : 0;
				if (index < 0)
					index = 0;

				if (SelectedIndex == index)
					return;

				if (animated)
					NSAnimationContext.RunAnimation(context => { ((NSPageController)Animator).SelectedIndex = index; },
						CompleteTransition);
				else SelectedIndex = index;
			}
		}
	}
}