namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 28722, "IsEnabled does not work in BackButtonBehavior", PlatformAffected.iOS)]
public class Issue28722 : Shell
{
	public Issue28722()
	{
		Routing.RegisterRoute(nameof(Issue28722Page1), typeof(Issue28722Page1));
		Items.Add(new ContentPage());
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		Current.GoToAsync(nameof(Issue28722Page1));

	}
}

public class Issue28722Page1 : ContentPage
{
	public Issue28722Page1()
	{
		var label = new Label()
		{
			Text = "Welcome to Page 1",
			AutomationId = "HelloLabel",
			IsVisible = false
		};

		var backButtonBehavior = new BackButtonBehavior()
		{
			IsEnabled = true,
			TextOverride = "Click",
		};

		backButtonBehavior.Command = new Command(() =>
		{
			label.IsVisible = !label.IsVisible;
			backButtonBehavior.IsEnabled = false;
		});

		Shell.SetBackButtonBehavior(this, backButtonBehavior);
		Content = label;
	}
}
