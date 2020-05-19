using System;
using System.Collections;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using UWPApp = Windows.UI.Xaml.Application;
using UWPDataTemplate = Windows.UI.Xaml.DataTemplate;
using WScrollBarVisibility = Windows.UI.Xaml.Controls.ScrollBarVisibility;
using WScrollMode = Windows.UI.Xaml.Controls.ScrollMode;
using WSnapPointsAlignment = Windows.UI.Xaml.Controls.Primitives.SnapPointsAlignment;
using WSnapPointsType = Windows.UI.Xaml.Controls.SnapPointsType;

namespace Xamarin.Forms.Platform.UWP
{
	public class CarouselViewRenderer : ItemsViewRenderer<CarouselView>
	{
		ScrollViewer _scrollViewer;
		int _gotoPosition = -1;
		bool _noNeedForScroll;

		public CarouselViewRenderer()
		{
			CarouselView.VerifyCarouselViewFlagEnabled(nameof(CarouselView));
		}

		protected CarouselView Carousel => Element;
		protected override IItemsLayout Layout => Carousel?.ItemsLayout;
		LinearItemsLayout CarouselItemsLayout => Carousel?.ItemsLayout;

		UWPDataTemplate CarouselItemsViewTemplate => (UWPDataTemplate)UWPApp.Current.Resources["CarouselItemsViewDefaultTemplate"];

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs changedProperty)
		{
			base.OnElementPropertyChanged(sender, changedProperty);

			if (changedProperty.IsOneOf(CarouselView.ItemsSourceProperty, LinearItemsLayout.ItemSpacingProperty))
				UpdateItemsSource();
			else if (changedProperty.Is(CarouselView.ItemTemplateProperty))
				UpdateItemTemplate();
			else if (changedProperty.Is(CarouselView.PeekAreaInsetsProperty))
				UpdatePeekAreaInsets();
			else if (changedProperty.Is(CarouselView.IsSwipeEnabledProperty))
				UpdateIsSwipeEnabled();
			else if (changedProperty.Is(CarouselView.IsBounceEnabledProperty))
				UpdateIsBounceEnabled();
			else if (changedProperty.Is(CarouselView.PositionProperty))
				UpdateFromPosition();
			else if (changedProperty.Is(CarouselView.CurrentItemProperty))
				UpdateFromCurrentItem();
		}

		protected override void HandleLayoutPropertyChanged(PropertyChangedEventArgs property)
		{
			if (property.Is(LinearItemsLayout.ItemSpacingProperty))
				UpdateItemSpacing();
			else if (property.Is(ItemsLayout.SnapPointsTypeProperty))
				UpdateSnapPointsType();
			else if (property.Is(ItemsLayout.SnapPointsAlignmentProperty))
				UpdateSnapPointsAlignment();
		}

		protected override void SetUpNewElement(ItemsView newElement)
		{
			base.SetUpNewElement(newElement);

			if (newElement != null)
			{
				ListViewBase.SizeChanged += OnListSizeChanged;
			}
		}

		protected override void TearDownOldElement(ItemsView oldElement)
		{
			base.TearDownOldElement(oldElement);

			if (oldElement == null)
				return;

			if (ListViewBase != null)
			{
				ListViewBase.SizeChanged -= OnListSizeChanged;
				if (CollectionViewSource?.Source is ObservableItemTemplateCollection observableItemsSource)
					observableItemsSource.CollectionChanged -= CollectionItemsSourceChanged;

			}

			if (_scrollViewer != null)
			{
				_scrollViewer.ViewChanging -= OnScrollViewChanging;
				_scrollViewer.ViewChanged -= OnScrollViewChanged;
			}
		}

		protected override void UpdateItemsSource()
		{
			var itemsSource = Element.ItemsSource;

			if (itemsSource == null)
				return;

			var itemTemplate = Element.ItemTemplate;

			if (itemTemplate == null)
				return;

			base.UpdateItemsSource();
		}

