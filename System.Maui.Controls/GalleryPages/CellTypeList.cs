using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
	internal class CellTypesListPage : ContentPage
	{
		public CellTypesListPage ()
		{
			Content = new CellTypeList ();
		}
	}

	[Preserve (AllMembers = true)]
	public class CellNavigation
	{
		public string CellType { get; set; }
		public ContentPage Page { get; set; }

		public CellNavigation (string type, ContentPage page)
		{
			CellType = type;
			Page = page;
		}
	}

	public class CellTypeList : ListView
	{
		CellNavigation _last;

		public const string CellTestContainerId = "CellTestContainer";

		// TODO Add gallerys for ViewCell, ListView and TableView
		public CellTypeList ()
		{
			var itemList = new List<CellNavigation> {
				new CellNavigation ("TextCell List", new TextCellListPage ()),
				new CellNavigation ("TextCell Table", new TextCellTablePage ()),
				new CellNavigation ("ImageCell List", new ImageCellListPage ()),
				new CellNavigation ("ImageCell Url List", new UrlImageCellListPage()),
				new CellNavigation ("ImageCell Table", new ImageCellTablePage ()),
				new CellNavigation ("SwitchCell List", new SwitchCellListPage ()),
				new CellNavigation ("SwitchCell Table", new SwitchCellTablePage ()),
				new CellNavigation ("EntryCell List", new EntryCellListPage ()),
				new CellNavigation ("EntryCell Table", new EntryCellTablePage ()),
				new CellNavigation ("ViewCell Image url table", new UrlImageViewCellListPage())
			};
			
			ItemsSource = itemList;

			var template = new DataTemplate (typeof (TextCell));
			template.SetBinding (TextCell.TextProperty, new Binding ("CellType"));

			ItemTemplate = template;
			ItemSelected += (s, e) => {
				
				if (SelectedItem == null)
					return;

				var cellNav = (CellNavigation) e.SelectedItem;

				if (cellNav == _last)
				{
					_last = null;
					return;
				}

				Navigation.PushAsync (cellNav.Page);
				_last = cellNav;
#if !__WINDOWS__
				SelectedItem = null;
#endif
			};
		}		
	}
}
