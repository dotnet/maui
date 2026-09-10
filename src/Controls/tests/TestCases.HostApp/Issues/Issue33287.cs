using System;
using System.Threading.Tasks;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33287, "DisplayAlertAsync throws NullReferenceException when page is no longer displayed", PlatformAffected.All)]
public class Issue33287 : NavigationPage
{
	public Issue33287() : base(new Issue33287MainPage())
	{
	}
}

public class Issue33287MainPage : ContentPage
{
	public Issue33287MainPage()
	{
		Title = "Issue 33287";

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 10,
			Children =
			{
				new Button
				{
					Text = "Navigate to Second Page",
					AutomationId = "NavigateButton",
					Command = new Command(async () =>
					{
						var secondPage = new Issue33287SecondPage();
						await Navigation.PushAsync(secondPage);
						secondPage.ExposeBackButtonToAutomation();
					})
				},
				new Label
				{
					Text = "MainPage",
					AutomationId = "MainPageLabel"
				}
			}
		};
	}
}

public class Issue33287SecondPage : ContentPage
{
	readonly Button _goBackButton;

	public Issue33287SecondPage()
	{
		Title = "Second Page";

		_goBackButton = new Button
		{
			Text = "Go Back",
			Command = new Command(async () => await Navigation.PopAsync())
		};

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Children =
			{
				_goBackButton
			}
		};
	}

	internal void ExposeBackButtonToAutomation() =>
		_goBackButton.AutomationId = "GoBackButton";

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		// Wait long enough for the user/test to navigate back
#if MACCATALYST
		await Task.Delay(4000);
#else
		await Task.Delay(2000);
#endif

		// Without the fix this throws NullReferenceException and crashes the app
		await DisplayAlertAsync("Test Alert", "This alert was delayed", "OK");
	}
}
