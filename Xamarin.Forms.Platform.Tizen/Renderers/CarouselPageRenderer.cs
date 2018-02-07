using System;
using ElmSharp;
using ESize = ElmSharp.Size;

namespace Xamarin.Forms.Platform.Tizen
{
	/// <summary>
	/// Renderer of a CarouselPage widget.
	/// </summary>
	public class CarouselPageRenderer : VisualElementRenderer<CarouselPage>
	{
		Box _outterLayout;
		Box _innerContainer;
		Scroller _scroller;

		int _pageIndex = 0;

		int _changedByScroll = 0;
		ESize _layoutBound;
		bool _isInitalized = false;

		/// <summary>
		/// Invoked whenever the CarouselPage element has been changed in Xamarin.
		/// </summary>
		/// <param name="e">Event parameters.</param>
		protected override void OnElementChanged(ElementChangedEventArgs<CarouselPage> e)
		{
			if (NativeView == null)
			{
				//Create box that holds toolbar and selected content
				_outterLayout = new Box(Forms.NativeParent)
				{
					AlignmentX = -1,
					AlignmentY = -1,
					WeightX = 1,
					WeightY = 1,
					IsHorizontal = false,
				};
				_outterLayout.Show();

				_scroller = new Scroller(Forms.NativeParent);
				_scroller.PageScrolled += OnScrolled;

				// Disables the visibility of the scrollbar in both directions:
				_scroller.HorizontalScrollBarVisiblePolicy = ElmSharp.ScrollBarVisiblePolicy.Invisible;
				_scroller.VerticalScrollBarVisiblePolicy = ElmSharp.ScrollBarVisiblePolicy.Invisible;
				// Sets the limit of scroll to one page maximum:
				_scroller.HorizontalPageScrollLimit = 1;
				_scroller.SetPageSize(1.0, 1.0);
				_scroller.SetAlignment(-1, -1);
				_scroller.SetWeight(1.0, 1.0);
				_scroller.Show();

				_innerContainer = new Box(Forms.NativeParent);
				_innerContainer.SetLayoutCallback(OnInnerLayoutUpdate);
				_innerContainer.SetAlignment(-1, -1);
				_innerContainer.SetWeight(1.0, 1.0);
				_innerContainer.Show();
				_scroller.SetContent(_innerContainer);

				_outterLayout.PackEnd(_scroller);
				SetNativeView(_outterLayout);
			}

			if (e.OldElement != null)
			{
				e.OldElement.CurrentPageChanged -= OnCurrentPageChanged;
				e.OldElement.PagesChanged -= OnPagesChanged;
				_isInitalized = false;

			}

			if (e.NewElement != null)
			{
				Element.CurrentPageChanged += OnCurrentPageChanged;
				Element.PagesChanged += OnPagesChanged;
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementReady()
		{
			base.OnElementReady();
			_isInitalized = true;
			UpdateCarouselContent();
		}

		/// <summary>
		/// Called just before the associated element is deleted.
		/// </summary>
		/// <param name="disposing">True if the memory release was requested on demand.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Element != null)
				{
					Element.CurrentPageChanged -= OnCurrentPageChanged;
					Element.PagesChanged -= OnPagesChanged;
				}
				_innerContainer = null;
				_scroller = null;
			}
			base.Dispose(disposing);
		}

		void OnInnerLayoutUpdate()
		{
			if (!_isInitalized || _layoutBound == _innerContainer.Geometry.Size)
				return;

			_layoutBound = _innerContainer.Geometry.Size;

			int baseX = _innerContainer.Geometry.X;
			Rect bound = _scroller.Geometry;
			int index = 0;
			foreach (var page in Element.Children)
			{
				var nativeView = Platform.GetRenderer(page).NativeView;
				bound.X = baseX + index * bound.Width;
				nativeView.Geometry = bound;
				index++;
			}
			_innerContainer.MinimumWidth = Element.Children.Count * bound.Width;

			if (_scroller.HorizontalPageIndex != _pageIndex)
			{
				// If you change the orientation of the device and the Animation is set to false, it will not work.
				_scroller.ScrollTo(_pageIndex, 0, true);
			}
		}


		/// <summary>
		/// Handles the process of switching between the displayed pages.
		/// </summary>
		/// <param name="sender">An object originating the request</param>
		/// <param name="ea">Additional arguments to the event handler</param>
		void OnCurrentPageChanged(object sender, EventArgs ea)
		{
			if (IsChangedByScroll())
				return;
			Element.UpdateFocusTreePolicy();

			if (Element.CurrentPage != Element.Children[_pageIndex])
			{
				var previousPageIndex = _pageIndex;
				_pageIndex = Element.Children.IndexOf(Element.CurrentPage);
				if (previousPageIndex != _pageIndex)
				{
					// notify disappearing/appearing pages and scroll to the requested page
					(Element.Children[previousPageIndex] as IPageController)?.SendDisappearing();
					_scroller.ScrollTo(_pageIndex, 0, false);
					(Element.CurrentPage as IPageController)?.SendAppearing();
				}
			}
		}

		/// <summary>
		/// Handles the PageScrolled event of the _scroller.
		/// </summary>
		void OnScrolled(object sender, EventArgs e2)
		{
			_changedByScroll++;
			int previousIndex = _pageIndex;
			_pageIndex = _scroller.HorizontalPageIndex;

			if (previousIndex != _pageIndex)
			{
				(Element.CurrentPage as IPageController)?.SendDisappearing();
				Element.CurrentPage = Element.Children[_pageIndex];
				(Element.CurrentPage as IPageController)?.SendAppearing();
			}

			_changedByScroll--;
		}

		/// <summary>
		/// Updates the content of the carousel.
		/// </summary>
		void UpdateCarouselContent()
		{
			_innerContainer.UnPackAll();
			foreach (var page in Element.Children)
			{
				EvasObject nativeView = Platform.GetOrCreateRenderer(page).NativeView;
				_innerContainer.PackEnd(nativeView);
			}
			_pageIndex = Element.Children.IndexOf(Element.CurrentPage);
			_scroller.ScrollTo(_pageIndex, 0, false);
			Element.UpdateFocusTreePolicy();
		}

		/// <summary>
		/// Handles the notifications about content sub-page changes.
		/// </summary>
		void OnPagesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			UpdateCarouselContent();
		}

		bool IsChangedByScroll()
		{
			return _changedByScroll > 0;
		}
	}
}

