namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 14181, "Toolbar Items Do Not Reflect Shell ForegroundColor", PlatformAffected.iOS)]
public class Issue14181 : Shell
{
	public Issue14181()
	{
		Items.Add(new Issue14181Page());
		Shell.SetForegroundColor(this, Colors.Purple);
	}

	public class Issue14181Page : ContentPage
	{
		public Issue14181Page()
		{
			Title = "Home";
			ToolbarItems.Add(new ToolbarItem
			{
				IconImageSource = "calculator.png",
				Order = ToolbarItemOrder.Primary
			});

			Content = new Label
			{
				Text = "The test passes if the color is being applied to the toolbar.",
				AutomationId = "Issue14181_DescriptionLabel",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
		}
	}
}