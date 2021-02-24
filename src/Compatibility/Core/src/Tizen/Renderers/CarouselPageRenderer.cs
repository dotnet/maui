using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using ElmSharp;
using EBox = ElmSharp.Box;
using ELayout = ElmSharp.Layout;
using ERect = ElmSharp.Rect;
using ESize = ElmSharp.Size;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	/// <summary>
	/// Renderer of a CarouselPage widget.
	/// </summary>
	public class CarouselPageRenderer : VisualElementRenderer<CarouselPage>
	{
		const int ItemMaxCount = 20;
		const int OddMiddleItem = 10;
		const int EvenMiddleItem = 11;

		Index _index;
		List<IndexItem> _items = new List<IndexItem>();

		bool _isUpdateCarousel;
		int _childCount;

		Box _outterLayout;
		EBox _innerContainer;
		Scroller _scroller;

		int _pageIndex = 0;

		int _changedByScroll = 0;
		ESize _layoutBound;
		bool _isInitalized = false;

		protected override void OnElementChanged(ElementChangedEventArgs<CarouselPage> e)
		{
			if (NativeView == null)
			{
				_outterLayout = new Box(Forms.NativeParent)
				{
					AlignmentX = -1,
					AlignmentY = -1,
					WeightX = 1,
					WeightY = 1,
				};
				_outterLayout.Show();

				_scroller = new Scroller(Forms.NativeParent)
				{
					HorizontalScrollBarVisiblePolicy = ScrollBarVisiblePolicy.Invisible,
					VerticalScrollBarVisiblePolicy = ScrollBarVisiblePolicy.Invisible,
					HorizontalPageScrollLimit = 1,
					HorizontalRelativePageSize = 1.0,
					AlignmentX = -1,
					AlignmentY = -1,
					WeightX = 1,
					WeightY = 1,
				};
				_scroller.PageScrolled += OnPageScrolled;
				_scroller.Show();

				_innerContainer = new Box(Forms.NativeParent)
				{
					AlignmentX = -1,
					AlignmentY = -1,
					WeightX = 1,
					WeightY = 1,
				};
				_innerContainer.SetLayoutCallback(OnInnerLayoutUpdate);
				_innerContainer.Show();
				_scroller.SetContent(_innerContainer);

				_index = new Index(Forms.NativeParent)
				{
					IsHorizontal = true,
					AutoHide = false,
					AlignmentX = -1,
					AlignmentY = -1,
					WeightX = 1,
					WeightY = 1,
				};
				_index.Changed += OnIndexChanged;
				_index.Show();

				_outterLayout.SetLayoutCallback(OnOutterLayoutUpdate);
				_outterLayout.PackEnd(_scroller);
				_outterLayout.PackEnd(_index);

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
			UpdateIndexItem();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Element != null)
				{
					Element.CurrentPageChanged -= OnCurrentPageChanged;
					Element.PagesChanged -= OnPagesChanged;
					_scroller.PageScrolled -= OnPageScrolled;
					_index.Changed -= OnIndexChanged;
				}
				_innerContainer = null;
				_scroller = null;
				_index = null;
			}
			base.Dispose(disposing);
		}

		void OnOutterLayoutUpdate()
		{
			_scroller.Geometry = _outterLayout.Geometry;
			var newGeometry = _outterLayout.Geometry;
			if (Device.Idiom != TargetIdiom.Watch)
				newGeometry.Y += (int)(newGeometry.Height * 0.9);
			newGeometry.Height = (int)(newGeometry.Height * 0.1);
			_index.Geometry = newGeometry;
		}

		void OnInnerLayoutUpdate()
		{
			if (!_isInitalized || (_layoutBound == _innerContainer.Geometry.Size && _childCount == Element.Children.Count))
				return;

			_layoutBound = _innerContainer.Geometry.Size;
			_childCount = Element.Children.Count;

			int baseX = _innerContainer.Geometry.X;
			ERect bound = _scroller.Geometry;
			int index = 0;

			foreach (var page in Element.Children)
			{
				var nativeView = Platform.GetRenderer(page).NativeView;
				bound.X = baseX + index * bound.Width;
				nativeView.Geometry = bound;
				index++;
			}

			var widthRequest = _childCount * bound.Width;
			_innerContainer.MinimumWidth = widthRequest;
			if (_innerContainer.Geometry.Width == widthRequest && _scroller.HorizontalPageIndex != _pageIndex)
				_scroller.ScrollTo(_pageIndex, 0, true);
		}

		void OnCurrentPageChanged(object sender, EventArgs e)
		{
			CustomFocusManager.StartReorderTabIndex();

			Element.UpdateFocusTreePolicy();

			if (IsChangedByScroll())
				return;

			if (Element.CurrentPage != Element.Children[_pageIndex])
			{
				var previousPageIndex = _pageIndex;
				_pageIndex = Element.Children.IndexOf(Element.CurrentPage);
				if (previousPageIndex != _pageIndex)
				{
					(Element.Children[previousPageIndex] as IPageController)?.SendDisappearing();
					_scroller.ScrollTo(_pageIndex, 0, false);
					(Element.CurrentPage as IPageController)?.SendAppearing();
					OnSelect(_pageIndex);
				}
			}
		}

		void OnPageScrolled(object sender, EventArgs e)
		{
			if (_isUpdateCarousel)
			{
				_isUpdateCarousel = false;
				return;
			}

			_changedByScroll++;
			int previousIndex = _pageIndex;
			_pageIndex = _scroller.HorizontalPageIndex;
			if (previousIndex != _pageIndex)
			{
				(Element.Children[previousIndex] as IPageController)?.SendDisappearing();
				Element.CurrentPage = Element.Children[_pageIndex];
				(Element.CurrentPage as IPageController)?.SendAppearing();
				OnSelect(_pageIndex);
			}
			Element.UpdateFocusTreePolicy();
			_changedByScroll--;
		}

		void UpdateCarouselContent()
		{
			_innerContainer.UnPackAll();
			_layoutBound = new ESize(0, 0);
			foreach (var page in Element.Children)
			{
				var nativeView = Platform.GetOrCreateRenderer(page).NativeView;
				_innerContainer.PackEnd(nativeView);
			}
			_pageIndex = Element.Children.IndexOf(Element.CurrentPage);

			_isUpdateCarousel = true;
			_scroller.ScrollTo(_pageIndex, 0, false);
			Element.UpdateFocusTreePolicy();
			_isUpdateCarousel = false;
		}

		void OnPagesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateCarouselContent();
			UpdateIndexItem();
		}

		bool IsChangedByScroll()
		{
			return _changedByScroll > 0;
		}

		void OnSelect(int selectIndex)
		{
			if (selectIndex >= ItemMaxCount)
				selectIndex = ItemMaxCount - 1;
			if (selectIndex > -1)
				_items[selectIndex].Select(true);
		}

		void OnIndexChanged(object sender, EventArgs e)
		{
			var changedIndex = _items.IndexOf(_index.SelectedItem);
			if (changedIndex != Element.Children.IndexOf(Element.CurrentPage))
				_scroller.ScrollTo(changedIndex, 0, true);
		}

		void UpdateIndexItem()
		{
			_index.SetStyledIndex();
			_items.Clear();

			var indexCount = Element.Children.Count;
			if (indexCount > ItemMaxCount)
				indexCount = ItemMaxCount;
			for (int i = 0; i < indexCount; i++)
			{
				var item = _index.Append(i.ToString());
				if (Device.Idiom == TargetIdiom.Watch)
					item.SetIndexItemStyle(indexCount, i, EvenMiddleItem, OddMiddleItem);
				_items.Add(item);
			}
			_index.Update(0);
			OnSelect(_pageIndex);
		}
	}
}
