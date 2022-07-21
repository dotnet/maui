using System.Collections.Generic;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Pages;
using Maui.Controls.Sample.Pages.ListViewGalleries;
using Maui.Controls.Sample.ViewModels.Base;

namespace Maui.Controls.Sample.ViewModels
{
	public class ListViewModel : BaseGalleryViewModel
	{
		protected override IEnumerable<SectionModel> CreateItems() => new[]
		{
			new SectionModel(typeof(ListViewEntryCell), "Entry Cell",
				"EntryCell controls are used to present text data that the user can edit."),
			new SectionModel(typeof(ListViewImageCell), "Image Cell",
				"ImageCell controls are similar to TextCells but include an image to the left of the text."),
			new SectionModel(typeof(ListViewRefresh), "RefreshView",
				"Pull-to-refresh allows the user to pull the ListView down to refresh the contents"),
			new SectionModel(typeof(ListViewSwitchCell), "Switch Cell",
				"SwitchCell controls are used to present and capture on/off or true/false states."),
			new SectionModel(typeof(ListViewTextCell), "Text Cell",
				"TextCell controls are used for displaying text with an optional second line for detail text."),
			new SectionModel(typeof(ListViewViewCell), "View Cell",
				"A ViewCell is a cell that can be added to a ListView or TableView, which contains a developer-defined view."),
			new SectionModel(typeof(ListViewContextActions), "Context actions / menus",
				"Context actions on view cells enable additional actions on a cell."),
		};
	}
}
