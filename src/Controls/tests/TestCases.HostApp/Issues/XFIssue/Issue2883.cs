namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 2883, "ViewCell IsEnabled set to false does not disable a cell in a TableView")]
public class Issue2883 : TestContentPage
{
	protected override void Init()
	{
		var btnCustom1 = new Button()
		{
			AutomationId = "btnCustomCellTable",
			Text = "Custom Table Cell",
			HorizontalOptions = LayoutOptions.Start
		};
		var btnCustom1Enabled = new Button()
		{
			AutomationId = "btnCustomCellTableEnabled",
			Text = "Custom Table Cell Enabled",
			HorizontalOptions = LayoutOptions.Start
		};

		var btnCustom = new Button()
		{
			AutomationId = "btnCustomCellListView",
			Text = "Custom Cell",
			HorizontalOptions = LayoutOptions.Start
		};

		var btnCustomEnabled = new Button()
		{
			AutomationId = "btnCustomCellListViewEnabled",
			Text = "Custom Cell Enabled",
			HorizontalOptions = LayoutOptions.Start
		};

		btnCustom.Clicked += (object sender, EventArgs e) =>
		{
			DisplayAlertAsync("Clicked", "I was clicked even disabled", "ok");
		};
		btnCustom1.Clicked += (object sender, EventArgs e) =>
		{
			DisplayAlertAsync("Clicked", "I was clicked even disabled", "ok");
		};

		btnCustom1Enabled.Clicked += (object sender, EventArgs e) =>
		{
			DisplayAlertAsync("Clicked", "I was clicked", "ok");
		};
		btnCustomEnabled.Clicked += (object sender, EventArgs e) =>
		{
			DisplayAlertAsync("Clicked", "I was clicked", "ok");
		};

		var customCell = new ViewCell()
		{
			IsEnabled = false,
			View = new StackLayout { Children = { btnCustom } }
		};

		var customCellEnabled = new ViewCell()
		{
			View = new StackLayout { Children = { btnCustomEnabled } }
		};

		var customTableCell = new ViewCell()
		{
			IsEnabled = false,
			View = new StackLayout { Children = { btnCustom1 } }
		};

		var customTableCellEnabled = new ViewCell()
		{
			View = new StackLayout { Children = { btnCustom1Enabled } }
		};

		var tableview = new TableView()
		{
			Intent = TableIntent.Form,
			Root = new TableRoot(),
			VerticalOptions = LayoutOptions.Start
		};

		tableview.Root.Add(new TableSection() { customTableCell, customTableCellEnabled });

		var listview = new ListView { VerticalOptions = LayoutOptions.Start };
		var listview2 = new ListView { VerticalOptions = LayoutOptions.Start };

		listview.ItemTemplate = new DataTemplate(() => customCell);
		listview2.ItemTemplate = new DataTemplate(() => customCellEnabled);
		listview2.ItemsSource = listview.ItemsSource = new List<string>() { "1" };

		Content = new StackLayout
		{
			Orientation = StackOrientation.Vertical,
			Children = { tableview, listview, listview2 }
		};
	}
}
