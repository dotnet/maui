#nullable disable
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Windows.Foundation;
using WApp = Microsoft.UI.Xaml.Application;
using WDataTemplate = Microsoft.UI.Xaml.DataTemplate;
using WScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility;
using WScrollMode = Microsoft.UI.Xaml.Controls.ScrollMode;
using WSnapPointsAlignment = Microsoft.UI.Xaml.Controls.Primitives.SnapPointsAlignment;
using WSnapPointsType = Microsoft.UI.Xaml.Controls.SnapPointsType;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class CarouselViewHandler : ItemsViewHandler<CarouselView>
	{
		LoopableCollectionView _loopableCollectionView;
		ScrollViewer _scrollViewer;
		WScrollBarVisibility? _horizontalScrollBarVisibilityWithoutLoop;
		WScrollBarVisibility? _verticalScrollBarVisibilityWithoutLoop;
		Size _currentSize;
		bool _isCarouselViewReady;
		NotifyCollectionChangedEventHandler _collectionChanged;
		readonly WeakNotifyCollectionChangedProxy _proxy = new();

		~CarouselViewHandler() => _proxy.Unsubscribe();

		protected override IItemsLayout Layout { get; }

		LinearItemsLayout CarouselItemsLayout => ItemsView?.ItemsLayout;
		WDataTemplate CarouselItemsViewTemplate => (WDataTemplate)WApp.Current.Resources["CarouselItemsViewDefaultTemplate"];

		protected override void ConnectHandler(ListViewBase platformView)
		{
			ItemsView.Scrolled += CarouselScrolled;
			platformView.SizeChanged += OnListViewSizeChanged;

			UpdateScrollBarVisibilityForLoop();

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(ListViewBase platformView)
		{
			if (ItemsView != null)
				ItemsView.Scrolled -= CarouselScrolled;

			if (platformView != null)
			{
				platformView.SizeChanged -= OnListViewSizeChanged;
				_proxy.Unsubscribe();
			}

			if (_scrollViewer != null)
			{
				_scrollViewer.ViewChanging -= OnScrollViewChanging;
				_scrollViewer.ViewChanged -= OnScrollViewChanged;
				_scrollViewer.SizeChanged -= OnScrollViewSizeChanged;
			}

			base.DisconnectHandler(platformView);
		}

		protected override void UpdateItemsSource()
		{
			var itemTemplate = ItemsView?.ItemTemplate;

			if (itemTemplate == null)
				return;

			base.UpdateItemsSource();
		}

		protected override void UpdateItemTemplate()
		{
			if (Element == null || ListViewBase == null)
				return;

			ListViewBase.ItemTemplate = CarouselItemsViewTemplate;
			UpdateItemsSource();
		}

		protected override void OnScrollViewerFound(ScrollViewer scrollViewer)
		{
			base.OnScrollViewerFound(scrollViewer);

			_scrollViewer = scrollViewer;
			_scrollViewer.ViewChanging += OnScrollViewChanging;
			_scrollViewer.ViewChanged += OnScrollViewChanged;
			_scrollViewer.SizeChanged += OnScrollViewSizeChanged;

			if (Element.Loop)
			{
				UpdateScrollBarVisibilityForLoop();
			}
			else
			{
				UpdateScrollBarVisibility();
			}
		}

		protected override ICollectionView GetCollectionView(CollectionViewSource collectionViewSource)
		{
			_loopableCollectionView?.CleanUp();
			_loopableCollectionView = new LoopableCollectionView(base.GetCollectionView(collectionViewSource));

			if (Element is CarouselView cv && cv.Loop)
			{
				_loopableCollectionView.IsLoopingEnabled = true;
			}

			return _loopableCollectionView;
		}

		protected override ListViewBase SelectListViewBase()
		{
			return CreateCarouselListLayout(CarouselItemsLayout.Orientation);
		}

		protected override CollectionViewSource CreateCollectionViewSource()
		{
			var collectionViewSource = TemplatedItemSourceFactory.Create(Element.ItemsSource, Element.ItemTemplate, Element,
				GetItemHeight(), GetItemWidth(), GetItemSpacing(), MauiContext);

			if (collectionViewSource is ObservableItemTemplateCollection observableItemsSource)
			{
				_collectionChanged ??= OnCollectionItemsSourceChanged;
				_proxy.Subscribe(observableItemsSource, _collectionChanged);
			}

			return new CollectionViewSource
			{
				Source = collectionViewSource,
				IsSourceGrouped = false
			};
		}

		protected override ItemsViewScrolledEventArgs ComputeVisibleIndexes(ItemsViewScrolledEventArgs args, ItemsLayoutOrientation orientation, bool advancing)
		{
			args = base.ComputeVisibleIndexes(args, orientation, advancing);

			if (ItemsView.Loop && ItemsView.ItemsSource is not null)
			{
				args.FirstVisibleItemIndex %= ItemCount;
				args.CenterItemIndex %= ItemCount;
				args.LastVisibleItemIndex %= ItemCount;
			}

			return args;
		}

		ListViewBase CreateCarouselListLayout(ItemsLayoutOrientation layoutOrientation)
		{
			UI.Xaml.Controls.ListView listView;

			if (layoutOrientation == ItemsLayoutOrientation.Horizontal)
			{
				listView = new FormsListView()
				{
					Style = (UI.Xaml.Style)WApp.Current.Resources["HorizontalCarouselListStyle"],
					ItemsPanel = (ItemsPanelTemplate)WApp.Current.Resources["HorizontalListItemsPanel"]
				};

				ScrollViewer.SetHorizontalScrollBarVisibility(listView, WScrollBarVisibility.Auto);
				ScrollViewer.SetVerticalScrollBarVisibility(listView, WScrollBarVisibility.Disabled);
			}
			else
			{
				listView = new FormsListView()
				{
					Style = (UI.Xaml.Style)WApp.Current.Resources["VerticalCarouselListStyle"]
				};

				ScrollViewer.SetHorizontalScrollBarVisibility(listView, WScrollBarVisibility.Disabled);
				ScrollViewer.SetVerticalScrollBarVisibility(listView, WScrollBarVisibility.Auto);
			}

			listView.Padding = WinUIHelpers.CreateThickness(ItemsView.PeekAreaInsets.Left, ItemsView.PeekAreaInsets.Top, ItemsView.PeekAreaInsets.Right, ItemsView.PeekAreaInsets.Bottom);

			return listView;
		}

		public static void MapCurrentItem(CarouselViewHandler handler, CarouselView carouselView)
		{
			handler.UpdateCurrentItem();
		}

		public static void MapPosition(CarouselViewHandler handler, CarouselView carouselView)
		{
			// If the initial position hasn't been set, we have a UpdateInitialPosition call on CarouselViewHandler
			// that will handle this so we want to skip this mapper call. We need to wait for the LIstView to be ready
			if (handler.InitialPositionSet)
			{
				handler.UpdatePosition();
			}

		}

		public static void MapIsBounceEnabled(CarouselViewHandler handler, CarouselView carouselView)
		{
			handler.UpdateIsBounceEnabled();
		}

		public static void MapIsSwipeEnabled(CarouselViewHandler handler, CarouselView carouselView)
		{
			handler.UpdateIsSwipeEnabled();
		}

		public static void MapPeekAreaInsets(CarouselViewHandler handler, CarouselView carouselView)
		{
			handler.UpdatePeekAreaInsets();
		}

		public static void MapLoop(CarouselViewHandler handler, CarouselView carouselView)
		{
			handler.UpdateLoop();
		}

		internal bool InitialPositionSet { get; private set; }


		void UpdateIsBounceEnabled()
		{
			if (_scrollViewer != null)
				_scrollViewer.IsScrollInertiaEnabled = ItemsView.IsBounceEnabled;
		}

		void UpdateIsSwipeEnabled()
		{
			ListViewBase.IsSwipeEnabled = ItemsView.IsSwipeEnabled;

			switch (CarouselItemsLayout.Orientation)
			{
				case ItemsLayoutOrientation.Horizontal:
					ScrollViewer.SetHorizontalScrollMode(ListViewBase, ItemsView.IsSwipeEnabled ? WScrollMode.Auto : WScrollMode.Disabled);
					ScrollViewer.SetHorizontalScrollBarVisibility(ListViewBase, ItemsView.IsSwipeEnabled ? WScrollBarVisibility.Auto : WScrollBarVisibility.Hidden);
					break;
				case ItemsLayoutOrientation.Vertical:
					ScrollViewer.SetVerticalScrollMode(ListViewBase, ItemsView.IsSwipeEnabled ? WScrollMode.Auto : WScrollMode.Disabled);
					ScrollViewer.SetVerticalScrollBarVisibility(ListViewBase, ItemsView.IsSwipeEnabled ? WScrollBarVisibility.Auto : WScrollBarVisibility.Hidden);
					break;
			}
		}

		void UpdatePeekAreaInsets()
		{
			ListViewBase.Padding = WinUIHelpers.CreateThickness(ItemsView.PeekAreaInsets.Left, ItemsView.PeekAreaInsets.Top, ItemsView.PeekAreaInsets.Right, ItemsView.PeekAreaInsets.Bottom);
			UpdateItemsSource();
		}

		void UpdateLoop()
		{
			UpdateScrollBarVisibilityForLoop();
			UpdateItemsSource();
		}

		double GetItemWidth()
		{
			var itemWidth = ListViewBase.ActualWidth;

			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
			{
				itemWidth = ListViewBase.ActualWidth - ItemsView.PeekAreaInsets.Left - ItemsView.PeekAreaInsets.Right;
			}

			return Math.Max(itemWidth, 0);
		}

		double GetItemHeight()
		{
			var itemHeight = ListViewBase.ActualHeight;

			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
			{
				itemHeight = ListViewBase.ActualHeight - ItemsView.PeekAreaInsets.Top - ItemsView.PeekAreaInsets.Bottom;
			}

			return Math.Max(itemHeight, 0);
		}

		Thickness? GetItemSpacing()
		{
			var itemSpacing = CarouselItemsLayout.ItemSpacing;

			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
				return new Thickness(itemSpacing, 0, 0, 0);

			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
				return new Thickness(0, itemSpacing, 0, 0);

			return new Thickness(0);
		}

		bool IsValidPosition(int position)
		{
			if (ItemCount == 0)
				return false;

			if (position < 0 || position >= ItemCount)
				return false;

			return true;
		}

		void SetCarouselViewPosition(int position)
		{
			if (ItemCount == 0)
			{
				return;
			}

			if (!IsValidPosition(position))
				return;

			var currentPosition = ItemsView.Position;

			if (currentPosition != position)
				ItemsView.Position = position;
		}

		void SetCarouselViewCurrentItem(int carouselPosition)
		{
			if (!IsValidPosition(carouselPosition))
				return;

			if (!(GetItem(carouselPosition) is ItemTemplateContext itemTemplateContext))
				throw new InvalidOperationException("Visible item not found");

			var item = itemTemplateContext.Item;
			ItemsView.CurrentItem = item;
		}

		int GetItemPositionInCarousel(object item)
		{
			for (int n = 0; n < ItemCount; n++)
			{
				if (GetItem(n) is ItemTemplateContext pair)
				{
					if (pair.Item == item)
					{
						return n;
					}
				}
			}

			return -1;
		}

		void UpdateInitialPosition()
		{
			if (ListViewBase == null)
			{
				return;
			}

			if (ListViewBase.Items.Count > 0)
			{
				if (Element.Loop)
				{
					var item = ItemsView.CurrentItem ?? ListViewBase.Items.FirstOrDefault();
					_loopableCollectionView.CenterMode = true;
					ListViewBase.ScrollIntoView(item);
					_loopableCollectionView.CenterMode = false;
				}

				if (ItemsView.CurrentItem != null)
					UpdateCurrentItem();
				else
					UpdatePosition();

				InitialPositionSet = true;
			}
		}

		void UpdateCurrentItem()
		{
			if (CollectionViewSource == null)
				return;

			var currentItemPosition = GetItemPositionInCarousel(ItemsView.CurrentItem);

			if (currentItemPosition < 0 || currentItemPosition >= ItemCount)
				return;

			if (ItemsView.Position != currentItemPosition)
			{
				ItemsView.ScrollTo(currentItemPosition, position: ScrollToPosition.Center, animate: ItemsView.AnimateCurrentItemChanges);
			}
		}

		void UpdatePosition()
		{
			if (CollectionViewSource == null)
				return;

			var carouselPosition = ItemsView.Position;

			if (carouselPosition < 0 || carouselPosition >= ItemCount)
				return;

			if (!ItemsView.IsDragging && !ItemsView.IsScrolling)
			{
				ItemsView.ScrollTo(carouselPosition, position: ScrollToPosition.Center, animate: ItemsView.AnimateCurrentItemChanges);
			}

			SetCarouselViewCurrentItem(carouselPosition);
		}

		WSnapPointsType GetWindowsSnapPointsType(SnapPointsType snapPointsType)
		{
			switch (snapPointsType)
			{
				case SnapPointsType.Mandatory:
					return WSnapPointsType.Mandatory;
				case SnapPointsType.MandatorySingle:
					return WSnapPointsType.MandatorySingle;
				case SnapPointsType.None:
					return WSnapPointsType.None;
			}

			return WSnapPointsType.None;
		}

		WSnapPointsAlignment GetWindowsSnapPointsAlignment(SnapPointsAlignment snapPointsAlignment)
		{
			switch (snapPointsAlignment)
			{
				case SnapPointsAlignment.Center:
					return WSnapPointsAlignment.Center;
				case SnapPointsAlignment.End:
					return WSnapPointsAlignment.Far;
				case SnapPointsAlignment.Start:
					return WSnapPointsAlignment.Near;
			}

			return WSnapPointsAlignment.Center;
		}

		void UpdateSnapPointsType()
		{
			if (_scrollViewer == null || CarouselItemsLayout == null)
				return;

			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
				_scrollViewer.HorizontalSnapPointsType = GetWindowsSnapPointsType(CarouselItemsLayout.SnapPointsType);

			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
				_scrollViewer.VerticalSnapPointsType = GetWindowsSnapPointsType(CarouselItemsLayout.SnapPointsType);
		}

		void UpdateSnapPointsAlignment()
		{
			if (_scrollViewer == null || CarouselItemsLayout == null)
				return;

			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
				_scrollViewer.HorizontalSnapPointsAlignment = GetWindowsSnapPointsAlignment(CarouselItemsLayout.SnapPointsAlignment);

			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
				_scrollViewer.VerticalSnapPointsAlignment = GetWindowsSnapPointsAlignment(CarouselItemsLayout.SnapPointsAlignment);
		}

		void UpdateScrollBarVisibilityForLoop()
		{
			if (_scrollViewer == null)
			{
				return;
			}

			if (Element.Loop)
			{
				// Track the current scrollbar settings
				_horizontalScrollBarVisibilityWithoutLoop = _scrollViewer.HorizontalScrollBarVisibility;
				_verticalScrollBarVisibilityWithoutLoop = _scrollViewer.VerticalScrollBarVisibility;

				// Disable the scroll bars, they don't make sense when looping
				_scrollViewer.HorizontalScrollBarVisibility = WScrollBarVisibility.Hidden;
				_scrollViewer.VerticalScrollBarVisibility = WScrollBarVisibility.Hidden;
			}
			else
			{
				// Restore the previous visibility (if any was recorded)
				if (_horizontalScrollBarVisibilityWithoutLoop.HasValue)
				{
					_scrollViewer.HorizontalScrollBarVisibility = _horizontalScrollBarVisibilityWithoutLoop.Value;
				}

				if (_verticalScrollBarVisibilityWithoutLoop.HasValue)
				{
					_scrollViewer.VerticalScrollBarVisibility = _verticalScrollBarVisibilityWithoutLoop.Value;
				}
			}
		}

		void CarouselScrolled(object sender, ItemsViewScrolledEventArgs e)
		{
			var position = e.CenterItemIndex;

			if (position == -1)
			{
				return;
			}

			if (position == Element.Position)
			{
				return;
			}

			SetCarouselViewPosition(position);
		}

		void OnScrollViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
		{
			if (ItemsView.ItemsSource is null)
				return;

			ItemsView.SetIsDragging(true);
			ItemsView.IsScrolling = true;
		}

		void OnScrollViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
		{
			ItemsView.SetIsDragging(e.IsIntermediate);
			ItemsView.IsScrolling = e.IsIntermediate;
		}

		void OnCollectionItemsSourceChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			var carouselPosition = ItemsView.Position;
			var currentItemPosition = GetItemPositionInCarousel(ItemsView.CurrentItem);
			var count = (sender as IList).Count;

			bool removingCurrentElement = currentItemPosition == -1;
			bool removingLastElement = e.OldStartingIndex == count;
			bool removingFirstElement = e.OldStartingIndex == 0;
			bool removingCurrentElementButNotFirst = removingCurrentElement && removingLastElement && ItemsView.Position > 0;

			if (removingCurrentElementButNotFirst)
			{
				carouselPosition = ItemsView.Position - 1;

			}
			else if (removingFirstElement && !removingCurrentElement)
			{
				carouselPosition = currentItemPosition;
			}

			// If we are adding a new item make sure to maintain the CurrentItemPosition
			else if (e.Action == NotifyCollectionChangedAction.Add
				&& currentItemPosition != -1)
			{
				carouselPosition = currentItemPosition;
			}

			SetCarouselViewCurrentItem(carouselPosition);
			SetCarouselViewPosition(carouselPosition);
		}

		void OnListViewSizeChanged(object sender, SizeChangedEventArgs e) => Resize(e.NewSize);

		void OnScrollViewSizeChanged(object sender, SizeChangedEventArgs e)
		{
			//If there's a scroll viewer, it's enough to resize based on its size changed event, so we can avoid two event handlers doing the same
			ListViewBase.SizeChanged -= OnListViewSizeChanged;
			Resize(e.NewSize);
		}

		void Resize(Size newSize)
		{
			if (newSize != _currentSize && newSize.Width > 0 && newSize.Height > 0)
			{
				_currentSize = newSize;

				if (_isCarouselViewReady)
					InvalidateItemSize();
				else
					InitialSetup();

				_isCarouselViewReady = true;
			}
		}

		void InitialSetup()
		{
			UpdateItemsSource();
			UpdateSnapPointsType();
			UpdateSnapPointsAlignment();
			UpdateInitialPosition();
		}

		void InvalidateItemSize()
		{
			var itemHeight = GetItemHeight();
			var itemWidth = GetItemWidth();

			foreach (var item in ListViewBase.GetChildren<ItemContentControl>())
			{
				item.ItemHeight = itemHeight;
				item.ItemWidth = itemWidth;
			}
			ListViewBase.InvalidateMeasure();
		}
	}
}
