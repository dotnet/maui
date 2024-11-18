#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.ApplicationModel;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public class CarouselViewController2 : ItemsViewController2<CarouselView>
	{
		bool _isUpdating = false;
		int _section = 0;
		CarouselViewLoopManager _carouselViewLoopManager;

		// We need to keep track of the old views to update the visual states
		// if this is null we are not attached to the window
		List<View> _oldViews;
		Items.ILoopItemsViewSource LoopItemsSource => ItemsSource as Items.ILoopItemsViewSource;

		public CarouselViewController2(CarouselView itemsView, UICollectionViewLayout layout) : base(itemsView, layout)
		{
			CollectionView.AllowsSelection = false;
			CollectionView.AllowsMultipleSelection = false;
		}

		private protected override NSIndexPath GetAdjustedIndexPathForItemSource(NSIndexPath indexPath)
		{
			return NSIndexPath.FromItemSection(GetIndexFromIndexPath(indexPath), _section);
		}

		public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
		{
			UICollectionViewCell cell;

			cell = base.GetCell(collectionView, indexPath);

			var element = (cell as TemplatedCell2)?.PlatformHandler?.VirtualView as VisualElement;

			if (element is not null)
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
				_carouselViewLoopManager = new CarouselViewLoopManager();
				_carouselViewLoopManager.SetItemsSource(LoopItemsSource);
			}
		}

		public override void ViewDidLoad()
		{
			InitializeCarouselViewLoopManager();
			base.ViewDidLoad();
		}

		public override void ViewWillLayoutSubviews()
		{
			base.ViewWillLayoutSubviews();
			UpdateVisualStates();
		}


		public override async void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();
			await UpdateInitialPosition();
		}

		public override void DraggingStarted(UIScrollView scrollView)
		{
			//	_isDragging = true;
			ItemsView?.SetIsDragging(true);
		}

		public override void DraggingEnded(UIScrollView scrollView, bool willDecelerate)
		{
			ItemsView?.SetIsDragging(false);
			//_isDragging = false;
		}

		public override void UpdateItemsSource()
		{
			UnsubscribeCollectionItemsSourceChanged(ItemsSource);
			_isUpdating = true;
			base.UpdateItemsSource();
			//we don't need to Subscribe because base calls CreateItemsViewSource
			_carouselViewLoopManager?.SetItemsSource(LoopItemsSource);

			if (InitialPositionSet && ItemsView is CarouselView carousel)
			{
				carousel.SetValueFromRenderer(CarouselView.CurrentItemProperty, null);
				carousel.SetValueFromRenderer(CarouselView.PositionProperty, 0);
			}
			_isUpdating = false;
		}

		protected override bool IsHorizontal => ItemsView?.ItemsLayout?.Orientation == ItemsLayoutOrientation.Horizontal;

		protected override UICollectionViewDelegateFlowLayout CreateDelegator() => new CarouselViewDelegator2(ItemsViewLayout, this);

		protected override string DetermineCellReuseId(NSIndexPath indexPath)
		{
			var itemIndex = GetAdjustedIndexPathForItemSource(indexPath);
			return base.DetermineCellReuseId(itemIndex);
		}

		protected override Items.IItemsViewSource CreateItemsViewSource()
		{
			var itemsSource = ItemsSourceFactory2.CreateForCarouselView(ItemsView.ItemsSource, this, ItemsView.Loop);
			_carouselViewLoopManager?.SetItemsSource(itemsSource);
			SubscribeCollectionItemsSourceChanged(itemsSource);
			return itemsSource;
		}

		private protected override async void AttachingToWindow()
		{
			base.AttachingToWindow();
			Setup(ItemsView);
			// if we navigate back on NavigationController LayoutSubviews might not fire.
			await UpdateInitialPosition();
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
			InitialPositionSet = false;

			UnsubscribeCollectionItemsSourceChanged(ItemsSource);

			_carouselViewLoopManager?.Dispose();
			_carouselViewLoopManager = null;
			_isUpdating = false;
		}

		void Setup(CarouselView carouselView)
		{
			InitializeCarouselViewLoopManager();

			_oldViews = new List<View>();

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
				return _carouselViewLoopManager.GetCorrectedIndexPathFromIndex(position);
			}

			return NSIndexPath.FromItemSection(position, _section);
		}

		internal int GetIndexFromIndexPath(NSIndexPath indexPath)
		{
			if (ItemsView?.Loop == true && _carouselViewLoopManager != null)
			{
				return _carouselViewLoopManager.GetCorrectedIndexFromIndexPath(indexPath);
			}

			return indexPath.Row;
		}

		// [UnconditionalSuppressMessage("Memory", "MEM0003", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		// void CarouselViewScrolled(object sender, ItemsViewScrolledEventArgs e)
		// {
		// 	System.Diagnostics.Debug.WriteLine($"CarouselViewScrolled: {e.CenterItemIndex}");
		// 	// If we are trying to center the item when Loop is enabled we don't want to update the position
		// 	// if (_isCenteringItem)
		// 	// {
		// 	// 	return;
		// 	// }

		// 	// // If we are dragging the carousel we don't want to update the position
		// 	// // We will do it when the dragging ends
		// 	// if (_isDragging)
		// 	// {
		// 	// 	return;
		// 	// }

		// 	//	SetPosition(e.CenterItemIndex);

		// 	UpdateVisualStates();
		// }

		int _positionAfterUpdate = -1;

		[UnconditionalSuppressMessage("Memory", "MEM0003", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		void CollectionViewUpdating(object sender, NotifyCollectionChangedEventArgs e)
		{
			_isUpdating = true;
			if (ItemsView is not CarouselView carousel)
			{
				return;
			}

			var count = ItemsSource.ItemCount;

			if (count == 0)
			{
				_positionAfterUpdate = -1;
				return;
			}

			int carouselPosition = carousel.Position;
			_positionAfterUpdate = carouselPosition;
			var currentItemPosition = ItemsSource.GetIndexForItem(carousel.CurrentItem).Row;

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

			//_gotoPosition = -1;

			var targetPosition = _positionAfterUpdate;
			_positionAfterUpdate = -1;

			SetPosition(targetPosition);
			SetCurrentItem(targetPosition);

			if (e.Action == NotifyCollectionChangedAction.Reset)
			{

			}
			else
			{
				//Since we can be removing the item that is already created next to the current item we need to update the visible cells
				if (ItemsView.Loop)
				{
					CollectionView.ReloadItems(new NSIndexPath[] { GetScrollToIndexPath(targetPosition) });
				}
			}


			_isUpdating = false;
			ScrollToPosition(targetPosition, targetPosition, false, true);
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

		void SubscribeCollectionItemsSourceChanged(Items.IItemsViewSource itemsSource)
		{
			UnsubscribeCollectionItemsSourceChanged(ItemsSource);

			if (itemsSource is Items.ObservableItemsSource newItemsSource)
			{
				newItemsSource.CollectionViewUpdating += CollectionViewUpdating;
				newItemsSource.CollectionViewUpdated += CollectionViewUpdated;
			}
		}

		void UnsubscribeCollectionItemsSourceChanged(Items.IItemsViewSource oldItemsSource)
		{
			if (oldItemsSource is Items.ObservableItemsSource oldObservableItemsSource)
			{
				oldObservableItemsSource.CollectionViewUpdating -= CollectionViewUpdating;
				oldObservableItemsSource.CollectionViewUpdated -= CollectionViewUpdated;
			}
		}

		internal bool IsUpdating()
		{
			return _isUpdating;
		}
		internal void UpdateLoop()
		{
			if (ItemsView is not CarouselView carousel)
			{
				return;
			}

			var carouselPosition = carousel.Position;

			if (LoopItemsSource is not null)
			{
				LoopItemsSource.Loop = carousel.Loop;
			}

			// CollectionView.ReloadData();

			// ScrollToPosition(carouselPosition, carouselPosition, false, true);
		}

		void ScrollToPosition(int goToPosition, int carouselPosition, bool animate, bool forceScroll = false)
		{
			if (ItemsView is not CarouselView carousel)
			{
				return;
			}

			if (ItemsSource is null || ItemsSource.ItemCount == 0)
			{
				return;
			}

			if (goToPosition != carouselPosition || forceScroll)
			{
				UICollectionViewScrollPosition uICollectionViewScrollPosition = IsHorizontal ? UICollectionViewScrollPosition.CenteredHorizontally : UICollectionViewScrollPosition.CenteredVertically;
				var goToIndexPath = GetScrollToIndexPath(goToPosition);

				CollectionView.ScrollToItem(goToIndexPath, uICollectionViewScrollPosition, animate);
			}
		}

		internal void SetPosition(int position)
		{
			if (ItemsView is not CarouselView carousel)
			{
				return;
			}

			if (ItemsSource is null || ItemsSource.ItemCount == 0)
			{
				return;
			}

			if (!InitialPositionSet || position == -1)
			{
				return;
			}

			ItemsView.SetValueFromRenderer(CarouselView.PositionProperty, position);
			SetCurrentItem(position);
			UpdateVisualStates();
		}

		void SetCurrentItem(int carouselPosition)
		{
			if (ItemsView is not CarouselView carousel)
			{
				return;
			}

			if (ItemsSource is null || ItemsSource.ItemCount == 0)
			{
				return;
			}

			var item = GetItemAtIndex(NSIndexPath.FromItemSection(carouselPosition, _section));
			ItemsView?.SetValueFromRenderer(CarouselView.CurrentItemProperty, item);
			UpdateVisualStates();
		}

		internal void UpdateFromCurrentItem()
		{
			if (!InitialPositionSet)
				return;

			if (ItemsView is not CarouselView carousel)
			{
				return;
			}

			if (carousel.CurrentItem is null || ItemsSource is null || ItemsSource.ItemCount == 0)
			{
				return;
			}

			var currentItemPosition = GetIndexForItem(carousel.CurrentItem).Row;

			ScrollToPosition(currentItemPosition, carousel.Position, carousel.AnimateCurrentItemChanges);

			UpdateVisualStates();
		}

		internal void UpdateFromPosition()
		{
			if (!InitialPositionSet)
			{
				return;
			}

			if (ItemsView is not CarouselView carousel)
			{
				return;
			}

			if (ItemsSource is null || ItemsSource.ItemCount == 0)
			{
				return;
			}

			var currentItemPosition = GetIndexForItem(carousel.CurrentItem).Row;
			var carouselPosition = carousel.Position;

			ScrollToPosition(carouselPosition, currentItemPosition, carousel.AnimatePositionChanges);

			// SetCurrentItem(carouselPosition);
		}

		async Task UpdateInitialPosition()
		{
			if (ItemsView is not CarouselView carousel)
			{
				return;
			}

			if (ItemsSource is null)
			{
				return;
			}

			if (InitialPositionSet)
			{
				return;
			}

			int position = carousel.Position;
			var currentItem = carousel.CurrentItem;

			if (currentItem != null)
			{
				// Sometimes the item could be just being removed while we navigate back to the CarouselView
				var positionCurrentItem = ItemsSource.GetIndexForItem(currentItem).Row;
				if (positionCurrentItem != -1)
				{
					position = positionCurrentItem;
				}
			}

			var projectedPosition = NSIndexPath.FromItemSection(position, _section);

			if (LoopItemsSource.Loop)
			{
				//We need to set the position to the correct position since we added 1 item at the beginning
				projectedPosition = GetScrollToIndexPath(position);
			}

			var uICollectionViewScrollPosition = IsHorizontal ? UICollectionViewScrollPosition.CenteredHorizontally : UICollectionViewScrollPosition.CenteredVertically;

			await Task.Delay(100).ContinueWith((t) =>
			{
				MainThread.BeginInvokeOnMainThread(() =>
				{
					if (!IsViewLoaded)
					{
						return;
					}
					InitialPositionSet = true;

					if (ItemsSource is null || ItemsSource.ItemCount == 0)
					{
						return;
					}

					CollectionView.ScrollToItem(projectedPosition, uICollectionViewScrollPosition, false);

					//Set the position on VirtualView to update the CurrentItem also
					SetPosition(position);

					UpdateVisualStates();
				});

			});

		}

		void UpdateVisualStates()
		{
			if (ItemsView is not CarouselView carousel)
			{
				return;
			}

			// We aren't ready to update the visual states yet
			if (_oldViews is null)
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
				if (!((cell as TemplatedCell2)?.PlatformHandler?.VirtualView is View itemView))
				{
					return;
				}

				var item = itemView.BindingContext;
				var pos = GetIndexForItem(item).Row;

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
		int _section = 0;
		Items.ILoopItemsViewSource _itemsSource;
		bool _disposed;

		public CarouselViewLoopManager()
		{

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

		public NSIndexPath GetCorrectedIndexPathFromIndex(int index)
		{
			return NSIndexPath.FromItemSection(index + 1, _section);
		}

		public int GetCorrectedIndexFromIndexPath(NSIndexPath indexPath)
		{
			if (indexPath.Row == 0)
			{
				return Math.Max(0, _itemsSource.ItemCount - 1);
			}
			else if (indexPath.Row == _itemsSource.ItemCount + 1)
			{
				return 0;
			}
			else
			{
				return Math.Max(0, indexPath.Row - 1);
			}
		}

		public void SetItemsSource(Items.ILoopItemsViewSource itemsSource) => _itemsSource = itemsSource;
	}
}
