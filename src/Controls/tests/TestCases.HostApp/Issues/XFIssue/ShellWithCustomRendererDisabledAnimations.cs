namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 0, "[Shell] Overriding animation with custom renderer to remove animation breaks next navigation",
	PlatformAffected.All)]
public class ShellWithCustomRendererDisabledAnimations : TestShell
{
	protected override void Init()
	{
		ContentPage contentPage = new ContentPage();
		base.AddFlyoutItem(contentPage, "Root");

		contentPage.Content = new Button()
		{
			Text = "Click Me",
			AutomationId = "PageLoaded",
			Command = new Command(async () =>
			{
				await Navigation.PushAsync(CreateSecondPage());
			})
		};
	}

	ContentPage CreateSecondPage()
	{
		ContentPage page = new ContentPage();

		page.Content = new StackLayout()
		{
			new Label()
			{
				Text = "If clicking `Go Back` goes back to previous page then test has passed"
			},
			new Button()
			{
				Text = "Go Back",
				Command = new Command(async () =>
				{
					await GoToAsync("..");
				}),
				AutomationId = "GoBack"
			}
		};

		return page;
	}
}