		protected override CollectionViewSource CreateCollectionViewSource()
		{
			var collectionViewSource = TemplatedItemSourceFactory.Create(Element.ItemsSource, Element.ItemTemplate, Element,
					GetItemHeight(), GetItemWidth(), GetItemSpacing());

			if (collectionViewSource is ObservableItemTemplateCollection observableItemsSource)
				observableItemsSource.CollectionChanged += CollectionItemsSourceChanged;

			return new CollectionViewSource
			{
				Source = collectionViewSource,
				IsSourceGrouped = false
			};
		}

		protected override ListViewBase SelectListViewBase()
		{
			ListViewBase listView = CreateCarouselListLayout(CarouselItemsLayout.Orientation);

			FindScrollViewer(listView);

			return listView;
		}

		protected override void UpdateItemTemplate()
		{
			if (Element == null || ListViewBase == null)
				return;

			ListViewBase.ItemTemplate = CarouselItemsViewTemplate;
		}

		void CollectionItemsSourceChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			var carouselPosition = Carousel.Position;
			var currentItemPosition = FindCarouselItemIndex(Carousel.CurrentItem);
			var count = (sender as IList).Count;

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
				_noNeedForScroll = true;
			}
			//If we are adding a new item make sure to maintain the CurrentItemPosition
			else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add
				&& currentItemPosition != -1)
			{
				carouselPosition = currentItemPosition;
			}

			_gotoPosition = -1;

			SetCurrentItem(carouselPosition);
			UpdatePosition(carouselPosition);
		}

		void OnListSizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
		{
			UpdateItemsSource();
			UpdateSnapPointsType();
			UpdateSnapPointsAlignment();
			UpdateInitialPosition();
		}

		void OnScrollViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
		{
			Carousel.SetIsDragging(true);
			Carousel.IsScrolling = true;
		}

		void OnScrollViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
		{
			Carousel.SetIsDragging(e.IsIntermediate);
			Carousel.IsScrolling = e.IsIntermediate;
			UpdatePositionFromScroll();
			HandleScroll(_scrollViewer);
		}

		void UpdatePeekAreaInsets()
		{
			ListViewBase.Padding = new Windows.UI.Xaml.Thickness(Carousel.PeekAreaInsets.Left, Carousel.PeekAreaInsets.Top, Carousel.PeekAreaInsets.Right, Carousel.PeekAreaInsets.Bottom);
			UpdateItemsSource();
		}

		void UpdateIsSwipeEnabled()
		{
			if (Carousel == null)
				return;

			ListViewBase.IsSwipeEnabled = Carousel.IsSwipeEnabled;

			switch (CarouselItemsLayout.Orientation)
			{
				case ItemsLayoutOrientation.Horizontal:
					ScrollViewer.SetHorizontalScrollMode(ListViewBase, Carousel.IsSwipeEnabled ? WScrollMode.Auto : WScrollMode.Disabled);
					ScrollViewer.SetHorizontalScrollBarVisibility(ListViewBase, Carousel.IsSwipeEnabled ? WScrollBarVisibility.Auto : WScrollBarVisibility.Disabled);
					break;
				case ItemsLayoutOrientation.Vertical:
					ScrollViewer.SetVerticalScrollMode(ListViewBase, Carousel.IsSwipeEnabled ? WScrollMode.Auto : WScrollMode.Disabled);
					ScrollViewer.SetVerticalScrollBarVisibility(ListViewBase, Carousel.IsSwipeEnabled ? WScrollBarVisibility.Auto : WScrollBarVisibility.Disabled);
					break;
			}
		}

		void UpdateIsBounceEnabled()
		{
			if (_scrollViewer != null)
				_scrollViewer.IsScrollInertiaEnabled = Carousel.IsBounceEnabled;
		}

		void UpdateItemSpacing()
		{
			UpdateItemsSource();

			var itemSpacing = CarouselItemsLayout.ItemSpacing;
			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
				_scrollViewer.Padding = new Windows.UI.Xaml.Thickness(0, 0, itemSpacing, 0);

			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
				_scrollViewer.Padding = new Windows.UI.Xaml.Thickness(0, 0, 0, itemSpacing);
		}

		void UpdateSnapPointsType()
		{
			if (_scrollViewer == null)
				return;

			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
				_scrollViewer.HorizontalSnapPointsType = GetWindowsSnapPointsType(CarouselItemsLayout.SnapPointsType);

			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
				_scrollViewer.VerticalSnapPointsType = GetWindowsSnapPointsType(CarouselItemsLayout.SnapPointsType);
		}

		void UpdateSnapPointsAlignment()
		{
			if (_scrollViewer == null)
				return;

			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
				_scrollViewer.HorizontalSnapPointsAlignment = GetWindowsSnapPointsAlignment(CarouselItemsLayout.SnapPointsAlignment);

			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
				_scrollViewer.VerticalSnapPointsAlignment = GetWindowsSnapPointsAlignment(CarouselItemsLayout.SnapPointsAlignment);
		}

		void UpdateInitialPosition()
		{
			int position;

			if (Carousel.CurrentItem != null)
			{
				position = FindCarouselItemIndex(Carousel.CurrentItem);
				Carousel.Position = position;
			}
			else
			{
				position = Carousel.Position;
			}

			SetCurrentItem(position);
			UpdatePosition(position);
			//if (position > 0)
			//	UpdateFromPosition();
		}

		void UpdatePositionFromScroll()
		{
			if (_scrollViewer == null)
				return;

			int itemCount = CollectionViewSource.View.Count;

			if (itemCount == 0)
				return;

			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal && _scrollViewer.HorizontalOffset == _previousHorizontalOffset)
				return;

			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Vertical && _scrollViewer.VerticalOffset == _previousVerticalOffset)
				return;

			bool goingNext = CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal ?
										_scrollViewer.HorizontalOffset > _previousHorizontalOffset :
										_scrollViewer.VerticalOffset > _previousVerticalOffset;

			var position = ListViewBase.GetVisibleIndexes(CarouselItemsLayout.Orientation, goingNext).centerItemIndex;
			if (position == -1)
				return;
			System.Diagnostics.Debug.WriteLine(position);
			UpdatePosition(position);
		}

		void UpdateFromCurrentItem()
		{
			var currentItemPosition = FindCarouselItemIndex(Carousel.CurrentItem);
			var carouselPosition = Carousel.Position;

			if (_gotoPosition == -1 && currentItemPosition != carouselPosition)
			{
				_gotoPosition = currentItemPosition;
				Carousel.ScrollTo(currentItemPosition, position: ScrollToPosition.Center, animate: Carousel.AnimateCurrentItemChanges);
			}
		}

		void UpdateFromPosition()
		{
			var itemCount = CollectionViewSource.View.Count;
			var carouselPosition = Carousel.Position;

			if (carouselPosition >= itemCount || carouselPosition < 0)
				throw new IndexOutOfRangeException($"Can't set CarouselView to position {carouselPosition}. ItemsSource has {itemCount} items.");

			if (carouselPosition == _gotoPosition)
				_gotoPosition = -1;

			if (_noNeedForScroll)
			{
				_noNeedForScroll = false;
				return;
			}

			if (_gotoPosition == -1 && !Carousel.IsDragging && !Carousel.IsScrolling)
			{
				_gotoPosition = carouselPosition;
				Carousel.ScrollTo(carouselPosition, position: ScrollToPosition.Center, animate: Carousel.AnimatePositionChanges);
			}
			SetCurrentItem(carouselPosition);
		}

		void UpdatePosition(int position)
		{
			if (!ValidatePosition(position))
				return;

			var carouselPosition = Carousel.Position;

			//we arrived center
			if (position == _gotoPosition)
				_gotoPosition = -1;

			if (_gotoPosition == -1 && carouselPosition != position)
				Carousel.SetValueFromRenderer(CarouselView.PositionProperty, position);
		}

		void SetCurrentItem(int carouselPosition)
		{
			if (ListViewBase.Items.Count == 0)
				return;

			if (!ValidatePosition(carouselPosition))
				return;

			if (!(ListViewBase.Items[carouselPosition] is ItemTemplateContext itemTemplateContext))
				throw new InvalidOperationException("Visible item not found");

			var item = itemTemplateContext.Item;
			Carousel.SetValueFromRenderer(CarouselView.CurrentItemProperty, item);
		}

		bool ValidatePosition(int position)
		{
			if (ListViewBase.Items.Count == 0)
				return false;

			if (position < 0 || position >= ListViewBase.Items.Count)
				return false;

			return true;
		}

		ListViewBase CreateCarouselListLayout(ItemsLayoutOrientation layoutOrientation)
		{
			Windows.UI.Xaml.Controls.ListView listView;

			if (layoutOrientation == ItemsLayoutOrientation.Horizontal)
			{
				listView = new FormsListView()
				{
					Style = (Windows.UI.Xaml.Style)UWPApp.Current.Resources["HorizontalCarouselListStyle"],
					ItemsPanel = (ItemsPanelTemplate)UWPApp.Current.Resources["HorizontalListItemsPanel"]
				};
			}
			else
			{
				listView = new FormsListView()
				{
					Style = (Windows.UI.Xaml.Style)UWPApp.Current.Resources["VerticalCarouselListStyle"]
				};
			}

			listView.Padding = new Windows.UI.Xaml.Thickness(Carousel.PeekAreaInsets.Left, Carousel.PeekAreaInsets.Top, Carousel.PeekAreaInsets.Right, Carousel.PeekAreaInsets.Bottom);

			return listView;
		}

		double GetItemWidth()
		{
			var itemWidth = ActualWidth;

			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
			{
				itemWidth = (ActualWidth - Carousel.PeekAreaInsets.Left - Carousel.PeekAreaInsets.Right);
			}

			return Math.Max(itemWidth, 0);
		}

		double GetItemHeight()
		{
			var itemHeight = ActualHeight;

			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
			{
				itemHeight = (ActualHeight - Carousel.PeekAreaInsets.Top - Carousel.PeekAreaInsets.Bottom);
			}

			return Math.Max(itemHeight, 0);
		}

		Thickness GetItemSpacing()
		{
			var itemSpacing = CarouselItemsLayout.ItemSpacing;

			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
				return new Thickness(itemSpacing, 0, 0, 0);

			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
				return new Thickness(0, itemSpacing, 0, 0);

			return new Thickness(0);
		}

		int FindCarouselItemIndex(object item)
		{
			for (int n = 0; n < CollectionViewSource?.View.Count; n++)
			{
				if (CollectionViewSource.View[n] is ItemTemplateContext pair)
				{
					if (pair.Item == item)
					{
						return n;
					}
				}
			}
			return -1;
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

		protected override void FindScrollViewer(ListViewBase listView)
		{
			var scrollViewer = listView.GetFirstDescendant<ScrollViewer>();

			if (scrollViewer != null)
			{
				_scrollViewer = scrollViewer;
				// TODO: jsuarezruiz This breaks the ScrollTo override. Review it.
				_scrollViewer.ViewChanging += OnScrollViewChanging;
				_scrollViewer.ViewChanged += OnScrollViewChanged;
				return;
			}

			void ListViewLoaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
			{
				var lv = (ListViewBase)sender;
				lv.Loaded -= ListViewLoaded;
				FindScrollViewer(listView);
			}

			listView.Loaded += ListViewLoaded;
		}
	}
}
