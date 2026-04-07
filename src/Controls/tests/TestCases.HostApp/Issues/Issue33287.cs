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

		var statusLabel = new Label
		{
			Text = "Status: Ready",
			AutomationId = "StatusLabel",
			FontAttributes = FontAttributes.Bold
		};

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 10,
			Children =
			{
				new Label
				{
					Text = "DisplayAlertAsync NullReferenceException Test",
					FontSize = 18,
					FontAttributes = FontAttributes.Bold
				},
				new Label
				{
					Text = "1. Tap 'Navigate to Second Page'\n2. Immediately tap 'Go Back'\n3. Wait 5 seconds - should NOT crash",
					TextColor = Colors.Gray
				},
				new Button
				{
					Text = "Navigate to Second Page",
					AutomationId = "NavigateButton",
					Command = new Command(async () =>
					{
						statusLabel.Text = "Status: Navigating...";
						await Navigation.PushAsync(new Issue33287SecondPage(statusLabel));
					})
				},
				statusLabel
			}
		};
	}
}

public class Issue33287SecondPage : ContentPage
{
	readonly Label _statusLabel;

	public Issue33287SecondPage(Label statusLabel)
	{
		_statusLabel = statusLabel;
		Title = "Second Page";

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 10,
			Children =
			{
				new Label
				{
					Text = "Second Page - Tap 'Go Back' quickly!",
					FontSize = 18,
					FontAttributes = FontAttributes.Bold
				},
				new Button
				{
					Text = "Go Back",
					AutomationId = "GoBackButton",
					Command = new Command(async () => await Navigation.PopAsync())
				}
			}
		};
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		await Task.Delay(3000);

		try
		{
			await DisplayAlertAsync("Test Alert", "This alert was delayed", "OK");
			_statusLabel.Text = "Status: ✅ Alert completed";
		}
		catch (Exception ex)
		{
			_statusLabel.Text = $"Status: ❌ {ex.GetType().Name}";
		}
	}
}
