namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, "31445", "Duplicate Title icon should not appear", PlatformAffected.Android)]

public class Issue31445 : NavigationPage
{
	public Issue31445() : base(new Issue31445Page())
	{
	}
}

public class Issue31445Page : ContentPage
{
	public Issue31445Page()
	{
		NavigationPage.SetTitleIconImageSource(this, "dotnet_bot.png");

		var label = new Label()
		{
			Text = "Test passes if only one title icon is set after button click",
			VerticalOptions = LayoutOptions.Start,
			AutomationId = "label"
		};

		var button = new Button()
		{
			Text = "Click here",
			VerticalOptions = LayoutOptions.Start,
			AutomationId = "Issue31445Button"
		};
		button.Clicked += (s, e) => { NavigationPage.SetTitleIconImageSource(this, "dotnet_bot.png"); };

		Content = new StackLayout
		{
			Children = { label, button }
		};
	}
}