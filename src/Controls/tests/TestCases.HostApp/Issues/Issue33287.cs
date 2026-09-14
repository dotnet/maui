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

		var resultLabel = new Label
		{
			Text = "Not run",
			AutomationId = "AlertResultLabel"
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
					{
						resultLabel.Text = "Waiting";
						var secondPage = new Issue33287SecondPage(() => resultLabel.Text = "Completed");
						await Navigation.PushAsync(secondPage);
						secondPage.ExposeBackButtonToAutomation();
					})
				},
				new Label
				{
					Text = "MainPage",
					AutomationId = "MainPageLabel"
				},
				resultLabel
			}
		};
	}
}

public class Issue33287SecondPage : ContentPage
{
	readonly Button _goBackButton;
	readonly TaskCompletionSource _unloaded = new(TaskCreationOptions.RunContinuationsAsynchronously);
	Task _alertTask;
	bool _navigatingBack;

	public Issue33287SecondPage(Action alertCompleted)
	{
		Title = "Second Page";
		Unloaded += (_, _) => _unloaded.TrySetResult();

		_goBackButton = new Button
		{
			Text = "Go Back",
			Command = new Command(async () =>
			{
				_navigatingBack = true;
				await Navigation.PopAsync();
				await _unloaded.Task;

				if (IsLoaded || Window is not null || _alertTask is null)
					throw new InvalidOperationException("The alert must be invoked while detaching the page.");

				await _alertTask;
				alertCompleted();
			})
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

	protected override void OnParentSet()
	{
		base.OnParentSet();

		if (_navigatingBack && Parent is null)
		{
			// Exercise the null-window path before navigation disconnects the handler
			// and DisplayAlertAsync would instead queue an alert for a future navigation.
			_alertTask = DisplayAlertAsync("Test Alert", "This alert was delayed", "OK");
		}
	}
}
