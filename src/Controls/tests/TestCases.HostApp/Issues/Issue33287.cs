using System;
using System.ComponentModel;

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
			Text = "Waiting for alert request",
			AutomationId = "AlertStatusLabel"
		};

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
						await Navigation.PushAsync(new Issue33287SecondPage(status =>
							statusLabel.Text = status)))
				},
				new Label
				{
					Text = "MainPage",
					AutomationId = "MainPageLabel"
				},
				statusLabel
			}
		};
	}
}

public class Issue33287SecondPage : ContentPage
{
	public Issue33287SecondPage(Action<string> updateStatus)
	{
		Title = "Second Page";
		PropertyChanged += OnPropertyChanged;

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Children =
			{
				new Button
				{
					Text = "Go Back",
					AutomationId = "GoBackButton",
					Command = new Command(async () => await Navigation.PopAsync())
				}
			}
		};

		void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != nameof(Window) || Window is not null)
				return;

			PropertyChanged -= OnPropertyChanged;
			updateStatus("Page detached");

			// The original NRE occurs synchronously while creating the alert request.
			// A detached page may keep the returned task pending until it is reattached.
			_ = DisplayAlertAsync("Test Alert", "This alert was delayed", "OK");
			updateStatus("Alert request returned");
		}
	}
}
