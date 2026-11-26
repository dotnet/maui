namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 40495, "Secondary toolbar overflow button gets Shell.ForegroundColor while the primary items do not", PlatformAffected.iOS)]
public class Issue40495 : Shell
{
	public Issue40495()
	{
		var tabBar = new TabBar();
		tabBar.Items.Add(new ShellContent { Title = "Home", Content = new Issue40495Page() });
		tabBar.Items.Add(new ShellContent { Title = "Other", Content = new ContentPage { Title = "Other" } });

		// The foreground color is set on the TabBar, not on the Shell or the page.
		Shell.SetForegroundColor(tabBar, Colors.Purple);

		Items.Add(tabBar);
	}

	public class Issue40495Page : ContentPage
	{
		public Issue40495Page()
		{
			Title = "Home";

			ToolbarItems.Add(new ToolbarItem
			{
				IconImageSource = "calculator.png",
				Order = ToolbarItemOrder.Primary
			});
			ToolbarItems.Add(new ToolbarItem { Text = "Info", Order = ToolbarItemOrder.Secondary });
			ToolbarItems.Add(new ToolbarItem { Text = "Share", Order = ToolbarItemOrder.Secondary });

			Content = new Label
			{
				Text = "The test passes if the overflow (...) button has the same color as the other toolbar item.",
				AutomationId = "Issue40495_DescriptionLabel",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
		}
	}
}
