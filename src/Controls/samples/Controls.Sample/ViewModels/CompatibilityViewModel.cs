using System.Collections.Generic;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Pages;
using Maui.Controls.Sample.ViewModels.Base;

namespace Maui.Controls.Sample.ViewModels
{
	public class CompatibilityViewModel : BaseGalleryViewModel
	{
		protected override IEnumerable<SectionModel> CreateItems() => new[]
		{
			new SectionModel(typeof(BoxViewPage), "BoxView",
				"BoxView renders a simple rectangle of a specified width, height, and color. You can use BoxView for decoration, rudimentary graphics, and for interaction with the user through touch."),

			new SectionModel(typeof(ImageButtonPage), "ImageButton",
				"ImageButton is a rectangular object that displays an image, and which fires a Clicked event when it's been pressed."),

			new SectionModel(typeof(CollectionViewPage), "CollectionView",
				"CollectionView displays a scrollable list of selectable data items, using different layout specifications. It aims to provide a more flexible, and performant alternative to ListView. "),

			new SectionModel(typeof(CarouselViewPage), "CarouselView",
				"CarouselView displays a scrollable list of data items."),

			new SectionModel(typeof(FramePage), "Frame",
				"The Frame class derives from ContentView and displays a border, or frame, around its child."),

			new SectionModel(typeof(IndicatorViewPage), "IndicatorView",
				"IndicatorView displays indicators that represent the number of items in a CarouselView. Set the CarouselView.IndicatorView property to the IndicatorView object to display indicators for the CarouselView."),

			new SectionModel(typeof(ListViewPage), "ListView",
				"ListView derives from ItemsView and displays a scrollable list of selectable data items."),

			new SectionModel(typeof(SwipeViewPage), "SwipeView",
				"RefreshView is a container control that provides pull-to-refresh functionality for scrollable content."),

			new SectionModel(typeof(TableViewPage), "TableView",
				"TableView displays a list of rows of type Cell with optional headers and subheaders. Set the Root property to an object of type TableRoot, and add TableSection objects to that TableRoot. Each TableSection is a collection of Cell objects."),

			new SectionModel(typeof(WebViewPage), "WebView",
				"WebView displays Web pages or HTML content."),
		};
	}
}