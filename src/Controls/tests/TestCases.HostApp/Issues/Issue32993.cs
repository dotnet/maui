namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 32993, "TabBar Should update correctly in RTL mode", PlatformAffected.iOS | PlatformAffected.Android)]
public class Issue32993 : Shell
{
	public Issue32993()
	{
		this.FlowDirection = FlowDirection.LeftToRight;

		Button setRTLButton = new Button
		{
			AutomationId = "Issue32993SetRTL",
			Text = "Set RTL",
			WidthRequest = 120
		};
		setRTLButton.Clicked += (s, e) => this.FlowDirection = FlowDirection.RightToLeft;

		var issue32993tab1Layout = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 15,
			Children =
			{
				new Label { Text = "This is Tab1" },
				new HorizontalStackLayout
				{
					Spacing = 10,
					HorizontalOptions = LayoutOptions.Center,
					Children = { setRTLButton }
				}
			}
		};

		var issue32993tab1 = new Tab
		{
			Title = "Tab1",
			Items =
			{
				new ShellContent { Title = "Issue32993", Content = new ContentPage { Content = issue32993tab1Layout } }
			}
		};

		var issue32993tab2Layout = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 15,
			Children =
			{
				new Label { Text = "This is Tab2" }
			}
		};

		var issue32993tab2 = new Tab
		{
			Title = "Tab2",
			Items =
			{
				new ShellContent { Title = "Issue32993", Content = new ContentPage { Content = issue32993tab2Layout } }
			}
		};

		Items.Add(new TabBar { Items = { issue32993tab1, issue32993tab2 } });
	}
}