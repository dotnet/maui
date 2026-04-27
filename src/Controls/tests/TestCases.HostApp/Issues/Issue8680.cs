namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 8680, "Rework OnBackButtonPressed to use onBackPressedDispatcher", PlatformAffected.Android)]
public class Issue8680 : TestNavigationPage
{
	protected override void Init()
	{
		PushAsync(new Issue8680MainPage());
	}
}

public class Issue8680MainPage : ContentPage
{
	public Issue8680MainPage()
	{
		var navigateButton = new Button
		{
			Text = "Go to Intercept Page",
			AutomationId = "NavigateButton",
		};
		navigateButton.Clicked += async (s, e) =>
		{
			await Navigation.PushAsync(new Issue8680InterceptPage());
		};

		Content = new VerticalStackLayout
		{
			Children =
   {
	new Label { Text = "Main Page", AutomationId = "MainPageLabel" },
	navigateButton,
   }
		};
	}
}

public class Issue8680InterceptPage : ContentPage
{
	int _backPressCount;
	readonly Label _statusLabel;

	public Issue8680InterceptPage()
	{
		_statusLabel = new Label
		{
			Text = "Back not pressed yet",
			AutomationId = "StatusLabel",
		};

		Content = new VerticalStackLayout
		{
			Children =
   {
	_statusLabel,
	new Label { Text = "Press device back button — it should be intercepted", AutomationId = "InterceptPageLabel" },
   }
		};
	}

	protected override bool OnBackButtonPressed()
	{
		_backPressCount++;
		_statusLabel.Text = $"Back intercepted: {_backPressCount}";
		return true; // true = handled, prevents navigation back
	}
}
