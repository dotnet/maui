using System;
using System.Collections.Generic;
using Tizen.UIExtensions.NUI;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class MauiCarouselView : MauiCollectionView<CarouselView>
	{
		List<View> _oldViews = new List<View>();
		bool _updatePositionFromUI;
		bool _updateCurrentItemFromUI;
		bool _itemLayouted;

		public override void SetupNewElement(CarouselView newElement)
		{
			base.SetupNewElement(newElement);

			Scrolled += OnScrolled;
			ScrollView.ScrollDragStarted += OnDragStart;
			ScrollView.ScrollDragEnded += OnDragStop;
			ScrollView.ScrollAnimationStarted += OnScrollStart;
			Relayout += OnRelayout;
			_itemLayouted = false;
		}

		public override void TearDownOldElement(CarouselView oldElement)
		{
			base.TearDownOldElement(oldElement);

			Scrolled -= OnScrolled;
			ScrollView.ScrollDragStarted -= OnDragStart;
			ScrollView.ScrollDragEnded -= OnDragStop;
			ScrollView.ScrollAnimationStarted -= OnScrollStart;
			Relayout -= OnRelayout;

		}

		void OnRelayout(object? sender, EventArgs e)
		{
			if (Size.Width > 0 && Size.Height > 0)
			{
				Application.Current?.Dispatcher.Dispatch(() =>
				{
					_itemLayouted = true;
					UpdatePosition();
					UpdateCurrentItem();
				});
			}

		}

		public override IItemsLayout GetItemsLayout()
		{
			return ItemsView!.ItemsLayout;
		}

		public override void UpdateAdaptor()
		{
			base.UpdateAdaptor();

			UpdatePosition();
			UpdateCurrentItem();
		}

		public override void UpdateLayoutManager()
		{
			base.UpdateLayoutManager();
			UpdatePosition();
			UpdateCurrentItem();
		}

		public void UpdatePositionFromUI(int position)
		{
			if (ItemsView == null)
				return;

			_updatePositionFromUI = true;
			ItemsView.Position = position;
			_updatePositionFromUI = false;
		}

		public void UpdateCurrentItemFromUI(object item)
		{
			if (ItemsView == null)
				return;

			_updateCurrentItemFromUI = true;
			ItemsView.CurrentItem = item;
			_updateCurrentItemFromUI = false;
		}


		public void UpdateCurrentItem()
		{
			if (_updateCurrentItemFromUI)
				return;

			if (ItemsView == null || Adaptor == null || LayoutManager == null || Size.Width == 0 || Size.Height == 0)
				return;

			if (!_itemLayouted)
				return;

			if (ItemsView.CurrentItem != null)
				ScrollTo(Adaptor.GetItemIndex(ItemsView.CurrentItem));
		}

		public void UpdatePosition()
		{
			if (_updatePositionFromUI)
				return;

			if (!_itemLayouted)
				return;

			if (ItemsView == null || Adaptor == null || LayoutManager == null || Size.Width == 0 || Size.Height == 0)
				return;

			ScrollTo(ItemsView.Position);
		}

		public void UpdateIsSwipeEnabled()
		{
			if (ItemsView == null)
				return;

			ScrollView.ScrollEnabled = ItemsView.IsSwipeEnabled;
		}

		protected override ItemTemplateAdaptor CreateItemAdaptor(ItemsView view)
		{
			return new CarouselViewItemTemplateAdaptor(view);
		}

		void ScrollTo(int position)
		{
			if (ItemsView == null || Adaptor == null || LayoutManager == null)
				return;

			if (ItemsView.IsScrolling)
				return;

			if (position > -1 && position < Adaptor.Count)
			{
				ScrollTo(position, animate: true);
			}
		}

		void OnScrolled(object? sender, CollectionViewScrolledEventArgs e)
		{

			if (ItemsView == null || Adaptor == null || LayoutManager == null)
				return;

			var scrolledIndex = e.CenterItemIndex;

			if (0 <= scrolledIndex && scrolledIndex < Adaptor.Count)
			{
				UpdatePositionFromUI(scrolledIndex);
				UpdateCurrentItemFromUI(Adaptor[scrolledIndex]!);
				RequestItemSelect(scrolledIndex);
			}

			ItemsView.IsScrolling = false;

			if (Adaptor is ItemTemplateAdaptor adaptor)
			{
				var newViews = new List<View>();
				var carouselPosition = ItemsView.Position;
				var previousPosition = carouselPosition - 1;
				var nextPosition = carouselPosition + 1;

				for (int i = e.FirstVisibleItemIndex; i <= e.LastVisibleItemIndex; i++)
				{
					if (i < 0 || i >= Adaptor.Count)
						continue;
					var itemView = adaptor.GetTemplatedView(i);
					if (itemView == null)
					{
						continue;
					}

					if (i == carouselPosition)
					{
						VisualStateManager.GoToState(itemView, CarouselView.CurrentItemVisualState);
					}
					else if (i == previousPosition)
					{
						VisualStateManager.GoToState(itemView, CarouselView.PreviousItemVisualState);
					}
					else if (i == nextPosition)
					{
						VisualStateManager.GoToState(itemView, CarouselView.NextItemVisualState);
					}
					else
					{
						VisualStateManager.GoToState(itemView, CarouselView.DefaultItemVisualState);
					}

					newViews.Add(itemView);

					if (!ItemsView.VisibleViews.Contains(itemView))
					{
						ItemsView.VisibleViews.Add(itemView);
					}
				}

				foreach (var itemView in _oldViews)
				{
					if (!newViews.Contains(itemView))
					{
						VisualStateManager.GoToState(itemView, CarouselView.DefaultItemVisualState);
						if (ItemsView.VisibleViews.Contains(itemView))
						{
							ItemsView.VisibleViews.Remove(itemView);
						}
					}
				}
				_oldViews = newViews;
			}
		}

		void OnDragStart(object? sender, EventArgs e)
		{
			if (ItemsView == null)
				return;

			ItemsView.SetIsDragging(true);
			ItemsView.IsScrolling = true;
		}

		void OnDragStop(object? sender, EventArgs e)
		{
			if (ItemsView == null)
				return;

			ItemsView.SetIsDragging(false);
			ItemsView.IsScrolling = false;
		}

		void OnScrollStart(object? sender, EventArgs e)
		{
			if (ItemsView == null)
				return;

			ItemsView.IsScrolling = true;
		}
	}
}
