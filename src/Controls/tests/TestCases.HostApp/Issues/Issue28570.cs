namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28570, "Setting BackButtonBehavior to not visible or not enabled does not work")]
public class Issue28570 : Shell
{
	public Issue28570()
	{
		// Register the routes
		Routing.RegisterRoute("Issue28570DetailPage", typeof(Issue28570DetailPage));

		ContentPage contentPage = new ContentPage
		{
			Title = "Main Page",
			Content = new Button
			{
				Text = "Go to Detail Page",
				AutomationId = "NavigateToDetailButton",
				Command = new Command(async () => await Shell.Current.GoToAsync("Issue28570DetailPage"))
			}
		};

		BackButtonBehavior backButtonBehavior = new BackButtonBehavior
		{
			IsVisible = false,
			TextOverride = "BackButton"
		};

		SetBackButtonBehavior(this, backButtonBehavior);

		// Create and add Shell contents
		var page1 = new ShellContent
		{
			Title = "Page 1",
			Content = contentPage
		};

		Items.Add(page1);
	}
}

public class Issue28570DetailPage : ContentPage
{
	public Issue28570DetailPage()
	{
		Title = "Detail page";

		Content = new VerticalStackLayout()
		{
			new Label()
			{
				AutomationId = "HelloLabel",
				Text = "Hello, from the detail page",
			}
		};
	}
}