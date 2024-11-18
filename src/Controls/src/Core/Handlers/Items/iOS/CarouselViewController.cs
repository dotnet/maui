#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Devices;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class CarouselViewController : ItemsViewController<CarouselView>
	{
		[Obsolete("Use ItemsView property instead")]
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Unused")]
		protected readonly CarouselView Carousel;

		CarouselViewLoopManager _carouselViewLoopManager;
		bool _isCenteringItem;

		// We need to keep track of the old views to update the visual states
		// if this is null we are not attached to the window
		List<View> _oldViews;
		int _gotoPosition = -1;
		CGSize _size;
		ILoopItemsViewSource LoopItemsSource => ItemsSource as ILoopItemsViewSource;
		bool _isDragging;

		bool _isRotating;

		public CarouselViewController(CarouselView itemsView, ItemsViewLayout layout) : base(itemsView, layout)
		{
			CollectionView.AllowsSelection = false;
			CollectionView.AllowsMultipleSelection = false;
		}

		private protected override NSIndexPath GetAdjustedIndexPathForItemSource(NSIndexPath indexPath)
		{
			return NSIndexPath.FromItemSection(GetIndexFromIndexPath(indexPath), 0);
		}

		public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
		{
			UICollectionViewCell cell;

			if (ItemsView?.Loop == true && _carouselViewLoopManager != null)
			{
				var cellAndCorrectedIndex = _carouselViewLoopManager.GetCellAndCorrectIndex(collectionView, indexPath, DetermineCellReuseId(indexPath));
				cell = cellAndCorrectedIndex.cell;
				var correctedIndexPath = NSIndexPath.FromRowSection(cellAndCorrectedIndex.correctedIndex, 0);

				if (cell is DefaultCell defaultCell)
				{
					UpdateDefaultCell(defaultCell, correctedIndexPath);
				}

				if (cell is TemplatedCell templatedCell)
				{
					UpdateTemplatedCell(templatedCell, correctedIndexPath);
				}
			}
			else
			{
				cell = base.GetCell(collectionView, indexPath);
			}

			var element = (cell as TemplatedCell)?.PlatformHandler?.VirtualView as VisualElement;

			if (element != null)
			{
				VisualStateManager.GoToState(element, CarouselView.DefaultItemVisualState);
			}

			return cell;
		}

		public override nint GetItemsCount(UICollectionView collectionView, nint section) => LoopItemsSource.LoopCount;

		void InitializeCarouselViewLoopManager()
		{
			if (_carouselViewLoopManager is null)
			{
				_carouselViewLoopManager = new CarouselViewLoopManager(Layout as UICollectionViewFlowLayout);
				_carouselViewLoopManager.SetItemsSource(LoopItemsSource);
			}
		}

		public override void ViewDidLoad()
		{
			InitializeCarouselViewLoopManager();
			base.ViewDidLoad();
		}

		void OnDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
		{
			_isRotating = true;
		}

		public override void ViewWillLayoutSubviews()
		{
			base.ViewWillLayoutSubviews();
			UpdateVisualStates();
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			if (ItemsView?.Loop == true && _carouselViewLoopManager != null)
			{
				_isCenteringItem = true;
				_carouselViewLoopManager.CenterIfNeeded(CollectionView, IsHorizontal);
				_isCenteringItem = false;
			}

			if (CollectionView.Bounds.Size != _size)
			{
				_size = CollectionView.Bounds.Size;
				BoundsSizeChanged();
			}
			else
			{
				UpdateInitialPosition();
			}
			_isRotating = false;
		}

		void BoundsSizeChanged()
		{
			//if the size changed center the item
			if (ItemsView is CarouselView carousel)
			{
				carousel.ScrollTo(carousel.Position, position: Microsoft.Maui.Controls.ScrollToPosition.Center, animate: false);
			}
		}

		public override void DraggingStarted(UIScrollView scrollView)
		{
			_isDragging = true;
			ItemsView?.SetIsDragging(true);
		}

		public override void DraggingEnded(UIScrollView scrollView, bool willDecelerate)
		{
			ItemsView?.SetIsDragging(false);
			_isDragging = false;
		}

		public override void UpdateItemsSource()
		{
			UnsubscribeCollectionItemsSourceChanged(ItemsSource);
			base.UpdateItemsSource();
			//we don't need to Subscribe because base calls CreateItemsViewSource
			_carouselViewLoopManager?.SetItemsSource(LoopItemsSource);

			if (InitialPositionSet && ItemsView is CarouselView carousel)
			{
				carousel.SetValueFromRenderer(CarouselView.CurrentItemProperty, null);
				carousel.SetValueFromRenderer(CarouselView.PositionProperty, 0);
			}
		}

		protected override bool IsHorizontal => ItemsView?.ItemsLayout?.Orientation == ItemsLayoutOrientation.Horizontal;

		protected override UICollectionViewDelegateFlowLayout CreateDelegator() => new CarouselViewDelegator(ItemsViewLayout, this);

		[Obsolete("Use DetermineCellReuseId(NSIndexPath indexPath) instead.")]
		protected override string DetermineCellReuseId()
		{
			if (ItemsView?.ItemTemplate != null)
			{
				return CarouselTemplatedCell.ReuseId;
			}

			return base.DetermineCellReuseId();
		}

		protected override string DetermineCellReuseId(NSIndexPath indexPath)
		{
			var itemIndex = GetIndexFromIndexPath(indexPath);
			return base.DetermineCellReuseId(NSIndexPath.FromItemSection(itemIndex, 0));
		}

		protected override void RegisterViewTypes()
		{
			CollectionView.RegisterClassForCell(typeof(CarouselTemplatedCell), CarouselTemplatedCell.ReuseId);
			base.RegisterViewTypes();
		}

		protected override IItemsViewSource CreateItemsViewSource()
		{
			var itemsSource = ItemsSourceFactory.CreateForCarouselView(ItemsView.ItemsSource, this, ItemsView.Loop);
			_carouselViewLoopManager?.SetItemsSource(itemsSource);
			SubscribeCollectionItemsSourceChanged(itemsSource);
			return itemsSource;
		}

		protected override void CacheCellAttributes(NSIndexPath indexPath, CGSize size)
		{
			var itemIndex = GetIndexFromIndexPath(indexPath);
			base.CacheCellAttributes(NSIndexPath.FromItemSection(itemIndex, 0), size);
		}

		private protected override void AttachingToWindow()
		{
			base.AttachingToWindow();
			Setup(ItemsView);
		}

		private protected override void DetachingFromWindow()
		{
			base.DetachingFromWindow();
			TearDown(ItemsView);
		}

		internal bool InitialPositionSet { get; private set; }

		void TearDown(CarouselView carouselView)
		{
			_oldViews = null;

			carouselView.Scrolled -= CarouselViewScrolled;
			DeviceDisplay.MainDisplayInfoChanged -= OnDisplayInfoChanged;

			UnsubscribeCollectionItemsSourceChanged(ItemsSource);

			_carouselViewLoopManager?.Dispose();
			_carouselViewLoopManager = null;
		}

		void Setup(CarouselView carouselView)
		{
			InitializeCarouselViewLoopManager();

			_oldViews = new List<View>();

			carouselView.Scrolled += CarouselViewScrolled;
			DeviceDisplay.MainDisplayInfoChanged += OnDisplayInfoChanged;

			SubscribeCollectionItemsSourceChanged(ItemsSource);
		}

		internal void UpdateIsScrolling(bool isScrolling)
		{
			if (ItemsView is CarouselView carousel)
			{
				carousel.IsScrolling = isScrolling;
			}
		}

		internal NSIndexPath GetScrollToIndexPath(int position)
		{
			if (ItemsView?.Loop == true && _carouselViewLoopManager != null)
			{
				return _carouselViewLoopManager.GetGoToIndex(CollectionView, position);
			}

			return NSIndexPath.FromItemSection(position, 0);
		}

		internal int GetIndexFromIndexPath(NSIndexPath indexPath)
		{
			if (ItemsView?.Loop == true && _carouselViewLoopManager != null)
			{
				return _carouselViewLoopManager.GetCorrectedIndexFromIndexPath(indexPath);
			}

			return indexPath.Row;
		}

		[UnconditionalSuppressMessage("Memory", "MEM0003", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		void CarouselViewScrolled(object sender, ItemsViewScrolledEventArgs e)
		{
			// If we are trying to center the item when Loop is enabled we don't want to update the position
			if (_isCenteringItem)
			{
				return;
			}

			// If we are dragging the carousel we don't want to update the position
			// We will do it when the dragging ends
			if (_isDragging)
			{
				return;
			}

			// If we are rotating the device we don't want to update the position
			if (_isRotating)
			{
				return;
			}

			SetPosition(e.CenterItemIndex);

			UpdateVisualStates();
		}

		int _positionAfterUpdate = -1;

		[UnconditionalSuppressMessage("Memory", "MEM0003", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		void CollectionViewUpdating(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (ItemsView is not CarouselView carousel)
			{
				return;
			}

			int carouselPosition = carousel.Position;
			_positionAfterUpdate = carouselPosition;
			var currentItemPosition = ItemsSource.GetIndexForItem(carousel.CurrentItem).Row;
			var count = ItemsSource.ItemCount;

			if (e.Action == NotifyCollectionChangedAction.Remove)
			{
				_positionAfterUpdate = GetPositionWhenRemovingItems(e.OldStartingIndex, carouselPosition, currentItemPosition, count);
			}

			if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				_positionAfterUpdate = GetPositionWhenResetItems();
			}

			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				_positionAfterUpdate = GetPositionWhenAddingItems(carouselPosition, currentItemPosition);
			}
		}

		[UnconditionalSuppressMessage("Memory", "MEM0003", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		void CollectionViewUpdated(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_positionAfterUpdate == -1)
			{
				return;
			}

			_gotoPosition = -1;

			var targetPosition = _positionAfterUpdate;
			_positionAfterUpdate = -1;

			SetPosition(targetPosition);
			SetCurrentItem(targetPosition);
		}

		int GetPositionWhenAddingItems(int carouselPosition, int currentItemPosition)
		{
			//If we are adding a new item make sure to maintain the CurrentItemPosition
			return currentItemPosition != -1 ? currentItemPosition : carouselPosition;
		}

		int GetPositionWhenResetItems()
		{
			//If we are reseting the collection Position should go to 0
			ItemsView?.SetValueFromRenderer(CarouselView.CurrentItemProperty, null);
			return 0;
		}

		int GetPositionWhenRemovingItems(int oldStartingIndex, int carouselPosition, int currentItemPosition, int count)
		{
			bool removingCurrentElement = currentItemPosition == -1;

			bool removingFirstElement = oldStartingIndex == 0;
			bool removingLastElement = oldStartingIndex == count;

			int currentPosition = ItemsView?.Position ?? 0;
			bool removingCurrentElementAndLast = removingCurrentElement && removingLastElement && currentPosition > 0;
			if (removingCurrentElementAndLast)
			{
				//If we are removing the last element update the position
				carouselPosition = currentPosition - 1;
			}
			else if (removingFirstElement && !removingCurrentElement)
			{
				//If we are not removing the current element set position to the CurrentItem
				carouselPosition = currentItemPosition;
			}

			return carouselPosition;
		}

		void SubscribeCollectionItemsSourceChanged(IItemsViewSource itemsSource)
		{
			if (itemsSource is ObservableItemsSource newItemsSource)
			{
				newItemsSource.CollectionViewUpdating += CollectionViewUpdating;
				newItemsSource.CollectionViewUpdated += CollectionViewUpdated;
			}
		}

		void UnsubscribeCollectionItemsSourceChanged(IItemsViewSource oldItemsSource)
		{
			if (oldItemsSource is ObservableItemsSource oldObservableItemsSource)
			{
				oldObservableItemsSource.CollectionViewUpdating -= CollectionViewUpdating;
				oldObservableItemsSource.CollectionViewUpdated -= CollectionViewUpdated;
			}
		}

		internal void UpdateLoop()
		{
			if (ItemsView is not CarouselView carousel)
			{
				return;
			}

			var carouselPosition = carousel.Position;

			if (LoopItemsSource != null)
			{
				LoopItemsSource.Loop = carousel.Loop;
			}

			CollectionView.ReloadData();

			ScrollToPosition(carouselPosition, carouselPosition, false, true);
		}

		void ScrollToPosition(int goToPosition, int carouselPosition, bool animate, bool forceScroll = false)
		{
			if (ItemsView is not CarouselView carousel)
			{
				return;
			}

			if (carousel.Loop)
			{
				carouselPosition = _carouselViewLoopManager?.GetCorrectPositionForCenterItem(CollectionView) ?? -1;
			}

			//no center item found, collection could be empty
			//also if we are dragging we don't need to ScrollTo
			if (carousel.IsDragging || carouselPosition == -1)
			{
				return;
			}

			if (_gotoPosition == -1 && (goToPosition != carouselPosition || forceScroll))
			{
				_gotoPosition = goToPosition;
				carousel.ScrollTo(goToPosition, position: Microsoft.Maui.Controls.ScrollToPosition.Center, animate: animate);
			}
		}

		void SetPosition(int position)
		{
			if (position == -1 || ItemsView is not CarouselView carousel)
			{
				return;
			}

			//we arrived center
			if (position == _gotoPosition)
			{
				_gotoPosition = -1;
			}

			//If _gotoPosition is != -1 we are scrolling to that possition
			if (_gotoPosition == -1 && carousel.Position != position)
			{
				carousel.SetValueFromRenderer(CarouselView.PositionProperty, position);
			}
		}

		void SetCurrentItem(int carouselPosition)
		{
			if (ItemsSource.ItemCount == 0)
			{
				return;
			}

			var item = GetItemAtIndex(NSIndexPath.FromItemSection(carouselPosition, 0));
			ItemsView?.SetValueFromRenderer(CarouselView.CurrentItemProperty, item);
			UpdateVisualStates();
		}

		internal void UpdateFromCurrentItem()
		{
			if (ItemsView is not CarouselView carousel)
			{
				return;
			}

			if (carousel.CurrentItem == null || ItemsSource == null || ItemsSource.ItemCount == 0)
			{
				return;
			}

			var currentItemPosition = GetIndexForItem(carousel.CurrentItem).Row;

			ScrollToPosition(currentItemPosition, carousel.Position, carousel.AnimateCurrentItemChanges);

			UpdateVisualStates();
		}

		internal void UpdateFromPosition()
		{
			if (ItemsView is not CarouselView carousel)
			{
				return;
			}

			var itemsCount = ItemsSource?.ItemCount;
			if (itemsCount == 0)
			{
				return;
			}

			var currentItemPosition = GetIndexForItem(carousel.CurrentItem).Row;
			var carouselPosition = carousel.Position;
			if (carouselPosition == _gotoPosition)
			{
				_gotoPosition = -1;
			}

			ScrollToPosition(carouselPosition, currentItemPosition, carousel.AnimatePositionChanges);

			SetCurrentItem(carouselPosition);
		}

		void UpdateInitialPosition()
		{
			var itemsCount = ItemsSource?.ItemCount;

			if (itemsCount == 0)
			{
				return;
			}

			if (!InitialPositionSet)
			{
				if (ItemsView is not CarouselView carousel)
				{
					return;
				}

				InitialPositionSet = true;

				int position = carousel.Position;
				var currentItem = carousel.CurrentItem;
				if (currentItem != null)
				{
					position = ItemsSource.GetIndexForItem(currentItem).Row;
				}
				else
				{
					SetCurrentItem(position);
				}

				carousel.ScrollTo(position, -1, Microsoft.Maui.Controls.ScrollToPosition.Center, false);
			}

			UpdateVisualStates();
		}

		void UpdateVisualStates()
		{
			if (ItemsView is not CarouselView carousel)
			{
				return;
			}

			// We aren't ready to update the visual states yet
			if (_oldViews == null)
			{
				return;
			}

			var cells = CollectionView.VisibleCells;

			var newViews = new List<View>();

			var carouselPosition = carousel.Position;
			var previousPosition = carouselPosition - 1;
			var nextPosition = carouselPosition + 1;

			foreach (var cell in cells)
			{
				if (!((cell as CarouselTemplatedCell)?.PlatformHandler?.VirtualView is View itemView))
				{
					return;
				}

				var item = itemView.BindingContext;
				var pos = ItemsSource.GetIndexForItem(item).Row;

				if (pos == carouselPosition)
				{
					VisualStateManager.GoToState(itemView, CarouselView.CurrentItemVisualState);
				}
				else if (pos == previousPosition)
				{
					VisualStateManager.GoToState(itemView, CarouselView.PreviousItemVisualState);
				}
				else if (pos == nextPosition)
				{
					VisualStateManager.GoToState(itemView, CarouselView.NextItemVisualState);
				}
				else
				{
					VisualStateManager.GoToState(itemView, CarouselView.DefaultItemVisualState);
				}

				newViews.Add(itemView);

				if (!carousel.VisibleViews.Contains(itemView))
				{
					carousel.VisibleViews.Add(itemView);
				}
			}

			foreach (var itemView in _oldViews)
			{
				if (!newViews.Contains(itemView))
				{
					VisualStateManager.GoToState(itemView, CarouselView.DefaultItemVisualState);
					if (carousel.VisibleViews.Contains(itemView))
					{
						carousel.VisibleViews.Remove(itemView);
					}
				}
			}

			_oldViews = newViews;
		}

		internal protected override void UpdateVisibility()
		{
			if (ItemsView.IsVisible)
			{
				CollectionView.Hidden = false;
			}
			else
			{
				CollectionView.Hidden = true;
			}
		}
	}

	class CarouselViewLoopManager : IDisposable
	{
		int _indexOffset = 0;
		UICollectionViewFlowLayout _layout;
		const int LoopCount = 3;
		ILoopItemsViewSource _itemsSource;
		bool _disposed;

		public CarouselViewLoopManager(UICollectionViewFlowLayout layout)
		{
			if (layout == null)
			{
				throw new ArgumentNullException(nameof(layout), "LoopManager expects a UICollectionViewFlowLayout");
			}

			_layout = layout;
		}

		public void CenterIfNeeded(UICollectionView collectionView, bool isHorizontal)
		{
			if (isHorizontal)
			{
				CenterHorizontalIfNeeded(collectionView);
			}
			else
			{
				CenterVerticallyIfNeeded(collectionView);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					_itemsSource = null;
				}

				_disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		public (UICollectionViewCell cell, int correctedIndex) GetCellAndCorrectIndex(UICollectionView collectionView, NSIndexPath indexPath, string reuseId)
		{
			var cell = collectionView.DequeueReusableCell(reuseId, indexPath) as UICollectionViewCell;
			var correctedIndex = GetCorrectedIndexFromIndexPath(indexPath);
			return (cell, correctedIndex);
		}

		public int GetCorrectedIndexFromIndexPath(NSIndexPath indexPath)
		{
			return GetCorrectedIndex(indexPath.Row - _indexOffset);
		}

		public int GetCorrectPositionForCenterItem(UICollectionView collectionView)
		{
			NSIndexPath centerIndexPath = GetIndexPathForCenteredItem(collectionView);
			if (centerIndexPath == null)
			{
				return -1;
			}

			return GetCorrectedIndexFromIndexPath(centerIndexPath);
		}

		public NSIndexPath GetGoToIndex(UICollectionView collectionView, int newPosition)
		{
			NSIndexPath centerIndexPath = GetIndexPathForCenteredItem(collectionView);
			if (centerIndexPath == null)
			{
				return NSIndexPath.FromItemSection(0, 0);
			}

			var currentCarouselPosition = GetCorrectedIndexFromIndexPath(centerIndexPath);
			var itemSourceCount = _itemsSource.ItemCount;

			var diffToStart = currentCarouselPosition + (itemSourceCount - newPosition);
			var diffToEnd = itemSourceCount - currentCarouselPosition + newPosition;

			var increment = currentCarouselPosition - newPosition;
			var incrementAbs = Math.Abs(increment);

			int goToPosition;
			if (diffToStart < incrementAbs)
			{
				goToPosition = centerIndexPath.Row - diffToStart;
			}
			else if (diffToEnd < incrementAbs)
			{
				goToPosition = centerIndexPath.Row + diffToEnd;
			}
			else
			{
				goToPosition = centerIndexPath.Row - increment;
			}

			NSIndexPath goToIndexPath = NSIndexPath.FromItemSection(goToPosition, 0);

			return goToIndexPath;
		}

		public void SetItemsSource(ILoopItemsViewSource itemsSource) => _itemsSource = itemsSource;

		void CenterVerticallyIfNeeded(UICollectionView collectionView)
		{
			var cellHeight = _layout.ItemSize.Height;
			var cellPadding = 0;
			var currentOffset = collectionView.ContentOffset;
			var contentHeight = GetTotalContentHeight();
			var boundsHeight = collectionView.Bounds.Size.Height;

			if (contentHeight == 0 || cellHeight == 0)
			{
				return;
			}

			var centerOffsetY = (LoopCount * contentHeight - boundsHeight) / 2;
			var distFromCenter = centerOffsetY - currentOffset.Y;

			if (Math.Abs(distFromCenter) > (contentHeight / GetMinLoopCount()))
			{
				var cellcount = distFromCenter / (cellHeight + cellPadding);
				var shiftCells = (int)((cellcount > 0) ? Math.Floor(cellcount) : Math.Ceiling(cellcount));
				var offsetCorrection = (Math.Abs(cellcount) % 1.0) * (cellHeight + cellPadding);

				if (collectionView.ContentOffset.Y < centerOffsetY)
				{
					collectionView.ContentOffset = new CGPoint(currentOffset.X, centerOffsetY - offsetCorrection);
				}
				else if (collectionView.ContentOffset.Y > centerOffsetY)
				{
					collectionView.ContentOffset = new CGPoint(currentOffset.X, centerOffsetY + offsetCorrection);
				}

				FinishCenterIfNeeded(collectionView, shiftCells);
			}
		}

		void CenterHorizontalIfNeeded(UICollectionView collectionView)
		{
			var cellWidth = _layout.ItemSize.Width;
			var cellPadding = 0;
			var currentOffset = collectionView.ContentOffset;
			var contentWidth = GetTotalContentWidth();
			var boundsWidth = collectionView.Bounds.Size.Width;

			if (contentWidth == 0 || cellWidth == 0)
			{
				return;
			}

			var centerOffsetX = (LoopCount * contentWidth - boundsWidth) / 2;
			var distFromCentre = centerOffsetX - currentOffset.X;

			if (Math.Abs(distFromCentre) > (contentWidth / GetMinLoopCount()))
			{
				var cellcount = distFromCentre / (cellWidth + cellPadding);
				var shiftCells = (int)((cellcount > 0) ? Math.Floor(cellcount) : Math.Ceiling(cellcount));
				var offsetCorrection = (Math.Abs(cellcount % 1.0f)) * (cellWidth + cellPadding);

				if (collectionView.ContentOffset.X < centerOffsetX)
				{
					collectionView.ContentOffset = new CGPoint(centerOffsetX - offsetCorrection, currentOffset.Y);
				}
				else if (collectionView.ContentOffset.X > centerOffsetX)
				{
					collectionView.ContentOffset = new CGPoint(centerOffsetX + offsetCorrection, currentOffset.Y);
				}

				FinishCenterIfNeeded(collectionView, shiftCells);
			}

		}

		void FinishCenterIfNeeded(UICollectionView collectionView, int shiftCells)
		{
			ShiftContentArray(shiftCells);

			collectionView.ReloadData();
		}

		int GetCorrectedIndex(int indexToCorrect)
		{
			var itemsCount = GetItemsSourceCount();
			if ((indexToCorrect < itemsCount && indexToCorrect >= 0) || itemsCount == 0)
			{
				return indexToCorrect;
			}

			var countInIndex = (double)(indexToCorrect / itemsCount);
			var flooredValue = (int)(Math.Floor(countInIndex));
			var offset = itemsCount * flooredValue;
			var newIndex = indexToCorrect - offset;
			if (newIndex < 0)
				return (itemsCount - Math.Abs(newIndex));
			return newIndex;
		}

		NSIndexPath GetIndexPathForCenteredItem(UICollectionView collectionView)
		{
			var centerPoint = new CGPoint(collectionView.Center.X + collectionView.ContentOffset.X, collectionView.Center.Y + collectionView.ContentOffset.Y);
			var centerIndexPath = collectionView.IndexPathForItemAtPoint(centerPoint);
			return centerIndexPath;
		}

		int GetMinLoopCount() => Math.Min(LoopCount, GetItemsSourceCount());

		int GetItemsSourceCount() => _itemsSource.ItemCount;

		nfloat GetTotalContentWidth() => GetItemsSourceCount() * _layout.ItemSize.Width;

		nfloat GetTotalContentHeight() => GetItemsSourceCount() * _layout.ItemSize.Height;

		void ShiftContentArray(int shiftCells)
		{
			var correctedIndex = GetCorrectedIndex(shiftCells);
			_indexOffset += correctedIndex;
		}
	}
}
