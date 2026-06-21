namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 20177, "Shell TitleColor changes the secondary ToolbarItems TextColor", PlatformAffected.UWP)]
public class Issue20177 : TestShell
{
	protected override void Init()
	{
		Shell.SetTitleColor(this, Colors.White);
		AddContentPage(CreateContentPage());
	}

	ContentPage CreateContentPage()
	{
		var page = new ContentPage();
		PopulateToolBarItems(page);
		page.Content = CreateGrid();
		return page;
	}

	Grid CreateGrid()
	{
		Grid grid = new Grid()
			{
				new Label()
				{
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions= LayoutOptions.Center,
					Text = "Secondary ToolBar Items should not use BarTextColor",
					AutomationId = "DescriptionLabel"
				}
			};
		return grid;
	}

	void PopulateToolBarItems(ContentPage page)
	{
		page.ToolbarItems.Add(new()
		{
			Text = "Menu item",
			Order = ToolbarItemOrder.Primary
		});
		page.ToolbarItems.Add(new()
		{
			Text = "Menu item 1",
			Order = ToolbarItemOrder.Secondary
		});
		page.ToolbarItems.Add(new()
		{
			Text = "Menu item 2",
			Order = ToolbarItemOrder.Secondary
		});
		page.ToolbarItems.Add(new()
		{
			Text = "Menu item 3",
			Order = ToolbarItemOrder.Secondary
		});
	}
}