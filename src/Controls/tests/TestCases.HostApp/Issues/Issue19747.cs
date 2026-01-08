namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19747, "Shell BackButtonBehavior TextOverride text gets truncated", PlatformAffected.Android)]
public class Issue19747 : TestShell
{
	public const string NavigateButtonText = "Navigate to Page 2";
	public const string BackButtonText = "Cancel";

	protected override void Init()
	{
		CreateContentPage("Home").Content =
			new StackLayout
			{
				new Label
				{
					Text = "Tap the button to navigate to Page 2. The back button should show 'Cancel' text fully visible, not truncated.",
					Margin = new Thickness(20)
				},
				new Button
				{
					AutomationId = "NavigateButton",
					Text = NavigateButtonText,
					Command = new Command(async () =>
					{
						await Shell.Current.Navigation.PushAsync(new Issue19747Page2());
					})
				}
			};
	}
}

public class Issue19747Page2 : ContentPage
{
	public Issue19747Page2()
	{
		Title = "Page2";

		// Set BackButtonBehavior with TextOverride
		Shell.SetBackButtonBehavior(this, new BackButtonBehavior
		{
			TextOverride = Issue19747.BackButtonText // "Cancel" - should be fully visible, not truncated
		});

		Content = new StackLayout
		{
			new Label
			{
				Text = "This is Page 2. The back button should show 'Cancel' as the text, not truncated.",
				Margin = new Thickness(20)
			},
			new Label
			{
				AutomationId = "StatusLabel",
				Text = $"Expected back button text: {Issue19747.BackButtonText}",
				Margin = new Thickness(20)
			}
		};
	}
}
