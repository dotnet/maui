namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 40161, "Shell root search appears on a pushed page after resuming", PlatformAffected.All)]
public class Issue40161 : TestShell
{
	protected override void Init()
	{
		FlyoutBehavior = FlyoutBehavior.Disabled;

		var push = new Button { Text = "Push page", AutomationId = "PushPage" };
		var root = new ContentPage { Title = "Root", Content = push };
		SetSearchHandler(root, new SearchHandler
		{
			Placeholder = "Root search 40161",
			SearchBoxVisibility = SearchBoxVisibility.Collapsible
		});

		push.Clicked += async (_, _) =>
		{
			var pop = new Button { Text = "Pop page", AutomationId = "PopPage" };
			var detail = new ContentPage { Title = "Pushed", Content = pop };
			pop.Clicked += async (_, _) => await Navigation.PopAsync();
			await Navigation.PushAsync(detail);
		};

		AddContentPage(root);
	}
}
