namespace Xamarin.Forms.Controls
{
	public class TextCellTablePage : ContentPage
	{

		public TextCellTablePage()
		{
			Title = "TextCell Table Gallery - Legacy";

			if (Device.RuntimePlatform == Device.iOS && Device.Idiom == TargetIdiom.Tablet)
				Padding = new Thickness(0, 0, 0, 60);

			var tableSection = new TableSection("Section One") {
				new TextCell { Text = "Text 1" },
				new TextCell { Text = "Text 2", Detail = "Detail 1" },
				new TextCell { Text = "Text 3" },
				new TextCell { Text = "Text 4", Detail = "Detail 2" },
				new TextCell { Text = "Text 5" },
				new TextCell { Text = "Text 6", Detail = "Detail 3" },
				new TextCell { Text = "Text 7" },
				new TextCell { Text = "Text 8", Detail = "Detail 4" },
				new TextCell { Text = "Text 9" },
				new TextCell { Text = "Text 10", Detail = "Detail 5" },
				new TextCell { Text = "Text 11" },
				new TextCell { Text = "Text 12", Detail = "Detail 6" }
			};

			var tableSectionTwo = new TableSection("Section Two") {
				new TextCell { Text = "Text 13" },
				new TextCell { Text = "Text 14", Detail = "Detail 7" },
				new TextCell { Text = "Text 15" },
				new TextCell { Text = "Text 16", Detail = "Detail 8" },
				new TextCell { Text = "Text 17" },
				new TextCell { Text = "Text 18", Detail = "Detail 9" },
				new TextCell { Text = "Text 19" },
				new TextCell { Text = "Text 20", Detail = "Detail 10" },
				new TextCell { Text = "Text 21" },
				new TextCell { Text = "Text 22", Detail = "Detail 11" },
				new TextCell { Text = "Text 23" },
				new TextCell { Text = "Text 24", Detail = "Detail 12" }
			};

			var root = new TableRoot("Text Cell table") {
				tableSection,
				tableSectionTwo
			};

			var table = new TableView
			{
				AutomationId = CellTypeList.CellTestContainerId,
				Root = root,
			};

			Content = table;
		}
	}
}