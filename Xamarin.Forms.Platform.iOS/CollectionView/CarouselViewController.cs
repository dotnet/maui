using System.Collections;
using System.Collections.Generic;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class CarouselViewController : ItemsViewController<CarouselView>
	{
		protected readonly CarouselView Carousel;

		bool _initialPositionSet;
		List<View> _oldViews;
		int _gotoPosition = -1;

		public CarouselViewController(CarouselView itemsView, ItemsViewLayout layout) : base(itemsView, layout)
		{
			Carousel = itemsView;
			CollectionView.AllowsSelection = false;
			CollectionView.AllowsMultipleSelection = false;
			Carousel.PropertyChanged += CarouselViewPropertyChanged;
			Carousel.Scrolled += CarouselViewScrolled;
			_oldViews = new List<View>();
		}

		public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
		{
			var cell = base.GetCell(collectionView, indexPath);

			var element = (cell as CarouselTemplatedCell)?.VisualElementRenderer?.Element;
			if (element != null)
				VisualStateManager.GoToState(element, CarouselView.DefaultItemVisualState);
			return cell;
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();
			UpdateInitialPosition();
		}

		public override void DraggingStarted(UIScrollView scrollView)
		{
			Carousel.SetIsDragging(true);
		}

		public override void DraggingEnded(UIScrollView scrollView, bool willDecelerate)
		{
			Carousel.SetIsDragging(false);
		}

		public override void UpdateItemsSource()
		{
			UnsubscribeCollectionItemsSourceChanged(ItemsSource);
			base.UpdateItemsSource();
			SubscribeCollectionItemsSourceChanged(ItemsSource);
			_initialPositionSet = false;
			UpdateInitialPosition();
		}

		protected override bool IsHorizontal => (Carousel?.ItemsLayout as ItemsLayout)?.Orientation == ItemsLayoutOrientation.Horizontal;

		protected override UICollectionViewDelegateFlowLayout CreateDelegator() => new CarouselViewDelegator(ItemsViewLayout, this);

		protected override string DetermineCellReuseId()
		{
			if (Carousel.ItemTemplate != null)
				return CarouselTemplatedCell.ReuseId;

			return base.DetermineCellReuseId();
		}

		protected override void RegisterViewTypes()
		{
			CollectionView.RegisterClassForCell(typeof(CarouselTemplatedCell), CarouselTemplatedCell.ReuseId);
			base.RegisterViewTypes();
		}

		protected override IItemsViewSource CreateItemsViewSource()
		{
			var itemsSource = base.CreateItemsViewSource();
			SubscribeCollectionItemsSourceChanged(itemsSource);
			return itemsSource;
		}

		protected override void BoundsSizeChanged()
		{
			base.BoundsSizeChanged();
			Carousel.ScrollTo(Carousel.Position, position: Xamarin.Forms.ScrollToPosition.Center, animate: false);
		}

		internal void TearDown()
		{
			Carousel.PropertyChanged -= CarouselViewPropertyChanged;
			Carousel.Scrolled -= CarouselViewScrolled;
			UnsubscribeCollectionItemsSourceChanged(ItemsSource);
		}

		internal void UpdateIsScrolling(bool isScrolling)
		{
			Carousel.IsScrolling = isScrolling;
		}

		void CarouselViewScrolled(object sender, ItemsViewScrolledEventArgs e)
		{
			SetPosition(e.CenterItemIndex);
			UpdateVisualStates();
		}

		void CollectionItemsSourceChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			var carouselPosition = Carousel.Position;
			var currentItemPosition = ItemsSource.GetIndexForItem(Carousel.CurrentItem).Row;
			var count = ItemsSource.ItemCount;

			bool removingCurrentElement = currentItemPosition == -1;
			bool removingLastElement = e.OldStartingIndex == count;
			bool removingFirstElement = e.OldStartingIndex == 0;
			bool removingCurrentElementButNotFirst = removingCurrentElement && removingLastElement && Carousel.Position > 0;

			if (removingCurrentElementButNotFirst)
			{
				carouselPosition = Carousel.Position - 1;
			}
			else if (removingFirstElement && !removingCurrentElement)
			{
				carouselPosition = currentItemPosition;
			}

			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
			{
				carouselPosition = 0;
			}

			//If we are adding a new item make sure to maintain the CurrentItemPosition
			else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add
				&& currentItemPosition != -1)
			{
				carouselPosition = currentItemPosition;
			}

			_gotoPosition = -1;

			SetCurrentItem(carouselPosition);
			SetPosition(carouselPosition);
		}

		void SubscribeCollectionItemsSourceChanged(IItemsViewSource itemsSource)
		{
			if (itemsSource is ObservableItemsSource newItemsSource)
				newItemsSource.CollectionItemsSourceChanged += CollectionItemsSourceChanged;
		}

		void UnsubscribeCollectionItemsSourceChanged(IItemsViewSource oldItemsSource)
		{
			if (oldItemsSource is ObservableItemsSource oldObservableItemsSource)
				oldObservableItemsSource.CollectionItemsSourceChanged -= CollectionItemsSourceChanged;
		}

		void CarouselViewPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs changedProperty)
		{
			if (changedProperty.Is(CarouselView.PositionProperty))
				UpdateFromPosition();
			else if (changedProperty.Is(CarouselView.CurrentItemProperty))
				UpdateFromCurrentItem();
		}

		void SetPosition(int position)
		{
			var carouselPosition = Carousel.Position;
			//we arrived center
			if (position == _gotoPosition)
				_gotoPosition = -1;

			if (_gotoPosition == -1 && carouselPosition != position)
			{
				Carousel.SetValueFromRenderer(CarouselView.PositionProperty, position);
			}
		}

		void SetCurrentItem(int carouselPosition)
		{
			if (ItemsSource.ItemCount == 0)
				return;

			var item = GetItemAtIndex(NSIndexPath.FromItemSection(carouselPosition, 0));
			Carousel.SetValueFromRenderer(CarouselView.CurrentItemProperty, item);
			UpdateVisualStates();
		}

		void UpdateFromCurrentItem()
		{
			var currentItemPosition = GetIndexForItem(Carousel.CurrentItem).Row;

			ScrollToPosition(currentItemPosition, Carousel.Position, Carousel.AnimateCurrentItemChanges);

			UpdateVisualStates();
		}

		void ScrollToPosition(int goToPosition, int carouselPosition, bool animate, bool forceScroll = false)
		{
			if (_gotoPosition == -1 && (goToPosition != carouselPosition || forceScroll))
			{
				_gotoPosition = goToPosition;
				Carousel.ScrollTo(goToPosition, position: Xamarin.Forms.ScrollToPosition.Center, animate: animate);
			}
		}

		void UpdateFromPosition()
		{
			var currentItemPosition = GetIndexForItem(Carousel.CurrentItem).Row;
			var carouselPosition = Carousel.Position;
			if (carouselPosition == _gotoPosition)
				_gotoPosition = -1;

			if (!Carousel.IsDragging)
				ScrollToPosition(carouselPosition, currentItemPosition, Carousel.AnimatePositionChanges);

			SetCurrentItem(carouselPosition);
		}

		void UpdateInitialPosition()
		{
			var itemsCount = ItemsSource?.ItemCount;

			if (itemsCount == 0)
				return;

			if (!_initialPositionSet)
			{
				_initialPositionSet = true;

				int position = Carousel.Position;
				var currentItem = Carousel.CurrentItem;
				if (currentItem != null)
				{
					position = ItemsSource.GetIndexForItem(currentItem).Row;
				}
				else
				{
					SetCurrentItem(position);
				}

				if (position > 0)
					ScrollToPosition(position, Carousel.Position, Carousel.AnimatePositionChanges, true);
			}

			UpdateVisualStates();
		}

		void UpdateVisualStates()
		{
			var cells = CollectionView.VisibleCells;

			var newViews = new List<View>();

			var carouselPosition = Carousel.Position;
			var previousPosition = carouselPosition - 1;
			var nextPosition = carouselPosition + 1;

			foreach (var cell in cells)
			{
				if (!((cell as CarouselTemplatedCell)?.VisualElementRenderer?.Element is View itemView))
					return;

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

				if (!Carousel.VisibleViews.Contains(itemView))
				{
					Carousel.VisibleViews.Add(itemView);
				}
			}

			foreach (var itemView in _oldViews)
			{
				if (!newViews.Contains(itemView))
				{
					VisualStateManager.GoToState(itemView, CarouselView.DefaultItemVisualState);
					if (Carousel.VisibleViews.Contains(itemView))
					{
						Carousel.VisibleViews.Remove(itemView);
					}
				}
			}

			_oldViews = newViews;
		}
	}
}
