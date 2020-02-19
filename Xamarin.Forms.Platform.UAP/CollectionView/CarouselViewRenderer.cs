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
using WScrollMode = Windows.UI.Xaml.Controls.ScrollMode;

namespace Xamarin.Forms.Platform.UWP
{
	public class CarouselViewRenderer : ItemsViewRenderer<CarouselView>
	{
		ScrollViewer _scrollViewer;

		public CarouselViewRenderer()
		{
			CarouselView.VerifyCarouselViewFlagEnabled(nameof(CarouselView));
		}

		CarouselView CarouselView => Element;
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
					GetItemHeight(), GetItemWidth(), GetItemSpacing()),
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

		protected override Task ScrollTo(ScrollToRequestEventArgs args)
		{
			var targetItem = FindCarouselItem(args);

			// TODO: jsuarezruiz Include support to animated scroll.
			ListViewBase.ScrollIntoView(targetItem);

			return Task.FromResult<object>(null);
		}

		void OnListSizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
		{
			UpdateItemsSource();
			UpdateSnapPointsType();
			UpdateSnapPointsAlignment();
		}

		void OnScrollViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
		{
			CarouselView.SetIsDragging(true);
			CarouselView.IsScrolling = true;
		}

		void OnScrollViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
		{
			CarouselView.SetIsDragging(e.IsIntermediate);
			CarouselView.IsScrolling = e.IsIntermediate;

			UpdatePositionFromScroll();
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

		void UpdatePositionFromScroll()
		{
			if (_scrollViewer == null)
				return;

			int itemCount = CollectionViewSource.View.Count;

			if (itemCount == 0)
				return;

			int position;

			if (_scrollViewer.HorizontalOffset > _scrollViewer.VerticalOffset)
				position = _scrollViewer.ScrollableWidth > 0 ? Convert.ToInt32(Math.Ceiling(_scrollViewer.HorizontalOffset * itemCount / _scrollViewer.ScrollableWidth)) : 0;
			else
				position = _scrollViewer.ScrollableHeight > 0 ? Convert.ToInt32(Math.Ceiling(_scrollViewer.VerticalOffset * itemCount / _scrollViewer.ScrollableHeight)) : 0;

			UpdatePosition(position);
		}

		void UpdatePosition(int position)
		{
			if (position <= 0)
				return;

			if (!(ListViewBase.Items[position - 1] is ItemTemplateContext itemTemplateContext))
				throw new InvalidOperationException("Visible item not found");

			CarouselView.SetCurrentItem(itemTemplateContext.Item);
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

			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
			{
				itemWidth = (ActualWidth - CarouselView.PeekAreaInsets.Left - CarouselView.PeekAreaInsets.Right - CarouselItemsLayout.ItemSpacing);
			}

			return Math.Max(itemWidth, 0);
		}

		double GetItemHeight()
		{
			var itemHeight = ActualHeight;

			if (CarouselItemsLayout.Orientation == ItemsLayoutOrientation.Vertical)
			{
				itemHeight = (ActualHeight - CarouselView.PeekAreaInsets.Top - CarouselView.PeekAreaInsets.Bottom - CarouselItemsLayout.ItemSpacing);
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