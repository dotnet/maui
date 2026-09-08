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
			new SectionModel(typeof(ListViewPage), "ListView",
				"ListView derives from ItemsView and displays a scrollable list of selectable data items."),

			new SectionModel(typeof(TableViewPage), "TableView",
				"TableView displays a list of rows of type Cell with optional headers and subheaders. Set the Root property to an object of type TableRoot, and add TableSection objects to that TableRoot. Each TableSection is a collection of Cell objects."),

			new SectionModel(typeof(TabbedPageGallery), "TabbedPage",
				"Display pages as a set of Tabs."),

			new SectionModel(typeof(AndExpandPage), "AndExpand",
				"StackLayout with legacy AndExpand options"),
		};
	}
}