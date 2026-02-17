namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34083, "Toolbar Items Do Not Reflect Shell ForegroundColor", PlatformAffected.iOS)]
public class Issue34083 : Shell
{
	public Issue34083()
	{
		Items.Add(new Issue34083Page());
		Shell.SetForegroundColor(this, Colors.Purple);
	}

	public class Issue34083Page : ContentPage
	{
		public Issue34083Page()
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
				AutomationId = "Issue34083_DescriptionLabel",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
		}
	}
}