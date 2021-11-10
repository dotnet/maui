using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;
using UWPApp = Microsoft.UI.Xaml.Application;
using UWPDataTemplate = Microsoft.UI.Xaml.DataTemplate;
using WScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility;
using WScrollMode = Microsoft.UI.Xaml.Controls.ScrollMode;
using WSnapPointsAlignment = Microsoft.UI.Xaml.Controls.Primitives.SnapPointsAlignment;
using WSnapPointsType = Microsoft.UI.Xaml.Controls.SnapPointsType;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class CarouselViewHandler : ItemsViewHandler<CarouselView>
	{
		protected override IItemsLayout Layout => VirtualView?.ItemsLayout;
		LinearItemsLayout CarouselItemsLayout => VirtualView?.ItemsLayout;

		UWPDataTemplate CarouselItemsViewTemplate => (UWPDataTemplate)UWPApp.Current.Resources["CarouselItemsViewDefaultTemplate"];

		public static void MapIsSwipeEnabled(CarouselViewHandler handler, CarouselView carouselView)
		{

		}

		public static void MapIsBounceEnabled(CarouselViewHandler handler, CarouselView carouselView)
		{

		}

		public static void MapPeekAreaInsets(CarouselViewHandler handler, CarouselView carouselView)
		{

		}

		public static void MapPosition(CarouselViewHandler handler, CarouselView carouselView)
		{

		}

		public static void MapCurrentItem(CarouselViewHandler handler, CarouselView carouselView)
		{

		}

		protected override ListViewBase SelectListViewBase()
		{
			return CreateCarouselListLayout(CarouselItemsLayout.Orientation);
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

			listView.Padding = WinUIHelpers.CreateThickness(VirtualView.PeekAreaInsets.Left, VirtualView.PeekAreaInsets.Top, VirtualView.PeekAreaInsets.Right, VirtualView.PeekAreaInsets.Bottom);

			return listView;
		}
	}
}
