namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 0, "Shell Title View Tests",
	PlatformAffected.All)]
public class ShellTitleView : TestShell
{
	protected override void Init()
	{
		SetupMeasuringTest1();
		SetupMeasuringTest2();
		SetupMeasuringTest3();
	}

	void SetupMeasuringTest3()
	{
		ContentPage contentPage = new ContentPage();
		AddFlyoutItem(contentPage, "Width Measure and ToolBarItem (13949)");

		Grid grid = new Grid();
		grid.ColumnDefinitions.Add(new ColumnDefinition());
		grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
		grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
		grid.AddChild(new Label() { Text = "Text" }, 0, 0);
		grid.AddChild(new Button() { Text = "B1" }, 1, 0);
		grid.AddChild(new Button() { Text = "B2" }, 2, 0);

		Shell.SetTitleView(contentPage, grid);

		contentPage.Content = new StackLayout()
		{
			new Label() { Text = "TitleView should have one label and two buttons"}
		};

		contentPage.ToolbarItems.Add(new ToolbarItem() { Text = "Item" });
	}

	void SetupMeasuringTest2()
	{
		ContentPage contentPage = new ContentPage();
		AddFlyoutItem(contentPage, "Width Measure (13949)");

		Grid grid = new Grid();
		grid.ColumnDefinitions.Add(new ColumnDefinition());
		grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
		grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
		grid.AddChild(new Label() { Text = "Text" }, 0, 0);
		grid.AddChild(new Button() { Text = "B1" }, 1, 0);
		grid.AddChild(new Button() { Text = "B2" }, 2, 0);

		Shell.SetTitleView(contentPage, grid);

		contentPage.Content = new StackLayout()
		{
			new Label() { Text = "TitleView should have one label and two buttons"}
		};
	}

	void SetupMeasuringTest1()
	{
		AddTopTab(createContentPage("title 1"), "page 1");
		AddTopTab(createContentPage(null), "page 2");
		AddTopTab(createContentPage("title 3"), "page 3");
		AddTopTab(createContentPage("title 4"), "page 4");
		Items[0].Title = "Duplicate Test";
		ContentPage createContentPage(string titleView)
		{
			Label safeArea = new Label();
			ContentPage page = new ContentPage()
			{
				Content = new StackLayout()
				{
					new Label()
					{
						Text = "Tab 1, 3, and 4 should have a single visible TitleView. If the TitleView is duplicated or not visible the test has failed.",
						AutomationId = "Instructions"
					},
					safeArea
				}
			};

			page.ToolbarItems.Add(new ToolbarItem() { IconImageSource = "coffee.png", Order = ToolbarItemOrder.Primary, Priority = 0 });

			if (!string.IsNullOrWhiteSpace(titleView))
			{
				var stackLayout = new StackLayout()
				{
					new Label() { Text = titleView }
				};

				stackLayout.AutomationId = "TitleViewId";

				Shell.SetTitleView(page, stackLayout);
			}

			return page;
		}
	}
}
