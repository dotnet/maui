#nullable disable
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Windows.Foundation;
using WApp = Microsoft.UI.Xaml.Application;
using WDataTemplate = Microsoft.UI.Xaml.DataTemplate;
using WScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility;
using WScrollMode = Microsoft.UI.Xaml.Controls.ScrollMode;
using WSetter = Microsoft.UI.Xaml.Setter;
using WSnapPointsAlignment = Microsoft.UI.Xaml.Controls.Primitives.SnapPointsAlignment;
using WSnapPointsType = Microsoft.UI.Xaml.Controls.SnapPointsType;
using WStyle = Microsoft.UI.Xaml.Style;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class CarouselViewHandler : ItemsViewHandler<CarouselView>
	{
		const double CenteringTolerance = 1;

		LoopableCollectionView _loopableCollectionView;
		ScrollViewer _scrollViewer;
		WScrollBarVisibility? _horizontalScrollBarVisibilityWithoutLoop;
		WScrollBarVisibility? _verticalScrollBarVisibilityWithoutLoop;
		Size _currentSize;
		bool _isCarouselViewReady;
		bool _isInternalPositionUpdate;
		int _positionUpdateFromScroll = -1;
		bool _hasCurrentItemUpdateFromScroll;
		object _currentItemUpdateFromScroll;
		bool _hasCurrentItemUpdateFromCollection;
		object _currentItemUpdateFromCollection;
		object _collectionCurrentItemOverride;
		IList _collectionItemsSource;
		bool _isRecentering;
		double _recenteringHorizontalOffset;
		double _recenteringVerticalOffset;
		Point? _failedRecenteringOffset;
		Point? _failedRecenteringTarget;
		int _recenteringAttemptCount;
		bool _isScrollingForward;
		int _centerItemIndexFromScroll = -1;
		int _centerRequestVersion;
		int _gotoPosition = -1;
		bool _isCollectionChanged;
		bool _isCollectionChangeScrollPending;
		int _collectionChangeVersion;
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

			ResetScrollState();
			base.DisconnectHandler(platformView);
		}

		void ResetScrollState()
		{
			_positionUpdateFromScroll = -1;
			_hasCurrentItemUpdateFromScroll = false;
			_currentItemUpdateFromScroll = null;
			_hasCurrentItemUpdateFromCollection = false;
			_currentItemUpdateFromCollection = null;
			_collectionCurrentItemOverride = null;
			_collectionItemsSource = null;
			ResetRecenteringState();
			_isScrollingForward = false;
			_centerItemIndexFromScroll = -1;
			_centerRequestVersion++;
			_gotoPosition = -1;
			_isCollectionChanged = false;
			_isCollectionChangeScrollPending = false;
			_collectionChangeVersion++;
		}

		void ResetRecenteringState()
		{
			_isRecentering = false;
			_recenteringHorizontalOffset = 0;
			_recenteringVerticalOffset = 0;
			_failedRecenteringOffset = null;
			_failedRecenteringTarget = null;
			_recenteringAttemptCount = 0;
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

			if (ItemsView.Loop && ItemsView.ItemsSource is not null && ItemCount > 0)
			{
				args.FirstVisibleItemIndex %= ItemCount;
				args.CenterItemIndex %= ItemCount;
				args.LastVisibleItemIndex %= ItemCount;
			}

			return args;
		}

		protected override void UpdateEmptyViewVisibility()
		{
			if (ItemsView?.Loop == true)
			{
				bool isEmpty = (CollectionViewSource?.View?.Count ?? 0) == 0;
				var targetTemplate = isEmpty ? null : CarouselItemsViewTemplate;
				if (ListViewBase.ItemTemplate != targetTemplate)
				{
					ListViewBase.ItemTemplate = targetTemplate;
				}
			}

			base.UpdateEmptyViewVisibility();
		}

		ListViewBase CreateCarouselListLayout(ItemsLayoutOrientation layoutOrientation)
		{
			UI.Xaml.Controls.ListView listView;

			if (layoutOrientation == ItemsLayoutOrientation.Horizontal)
			{
				listView = new FormsListView()
				{
					Style = (UI.Xaml.Style)WApp.Current.Resources["HorizontalCarouselListStyle"],
					ItemsPanel = (ItemsPanelTemplate)WApp.Current.Resources["HorizontalListItemsPanel"],
					ItemContainerStyle = GetItemContainerStyle(true)
				};

				ScrollViewer.SetHorizontalScrollBarVisibility(listView, WScrollBarVisibility.Auto);
				ScrollViewer.SetVerticalScrollBarVisibility(listView, WScrollBarVisibility.Disabled);
			}
			else
			{
				listView = new FormsListView()
				{
					Style = (UI.Xaml.Style)WApp.Current.Resources["VerticalCarouselListStyle"],
					ItemContainerStyle = GetItemContainerStyle(false)
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

		protected override async Task ScrollTo(ScrollToRequestEventArgs args)
		{
			_centerRequestVersion++;

			if (args.IsAnimated && args.Mode == ScrollToMode.Position)
			{
				_gotoPosition = args.Index;

				// Commit position before animation so PreviousPosition/PreviousItem are correct immediately.
				SetCarouselViewPosition(_gotoPosition);
			}

			try
			{
				await base.ScrollTo(args);
			}
			finally
			{
				// Conditional reset guards against a concurrent ScrollTo replacing the target.
				if (_gotoPosition == args.Index)
					_gotoPosition = -1;
			}
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
				itemWidth = ListViewBase.ActualWidth - ItemsView.PeekAreaInsets.Left - ItemsView.PeekAreaInsets.Right - ItemsView.ItemsLayout.ItemSpacing;
			}

			return Math.Max(itemWidth, 0);
		}

		double GetItemHeight()
		{
			var itemHeight = ListViewBase.ActualHeight;

			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
			{
				itemHeight = ListViewBase.ActualHeight - ItemsView.PeekAreaInsets.Top - ItemsView.PeekAreaInsets.Bottom - ItemsView.ItemsLayout.ItemSpacing;
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
			var itemCount = GetItemsSourceCount();
			if (itemCount == 0)
				return false;

			if (position < 0 || position >= itemCount)
				return false;

			return true;
		}

		int GetItemsSourceCount()
		{
			if (_collectionItemsSource is not null)
				return _collectionItemsSource.Count;

			return ItemCount;
		}

		void SetCarouselViewPosition(int position)
		{
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

			if (!(GetItemAtPosition(carouselPosition) is ItemTemplateContext itemTemplateContext))
				throw new InvalidOperationException("Visible item not found");

			var item = itemTemplateContext.Item;
			var isPositionUpdateFromScroll = _positionUpdateFromScroll == carouselPosition;
			var isCurrentItemUpdateFromCollection = _isInternalPositionUpdate;
			var hadCurrentItemUpdateFromCollection = _hasCurrentItemUpdateFromCollection;
			var previousCurrentItemUpdateFromCollection = _currentItemUpdateFromCollection;
			if (isPositionUpdateFromScroll)
			{
				_hasCurrentItemUpdateFromScroll = true;
				_currentItemUpdateFromScroll = item;
			}

			if (isCurrentItemUpdateFromCollection)
			{
				_hasCurrentItemUpdateFromCollection = true;
				_currentItemUpdateFromCollection = item;
			}

			try
			{
				ItemsView.CurrentItem = item;
			}
			finally
			{
				if (isPositionUpdateFromScroll)
				{
					_hasCurrentItemUpdateFromScroll = false;
					_currentItemUpdateFromScroll = null;
				}

				if (isCurrentItemUpdateFromCollection)
				{
					_hasCurrentItemUpdateFromCollection = hadCurrentItemUpdateFromCollection;
					_currentItemUpdateFromCollection = previousCurrentItemUpdateFromCollection;
				}
			}
		}

		int GetItemPositionInCarousel(object item)
		{
			var itemCount = _collectionItemsSource?.Count ?? ItemCount;
			for (int n = 0; n < itemCount; n++)
			{
				if (GetItemAtPosition(n) is ItemTemplateContext pair)
				{
					if (pair.Item == item)
					{
						return n;
					}
				}
			}

			return -1;
		}

		object GetItemAtPosition(int position) =>
			_collectionItemsSource is null ? GetItem(position) : _collectionItemsSource[position];

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

			if (_collectionCurrentItemOverride is not null)
			{
				if (ReferenceEquals(_collectionCurrentItemOverride, ItemsView.CurrentItem))
					return;

				if (_hasCurrentItemUpdateFromScroll)
					return;

				_collectionCurrentItemOverride = null;
			}

			if (_hasCurrentItemUpdateFromScroll
				&& _positionUpdateFromScroll == ItemsView.Position
				&& ReferenceEquals(_currentItemUpdateFromScroll, ItemsView.CurrentItem))
			{
				return;
			}

			if (_hasCurrentItemUpdateFromCollection
				&& ReferenceEquals(_currentItemUpdateFromCollection, ItemsView.CurrentItem))
			{
				return;
			}

			if (_isInternalPositionUpdate)
			{
				return;
			}

			var currentItemPosition = GetItemPositionInCarousel(ItemsView.CurrentItem);

			if (!IsValidPosition(currentItemPosition))
			{
				return;
			}

			if (_gotoPosition != -1)
			{
				return;
			}

			// Disable animation during collection changes to prevent cascading scroll events
			var animate = ItemsView.AnimateCurrentItemChanges && !_isInternalPositionUpdate;
			ItemsView.ScrollTo(currentItemPosition, position: ScrollToPosition.Center, animate: animate);
		}

		void UpdatePosition()
		{
			if (CollectionViewSource == null)
				return;

			if (_isInternalPositionUpdate)
				return;

			var carouselPosition = ItemsView.Position;

			if (carouselPosition < 0 || carouselPosition >= ItemCount)
				return;

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
			// Ignore ViewChanged events fired before the initial position is established.
			if (!InitialPositionSet)
			{
				return;
			}

			var itemsLayout = CarouselItemsLayout;
			if (itemsLayout is not null)
			{
				var scrollDelta = itemsLayout.Orientation == ItemsLayoutOrientation.Horizontal
					? e.HorizontalDelta
					: e.VerticalDelta;
				if (!_isRecentering && scrollDelta != 0)
					_isScrollingForward = scrollDelta > 0;
			}

			var position = e.CenterItemIndex;
			if (_isCollectionChanged && ItemsView.ItemsUpdatingScrollMode == ItemsUpdatingScrollMode.KeepScrollOffset)
			{
				position = ItemsView.Position;
				_isCollectionChanged = false;
				_isCollectionChangeScrollPending = true;
			}

			// Suppress intermediate scroll events during a programmatic animated scroll.
			if (_gotoPosition != -1)
			{
				return;
			}

			// Centered mandatory layouts are reconciled geometrically after the native scroll settles.
			if (ShouldCenterCarouselItem())
			{
				_centerItemIndexFromScroll = position;
				return;
			}

			SetPositionFromScroll(position);
		}

		internal void SetPositionFromScroll(int physicalPosition)
		{
			if (physicalPosition == -1)
			{
				return;
			}

			var position = physicalPosition;
			if (ItemsView.Loop && _loopableCollectionView?.RealCount > 0)
				position %= _loopableCollectionView.RealCount;

			if (position == Element.Position)
			{
				return;
			}

			_positionUpdateFromScroll = position;
			try
			{
				SetCarouselViewPosition(position);
			}
			finally
			{
				_positionUpdateFromScroll = -1;
			}
		}

		void OnScrollViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
		{
			if (ItemsView.ItemsSource is null)
				return;

			_centerRequestVersion++;
			if (_isRecentering)
				return;

			_recenteringAttemptCount = 0;
			ItemsView.SetIsDragging(true);
			ItemsView.IsScrolling = true;
		}

		void OnScrollViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
		{
			if (!_isRecentering)
			{
				ItemsView.SetIsDragging(e.IsIntermediate);
				ItemsView.IsScrolling = e.IsIntermediate;
			}

			if (e.IsIntermediate)
				return;

			var centerItemIndexFromScroll = _centerItemIndexFromScroll;
			_centerItemIndexFromScroll = -1;

			if (_isCollectionChangeScrollPending)
			{
				_isCollectionChangeScrollPending = false;
				ResetRecenteringState();
				return;
			}

			if (_isRecentering)
			{
				_isRecentering = false;
				var currentOffset = new Point(_scrollViewer.HorizontalOffset, _scrollViewer.VerticalOffset);
				var recenteringTarget = new Point(_recenteringHorizontalOffset, _recenteringVerticalOffset);
				if (AreClose(currentOffset, recenteringTarget))
				{
					_failedRecenteringOffset = null;
					_failedRecenteringTarget = null;
					_recenteringAttemptCount = 0;
					return;
				}

				if (_failedRecenteringOffset is Point failedOffset
					&& _failedRecenteringTarget is Point failedTarget
					&& AreClose(currentOffset, failedOffset)
					&& AreClose(recenteringTarget, failedTarget))
				{
					return;
				}

				_failedRecenteringOffset = currentOffset;
				_failedRecenteringTarget = recenteringTarget;
			}
			else
			{
				_failedRecenteringOffset = null;
				_failedRecenteringTarget = null;
			}

			QueueCenterCarouselItem(centerItemIndexFromScroll);
		}

		static bool AreClose(Point first, Point second) =>
			Math.Abs(first.X - second.X) <= CenteringTolerance
				&& Math.Abs(first.Y - second.Y) <= CenteringTolerance;

		void QueueCenterCarouselItem(int centerItemIndexFromScroll)
		{
			if (!ShouldCenterCarouselItem())
				return;

			var dispatcherQueue = ListViewBase?.DispatcherQueue;
			if (dispatcherQueue is null)
			{
				CenterCarouselItem(centerItemIndexFromScroll);
				return;
			}

			var centerRequestVersion = ++_centerRequestVersion;
			if (!dispatcherQueue.TryEnqueue(
				Microsoft.UI.Dispatching.DispatcherQueuePriority.Low,
				() =>
				{
					if (centerRequestVersion == _centerRequestVersion)
						CenterCarouselItem(centerItemIndexFromScroll);
				}))
			{
				CenterCarouselItem(centerItemIndexFromScroll);
			}
		}

		internal void CenterCarouselItem(int centerItemIndexFromScroll)
		{
			if (!InitialPositionSet
				|| _gotoPosition != -1
				|| !ShouldCenterCarouselItem())
			{
				return;
			}

			if (!TryGetClosestVisibleItem(out var closestVisibleIndex, out var horizontalOffset, out var verticalOffset, out var distance))
			{
				SetPositionFromScroll(centerItemIndexFromScroll);
				return;
			}

			var itemsView = ((IElementHandler)this).VirtualView as CarouselView;
			if (itemsView is null)
				return;

			var closestPosition = closestVisibleIndex;
			if (itemsView.Loop && _loopableCollectionView?.RealCount > 0)
				closestPosition %= _loopableCollectionView.RealCount;

			SetPositionFromScroll(closestVisibleIndex);

			// A nested application update may have selected another item while Position changed.
			if (!ReferenceEquals(((IElementHandler)this).VirtualView, itemsView)
				|| _gotoPosition != -1
				|| itemsView.Position != closestPosition)
				return;

			if (distance > CenteringTolerance && _recenteringAttemptCount < 2)
			{
				_recenteringAttemptCount++;
				_isRecentering = true;
				_recenteringHorizontalOffset = horizontalOffset;
				_recenteringVerticalOffset = verticalOffset;
				if (!_scrollViewer.ChangeView(horizontalOffset, verticalOffset, null, true))
				{
					_isRecentering = false;
					_recenteringAttemptCount = 0;
				}
			}
		}

		bool ShouldCenterCarouselItem()
		{
			var snapPointsType = CarouselItemsLayout?.SnapPointsType;
			return (snapPointsType == SnapPointsType.Mandatory || snapPointsType == SnapPointsType.MandatorySingle)
				&& CarouselItemsLayout.SnapPointsAlignment == SnapPointsAlignment.Center;
		}

		bool TryGetClosestVisibleItem(out int closestVisibleIndex, out double horizontalOffset, out double verticalOffset, out double distance)
		{
			closestVisibleIndex = -1;
			horizontalOffset = 0;
			verticalOffset = 0;
			distance = double.MaxValue;

			if (_scrollViewer?.Content is not UIElement content
				|| ListViewBase?.ItemsPanelRoot is not ItemsStackPanel itemsPanel)
			{
				return false;
			}

			var firstVisibleIndex = itemsPanel.FirstVisibleIndex;
			var lastVisibleIndex = itemsPanel.LastVisibleIndex;
			if (firstVisibleIndex < 0 || lastVisibleIndex < firstVisibleIndex)
				return false;

			var currentHorizontalOffset = _scrollViewer.HorizontalOffset;
			var currentVerticalOffset = _scrollViewer.VerticalOffset;
			var orientation = CarouselItemsLayout.Orientation;
			horizontalOffset = currentHorizontalOffset;
			verticalOffset = currentVerticalOffset;
			var closestDistance = double.MaxValue;

			for (int index = firstVisibleIndex; index <= lastVisibleIndex; index++)
			{
				if (ListViewBase.ContainerFromIndex(index) is not FrameworkElement container
					|| (orientation == ItemsLayoutOrientation.Horizontal
						? container.ActualWidth <= 0
						: container.ActualHeight <= 0))
					continue;

				var position = container.TransformToVisual(content).TransformPoint(new Point());
				var candidateHorizontalOffset = currentHorizontalOffset;
				var candidateVerticalOffset = currentVerticalOffset;
				if (orientation == ItemsLayoutOrientation.Horizontal)
					candidateHorizontalOffset = position.X - ((_scrollViewer.ViewportWidth - container.ActualWidth) / 2);
				else
					candidateVerticalOffset = position.Y - ((_scrollViewer.ViewportHeight - container.ActualHeight) / 2);

				var candidateDistance = Math.Abs(currentHorizontalOffset - candidateHorizontalOffset)
					+ Math.Abs(currentVerticalOffset - candidateVerticalOffset);
				if (TrySelectCandidate(candidateDistance, ref closestDistance, _isScrollingForward))
				{
					closestVisibleIndex = index;
					horizontalOffset = Math.Clamp(candidateHorizontalOffset, 0, _scrollViewer.ScrollableWidth);
					verticalOffset = Math.Clamp(candidateVerticalOffset, 0, _scrollViewer.ScrollableHeight);
					distance = Math.Abs(currentHorizontalOffset - horizontalOffset)
						+ Math.Abs(currentVerticalOffset - verticalOffset);
				}
			}

			return closestVisibleIndex != -1;
		}

		internal static bool TrySelectCandidate(double candidateDistance, ref double closestDistance, bool isScrollingForward)
		{
			if (candidateDistance >= closestDistance - CenteringTolerance
				&& (!isScrollingForward || Math.Abs(candidateDistance - closestDistance) > CenteringTolerance))
			{
				return false;
			}

			closestDistance = Math.Min(closestDistance, candidateDistance);
			return true;
		}

		void OnCollectionItemsSourceChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_centerRequestVersion++;

			// Set flag to disable animation during collection changes
			var wasInternalPositionUpdate = _isInternalPositionUpdate;
			var previousCollectionItemsSource = _collectionItemsSource;
			_isInternalPositionUpdate = true;
			_collectionItemsSource = (IList)sender;
			var collectionChangeVersion = ++_collectionChangeVersion;

			try
			{
				var carouselPosition = ItemsView.Position;
				var currentItemPosition = GetItemPositionInCarousel(ItemsView.CurrentItem);
				var count = _collectionItemsSource.Count;
				bool currentItemOverridden = false;

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
					_isCollectionChanged = true;
				}

				if (e.Action == NotifyCollectionChangedAction.Remove)
				{
					_isCollectionChanged = true;
				}

				if (ItemsView.ItemsUpdatingScrollMode == ItemsUpdatingScrollMode.KeepLastItemInView)
				{
					carouselPosition = count == 0 ? 0 : count - 1;
				}
				else if (ItemsView.ItemsUpdatingScrollMode == ItemsUpdatingScrollMode.KeepItemsInView)
				{
					carouselPosition = 0;
				}

				object collectionCurrentItem = null;
				if (IsValidPosition(carouselPosition)
					&& GetItemAtPosition(carouselPosition) is ItemTemplateContext itemTemplateContext)
				{
					collectionCurrentItem = itemTemplateContext.Item;
				}

				SetCarouselViewCurrentItem(carouselPosition);
				if (!ReferenceEquals(collectionCurrentItem, ItemsView.CurrentItem))
				{
					var overriddenCurrentItemPosition = GetItemPositionInCarousel(ItemsView.CurrentItem);
					if (overriddenCurrentItemPosition != -1)
					{
						carouselPosition = overriddenCurrentItemPosition;
						currentItemOverridden = true;
					}
				}

				var currentItemBeforePositionUpdate = ItemsView.CurrentItem;
				SetCarouselViewPosition(carouselPosition);
				if (!ReferenceEquals(currentItemBeforePositionUpdate, ItemsView.CurrentItem))
				{
					var overriddenCurrentItemPosition = GetItemPositionInCarousel(ItemsView.CurrentItem);
					if (overriddenCurrentItemPosition != -1)
					{
						carouselPosition = overriddenCurrentItemPosition;
						currentItemOverridden = true;
						SetCarouselViewPosition(carouselPosition);
					}
				}

				if (currentItemOverridden)
				{
					_collectionCurrentItemOverride = ItemsView.CurrentItem;
					QueueCollectionCurrentItemOverrideScroll(
						_collectionCurrentItemOverride,
						carouselPosition,
						collectionChangeVersion);
				}
				else
				{
					_collectionCurrentItemOverride = null;
				}
			}
			finally
			{
				if (_isCollectionChanged)
					QueueCollectionChangeReset(collectionChangeVersion);

				// Reset flag after collection operations complete
				_collectionItemsSource = previousCollectionItemsSource;
				_isInternalPositionUpdate = wasInternalPositionUpdate;
			}
		}

		void QueueCollectionCurrentItemOverrideScroll(
			object currentItem,
			int position,
			int collectionChangeVersion,
			int attemptsRemaining = 2)
		{
			var dispatcherQueue = ListViewBase?.DispatcherQueue;
			if (dispatcherQueue is null
				|| !dispatcherQueue.TryEnqueue(
					Microsoft.UI.Dispatching.DispatcherQueuePriority.Low,
					() => CompleteCollectionCurrentItemOverrideScroll(
						currentItem,
						position,
						collectionChangeVersion,
						attemptsRemaining)))
			{
				CompleteCollectionCurrentItemOverrideScroll(
					currentItem,
					position,
					collectionChangeVersion,
					attemptsRemaining: 0);
			}
		}

		void CompleteCollectionCurrentItemOverrideScroll(
			object currentItem,
			int position,
			int collectionChangeVersion,
			int attemptsRemaining)
		{
			if (collectionChangeVersion != _collectionChangeVersion
				|| !ReferenceEquals(_collectionCurrentItemOverride, currentItem))
			{
				return;
			}

			var itemsView = ((IElementHandler)this).VirtualView as CarouselView;
			if (itemsView is null
				|| !ReferenceEquals(itemsView.CurrentItem, currentItem)
				|| itemsView.Position != position)
			{
				_collectionCurrentItemOverride = null;
				return;
			}

			if (position < 0
				|| position >= ItemCount
				|| GetItem(position) is not ItemTemplateContext itemTemplateContext
				|| !ReferenceEquals(itemTemplateContext.Item, currentItem))
			{
				if (attemptsRemaining > 0)
				{
					QueueCollectionCurrentItemOverrideScroll(
						currentItem,
						position,
						collectionChangeVersion,
						attemptsRemaining - 1);
				}
				else
				{
					_collectionCurrentItemOverride = null;
				}

				return;
			}

			itemsView.ScrollTo(position, position: ScrollToPosition.Center, animate: false);
		}

		void QueueCollectionChangeReset(int collectionChangeVersion)
		{
			var dispatcherQueue = ListViewBase?.DispatcherQueue;
			if (dispatcherQueue is null
				|| !dispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
				{
					if (collectionChangeVersion == _collectionChangeVersion)
						_isCollectionChanged = false;
				}))
			{
				_isCollectionChanged = false;
			}
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
		}

		WStyle GetItemContainerStyle(bool isHorizontalLayout)
		{
			var h = CarouselItemsLayout?.ItemSpacing > 0 ? (CarouselItemsLayout.ItemSpacing) / 2 : 0;
			var padding = isHorizontalLayout ? WinUIHelpers.CreateThickness(h, 0, h, 0) : WinUIHelpers.CreateThickness(0, h, 0, h);

			var style = new WStyle(typeof(ListViewItem));
			style.Setters.Add(new WSetter(Control.PaddingProperty, padding));
			return style;
		}
	}
}
