using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33547, "[iOS] Shell Page gets moved partially outside of viewport when focusing element on page load", PlatformAffected.iOS)]
public class Issue33547 : TestShell
{
	protected override void Init()
	{
		Shell.SetForegroundColor(this, Colors.Black);

		Routing.RegisterRoute("newpage", typeof(Issue33547NewPage));

		var mainPage = new ContentPage
		{
			Title = "Issue 33547",
			Content = new VerticalStackLayout
			{
				Padding = 20,
				Spacing = 10,
				Children =
					{
						new Label
						{
							Text = "Tap button to navigate. Next page will auto-focus Entry and show keyboard.",
							AutomationId = "InstructionLabel"
						},
						new Button
						{
							Text = "Navigate to Page with Entry",
							AutomationId = "NavigationPushButton",
							Command = new Command(async () =>
							{
								await Shell.Current.Navigation.PushAsync(new Issue33547NewPage());
							})
						}
					}
			}
		};

		AddContentPage(mainPage, "Issue 33547");
	}
}
public class Issue33547NewPage : ContentPage
{
	public Issue33547NewPage()
	{
		Title = "NewPage";

		var entry = new Entry
		{
			AutomationId = "TestEntry",
			Placeholder = "Entry auto-focused (keyboard shown)",
			ReturnType = ReturnType.Done
		};

		var label = new Label
		{
			Text = "Under the Entry"
		};

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 10,
			Children = { entry, label }
		};

		Loaded += (sender, args) =>
		{
			entry.Focus();
		};
	}
}
