using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using UWPApp = Windows.UI.Xaml.Application;
using UWPDataTemplate = Windows.UI.Xaml.DataTemplate;
using WScrollBarVisibility = Windows.UI.Xaml.Controls.ScrollBarVisibility;
using WSnapPointsType = Windows.UI.Xaml.Controls.SnapPointsType;
using WSnapPointsAlignment = Windows.UI.Xaml.Controls.Primitives.SnapPointsAlignment;

namespace Xamarin.Forms.Platform.UWP
{
	public class CarouselViewRenderer : ItemsViewRenderer
	{
		ScrollViewer _scrollViewer;
		public CarouselViewRenderer()
		{
			CollectionView.VerifyCollectionViewFlagEnabled(nameof(CarouselView));
		}

		CarouselView CarouselView => (CarouselView)Element;
		protected override IItemsLayout Layout => CarouselView?.ItemsLayout;
		UWPDataTemplate CarouselItemsViewTemplate => (UWPDataTemplate)UWPApp.Current.Resources["CarouselItemsViewDefaultTemplate"];

		double _itemWidth;
		double _itemHeight;

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs changedProperty)
		{
			base.OnElementPropertyChanged(sender, changedProperty);

			if (changedProperty.IsOneOf(ItemsView.ItemsSourceProperty, CarouselView.NumberOfSideItemsProperty, LinearItemsLayout.ItemSpacingProperty))
				UpdateItemsSource();
			else if (changedProperty.Is(ItemsView.ItemTemplateProperty))
				UpdateItemTemplate();
			else if (changedProperty.Is(CarouselView.PeekAreaInsetsProperty))
				UpdatePeekAreaInsets();
			else if (changedProperty.Is(CarouselView.IsSwipeEnabledProperty))
				UpdateIsSwipeEnabled();
			else if (changedProperty.Is(CarouselView.IsBounceEnabledProperty))
				UpdateIsBounceEnabled();
		}

		protected override void HandleLayoutPropertyChange(PropertyChangedEventArgs property)
		{
			if (property.IsOneOf(LinearItemsLayout.ItemSpacingProperty, GridItemsLayout.HorizontalItemSpacingProperty, GridItemsLayout.VerticalItemSpacingProperty))
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
			return new CollectionViewSource
			{
				Source = TemplatedItemSourceFactory.Create(Element.ItemsSource, Element.ItemTemplate, Element, 
					_itemHeight, _itemWidth, GetItemSpacing()),
				IsSourceGrouped = false
			};
		}

		protected override ListViewBase SelectListViewBase()
		{
			ListViewBase listView = null;

			switch (Layout)
			{
				case LinearItemsLayout listItemsLayout:
					listView = CreateCarouselListLayout(listItemsLayout.Orientation);
					break;
			}

			if (listView == null)
			{
				listView = new FormsListView();
			}

			FindScrollViewer(listView);

			return listView;
		}

		protected override void UpdateItemTemplate()
		{
			if (Element == null || ListViewBase == null)
				return;

			ListViewBase.ItemTemplate = CarouselItemsViewTemplate;
		}

		protected override Task ScrollTo(ScrollToRequestEventArgs args)
		{
			var targetItem = FindCarouselItem(args);

			// TODO: jsuarezruiz Include support to animated scroll.
			ListViewBase.ScrollIntoView(targetItem);

			return Task.FromResult<object>(null);
		}

		void OnListSizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
		{
			_itemHeight = GetItemHeight();
			_itemWidth = GetItemWidth();
			UpdateItemsSource();
		}

		void OnScrollViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
		{
			CarouselView.SetIsDragging(true);
		}

		void OnScrollViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
		{
			CarouselView.SetIsDragging(e.IsIntermediate);
		}

		void UpdatePeekAreaInsets()
		{
			UpdateItemsSource();
		}

