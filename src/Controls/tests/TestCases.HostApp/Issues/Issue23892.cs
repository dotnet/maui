namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 23892, "Using long-press navigation on back button using shell does not update the shell's current page", PlatformAffected.iOS)]

public class Issue23892 : TestShell
{
	protected override void Init()
	{
		var page = CreateContentPage("Test page");
		int onAppearingCount = 0;

		Label label = new Label
		{
			AutomationId = "label",
			Text = "This is a test page"
		};
		page.Content = new StackLayout()
		{
			label,
			new Button
			{
				AutomationId = "button",
				Text = "Click to navigate to detail page",
				Command = new Command(()=> Shell.Current.Navigation.PushAsync(new Issue23892Detail()))
			}
		};

		page.Appearing += (s, e) =>
		{
			label.Text = $"OnAppearing count: {++onAppearingCount}";
		};
		Routing.RegisterRoute(nameof(Issue23892Detail), typeof(Issue23892Detail));
	}

	class Issue23892Detail : ContentPage
	{
		public Issue23892Detail()
		{
			SetBackButtonBehavior(this, new BackButtonBehavior
			{
				TextOverride = "Back",
			});

			Content = new StackLayout
			{
				Children =
				{
					new Label { Text = "This is a detail page" }
				}
			};
		}
	}
}