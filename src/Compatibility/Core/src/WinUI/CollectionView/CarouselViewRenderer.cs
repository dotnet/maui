using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Hosting;
using UWPApp = Microsoft.UI.Xaml.Application;
using UWPDataTemplate = Microsoft.UI.Xaml.DataTemplate;
using WScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility;
using WScrollMode = Microsoft.UI.Xaml.Controls.ScrollMode;
using WSnapPointsAlignment = Microsoft.UI.Xaml.Controls.Primitives.SnapPointsAlignment;
using WSnapPointsType = Microsoft.UI.Xaml.Controls.SnapPointsType;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class CarouselViewRenderer : ItemsViewRenderer<CarouselView>
	{
		ScrollViewer _scrollViewer;
		WScrollBarVisibility? _horizontalScrollBarVisibilityWithoutLoop;
		WScrollBarVisibility? _verticalScrollBarVisibilityWithoutLoop;

		public CarouselViewRenderer()
		{
		}

		protected CarouselView CarouselView => Element;
		protected override IItemsLayout Layout => CarouselView?.ItemsLayout;
		LinearItemsLayout CarouselItemsLayout => CarouselView?.ItemsLayout;

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
			else if (changedProperty.Is(CarouselView.LoopProperty))
				UpdateLoop();
		}

		protected virtual void UpdateLoop()
		{
			UpdateScrollBarVisibilityForLoop();
			UpdateItemsSource();
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
				ListViewBase.SizeChanged += InitialSetup;
				newElement.Scrolled += CarouselScrolled;
				UpdateScrollBarVisibilityForLoop();
			}
		}

		protected override void TearDownOldElement(ItemsView oldElement)
		{
			base.TearDownOldElement(oldElement);

			if (oldElement == null)
				return;

			oldElement.Scrolled -= CarouselScrolled;

			if (ListViewBase != null)
			{
				ListViewBase.SizeChanged -= InitialSetup;
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

		protected override ICollectionView GetCollectionView(CollectionViewSource collectionViewSource)
		{
			_view = new LoopableCollectionView(base.GetCollectionView(collectionViewSource));

			if (Element is CarouselView cv && cv.Loop)
			{
				_view.IsLoopingEnabled = true;
			}

			return _view;
		}

		protected override ListViewBase SelectListViewBase()
		{
			return CreateCarouselListLayout(CarouselItemsLayout.Orientation);
		}

		protected override void UpdateItemTemplate()
		{
			if (Element == null || ListViewBase == null)
				return;

			ListViewBase.ItemTemplate = CarouselItemsViewTemplate;
		}

		protected override ItemsViewScrolledEventArgs ComputeVisibleIndexes(ItemsViewScrolledEventArgs args, ItemsLayoutOrientation orientation, bool advancing)
		{
			args = base.ComputeVisibleIndexes(args, orientation, advancing);

			if (Element.Loop)
			{
				args.FirstVisibleItemIndex %= ItemCount;
				args.CenterItemIndex %= ItemCount;
				args.LastVisibleItemIndex %= ItemCount;
			}

			return args;
		}

		void CollectionItemsSourceChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			var carouselPosition = CarouselView.Position;
			var currentItemPosition = GetItemPositionInCarousel(CarouselView.CurrentItem);
			var count = (sender as IList).Count;

			bool removingCurrentElement = currentItemPosition == -1;
			bool removingLastElement = e.OldStartingIndex == count;
			bool removingFirstElement = e.OldStartingIndex == 0;
			bool removingCurrentElementButNotFirst = removingCurrentElement && removingLastElement && CarouselView.Position > 0;

			if (removingCurrentElementButNotFirst)
			{
				carouselPosition = CarouselView.Position - 1;

			}
			else if (removingFirstElement && !removingCurrentElement)
			{
				carouselPosition = currentItemPosition;
			}
			//If we are adding a new item make sure to maintain the CurrentItemPosition
			else if (e.Action == NotifyCollectionChangedAction.Add
				&& currentItemPosition != -1)
			{
				carouselPosition = currentItemPosition;
			}

			SetCarouselViewCurrentItem(carouselPosition);
			SetCarouselViewPosition(carouselPosition);
		}

		void OnScrollViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
		{
			CarouselView.SetIsDragging(true);
			CarouselView.IsScrolling = true;
		}

		private LoopableCollectionView _view;

		void OnScrollViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
		{
			CarouselView.SetIsDragging(e.IsIntermediate);
			CarouselView.IsScrolling = e.IsIntermediate;
		}

		void UpdatePeekAreaInsets()
		{
			ListViewBase.Padding = WinUIHelpers.CreateThickness(CarouselView.PeekAreaInsets.Left, CarouselView.PeekAreaInsets.Top, CarouselView.PeekAreaInsets.Right, CarouselView.PeekAreaInsets.Bottom);
			UpdateItemsSource();
		}

		void UpdateIsSwipeEnabled()
		{
			if (CarouselView == null)
				return;

			ListViewBase.IsSwipeEnabled = CarouselView.IsSwipeEnabled;

			switch (CarouselItemsLayout.Orientation)
			{
				case ItemsLayoutOrientation.Horizontal:
					ScrollViewer.SetHorizontalScrollMode(ListViewBase, CarouselView.IsSwipeEnabled ? WScrollMode.Auto : WScrollMode.Disabled);
					ScrollViewer.SetHorizontalScrollBarVisibility(ListViewBase, CarouselView.IsSwipeEnabled ? WScrollBarVisibility.Auto : WScrollBarVisibility.Disabled);
					break;
				case ItemsLayoutOrientation.Vertical:
					ScrollViewer.SetVerticalScrollMode(ListViewBase, CarouselView.IsSwipeEnabled ? WScrollMode.Auto : WScrollMode.Disabled);
					ScrollViewer.SetVerticalScrollBarVisibility(ListViewBase, CarouselView.IsSwipeEnabled ? WScrollBarVisibility.Auto : WScrollBarVisibility.Disabled);
					break;
			}
		}

		void UpdateIsBounceEnabled()
		{
			if (_scrollViewer != null)
				_scrollViewer.IsScrollInertiaEnabled = CarouselView.IsBounceEnabled;
		}

		void UpdateItemSpacing()
		{
			UpdateItemsSource();

			var itemSpacing = CarouselItemsLayout.ItemSpacing;
			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
				_scrollViewer.Padding = WinUIHelpers.CreateThickness(0, 0, itemSpacing, 0);

			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
				_scrollViewer.Padding = WinUIHelpers.CreateThickness(0, 0, 0, itemSpacing);
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

		void UpdateCarouselViewInitialPosition()
		{
			if (Element.Loop && ListViewBase.Items.Count > 0)
			{
				var item = ListViewBase.Items[0];
				_view.CenterMode = true;
				ListViewBase.ScrollIntoView(item);
				_view.CenterMode = false;
			}

			if (CarouselView.CurrentItem != null)
			{
				UpdateFromCurrentItem();
			}
			else
			{
				UpdateFromPosition();
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

		void UpdateFromCurrentItem()
		{
			var currentItemPosition = GetItemPositionInCarousel(CarouselView.CurrentItem);
			CarouselView.ScrollTo(currentItemPosition, position: ScrollToPosition.Center, animate: CarouselView.AnimateCurrentItemChanges);
		}

		void UpdateFromPosition()
		{
			if (CollectionViewSource == null)
				return;

			var carouselPosition = CarouselView.Position;

			if (carouselPosition >= ItemCount || carouselPosition < 0)
				throw new IndexOutOfRangeException($"Can't set CarouselView to position {carouselPosition}. ItemsSource has {ItemCount} items.");

			SetCarouselViewCurrentItem(carouselPosition);
		}

		void SetCarouselViewPosition(int position)
		{
			if (ItemCount == 0)
			{
				return;
			}

			if (!IsValidPosition(position))
				return;

			var currentPosition = CarouselView.Position;

			if (currentPosition != position)
			{
				CarouselView.SetValueFromRenderer(CarouselView.PositionProperty, position);
			}
		}

		void SetCarouselViewCurrentItem(int carouselPosition)
		{
			if (!IsValidPosition(carouselPosition))
				return;

			if (!(GetItem(carouselPosition) is ItemTemplateContext itemTemplateContext))
				throw new InvalidOperationException("Visible item not found");

			var item = itemTemplateContext.Item;
			CarouselView.SetValueFromRenderer(CarouselView.CurrentItemProperty, item);
		}

		bool IsValidPosition(int position)
		{
			if (ItemCount == 0)
				return false;

			if (position < 0 || position >= ItemCount)
				return false;

			return true;
		}

		ListViewBase CreateCarouselListLayout(ItemsLayoutOrientation layoutOrientation)
		{
			Microsoft.UI.Xaml.Controls.ListView listView;

			if (layoutOrientation == ItemsLayoutOrientation.Horizontal)
			{
				listView = new FormsListView()
				{
					Style = (Microsoft.UI.Xaml.Style)UWPApp.Current.Resources["HorizontalCarouselListStyle"],
					ItemsPanel = (ItemsPanelTemplate)UWPApp.Current.Resources["HorizontalListItemsPanel"]
				};

				ScrollViewer.SetHorizontalScrollBarVisibility(listView, WScrollBarVisibility.Auto);
				ScrollViewer.SetVerticalScrollBarVisibility(listView, WScrollBarVisibility.Disabled);
			}
			else
			{
				listView = new FormsListView()
				{
					Style = (Microsoft.UI.Xaml.Style)UWPApp.Current.Resources["VerticalCarouselListStyle"]
				};

				ScrollViewer.SetHorizontalScrollBarVisibility(listView, WScrollBarVisibility.Disabled);
				ScrollViewer.SetVerticalScrollBarVisibility(listView, WScrollBarVisibility.Auto);
			}

			listView.Padding = WinUIHelpers.CreateThickness(CarouselView.PeekAreaInsets.Left, CarouselView.PeekAreaInsets.Top, CarouselView.PeekAreaInsets.Right, CarouselView.PeekAreaInsets.Bottom);

			return listView;
		}

		double GetItemWidth()
		{
			var itemWidth = ActualWidth;

			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
			{
				itemWidth = (ActualWidth - CarouselView.PeekAreaInsets.Left - CarouselView.PeekAreaInsets.Right);
			}

			return Math.Max(itemWidth, 0);
		}

		double GetItemHeight()
		{
			var itemHeight = ActualHeight;

			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
			{
				itemHeight = (ActualHeight - CarouselView.PeekAreaInsets.Top - CarouselView.PeekAreaInsets.Bottom);
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

		protected override void OnScrollViewerFound(ScrollViewer scrollViewer)
		{
			base.OnScrollViewerFound(scrollViewer);

			_scrollViewer = scrollViewer;
			_scrollViewer.ViewChanging += OnScrollViewChanging;
			_scrollViewer.ViewChanged += OnScrollViewChanged;
			_scrollViewer.SizeChanged += InitialSetup;

			UpdateScrollBarVisibilityForLoop();
		}

		void InitialSetup(object sender, Microsoft.UI.Xaml.SizeChangedEventArgs e)
		{
			if (e.NewSize.Width > 0 && e.NewSize.Height > 0)
			{
				ListViewBase.SizeChanged -= InitialSetup;
				_scrollViewer.SizeChanged -= InitialSetup;

				UpdateItemsSource();
				UpdateSnapPointsType();
				UpdateSnapPointsAlignment();
				UpdateCarouselViewInitialPosition();
			}
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
	}
}