		void UpdateIsSwipeEnabled()
		{
			if (CarouselView == null)
				return;

			ListViewBase.IsSwipeEnabled = CarouselView.IsSwipeEnabled;

			switch (Layout)
			{
				case LinearItemsLayout listItemsLayout when listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal:
					ScrollViewer.SetHorizontalScrollMode(ListViewBase, CarouselView.IsSwipeEnabled ? ScrollMode.Auto : ScrollMode.Disabled);
					ScrollViewer.SetHorizontalScrollBarVisibility(ListViewBase, CarouselView.IsSwipeEnabled ? WScrollBarVisibility.Auto : WScrollBarVisibility.Disabled);
					break;
				case LinearItemsLayout listItemsLayout when listItemsLayout.Orientation == ItemsLayoutOrientation.Vertical:
					ScrollViewer.SetVerticalScrollMode(ListViewBase, CarouselView.IsSwipeEnabled ? ScrollMode.Auto : ScrollMode.Disabled);
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

			if (Layout is LinearItemsLayout listItemsLayout)
			{
				var itemSpacing = listItemsLayout.ItemSpacing;
				if (listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
					_scrollViewer.Padding = new Windows.UI.Xaml.Thickness(0, 0, itemSpacing, 0);

				if (listItemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
					_scrollViewer.Padding = new Windows.UI.Xaml.Thickness(0, 0, 0, itemSpacing);
			}
		}

		void UpdateSnapPointsType()
		{
			if (_scrollViewer == null)
				return;

			if (Layout is LinearItemsLayout listItemsLayout)
			{
				if (listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
					_scrollViewer.HorizontalSnapPointsType = GetWindowsSnapPointsType(listItemsLayout.SnapPointsType);

				if (listItemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
					_scrollViewer.VerticalSnapPointsType = GetWindowsSnapPointsType(listItemsLayout.SnapPointsType);
			}
		}

		void UpdateSnapPointsAlignment()
		{
			if (_scrollViewer == null)
				return;

			if (Layout is LinearItemsLayout listItemsLayout)
			{
				if (listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
					_scrollViewer.HorizontalSnapPointsAlignment = GetWindowsSnapPointsAlignment(listItemsLayout.SnapPointsAlignment);

				if (listItemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
					_scrollViewer.VerticalSnapPointsAlignment = GetWindowsSnapPointsAlignment(listItemsLayout.SnapPointsAlignment);
			}
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

			return listView;
		}

		double GetItemWidth()
		{
			var itemWidth = ActualWidth;

			if (Layout is LinearItemsLayout listItemsLayout && listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
			{
				var numberOfVisibleItems = CarouselView.NumberOfSideItems * 2 + 1;
				itemWidth = (ActualWidth - CarouselView.PeekAreaInsets.Left - CarouselView.PeekAreaInsets.Right - listItemsLayout.ItemSpacing) / numberOfVisibleItems;
			}

			return Math.Max(itemWidth, 0);
		}

		double GetItemHeight()
		{
			var itemHeight = ActualHeight;

			if (Layout is LinearItemsLayout listItemsLayout && listItemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
			{
				var numberOfVisibleItems = CarouselView.NumberOfSideItems * 2 + 1;
				itemHeight = (ActualHeight - CarouselView.PeekAreaInsets.Top - CarouselView.PeekAreaInsets.Bottom - listItemsLayout.ItemSpacing) / numberOfVisibleItems;
			}

			return Math.Max(itemHeight, 0);
		}

		Thickness GetItemSpacing()
		{
			if (Layout is LinearItemsLayout listItemsLayout)
			{
				var itemSpacing = listItemsLayout.ItemSpacing;

				if (listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
					return new Thickness(itemSpacing, 0, 0, 0);

				if (listItemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
					return new Thickness(0, itemSpacing, 0, 0);
			}

			return new Thickness(0);
		}

		object FindCarouselItem(ScrollToRequestEventArgs args)
		{
			if (args.Mode == ScrollToMode.Position)
				return CollectionViewSource.View[args.Index];

			if (Element.ItemTemplate == null)
				return args.Item;

			for (int n = 0; n < CollectionViewSource?.View.Count; n++)
			{
				if (CollectionViewSource.View[n] is ItemTemplateContext pair)
				{
					if (pair.Item == args.Item)
					{
						return CollectionViewSource.View[n];
					}
				}
			}

			return null;
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

		void FindScrollViewer(ListViewBase listView)
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