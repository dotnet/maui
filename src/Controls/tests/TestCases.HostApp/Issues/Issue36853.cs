namespace Maui.Controls.Sample.Issues;

// Reproduces the bug: Root → SecondPage (singleton) → ThirdPage → ///Root → SecondPage again → BLANK
[Issue(IssueTracker.Github, 36853, "Shell singleton page renders blank when re-pushed after absolute route PopToRoot on Android", PlatformAffected.Android)]
public class Issue36853 : TestShell
{
	protected override void Init()
	{
		Routing.RegisterRoute("Issue36853Second", typeof(Issue36853SecondPage));
		Routing.RegisterRoute("Issue36853Third", typeof(Issue36853ThirdPage));

		var mainPage = new ContentPage
		{
			Title = "Main",
			Content = new VerticalStackLayout
			{
				Spacing = 20,
				Padding = 20,
				Children =
				{
					new Label
					{
						Text = "Main Page",
						AutomationId = "Issue36853MainLabel",
						FontSize = 24
					},
					new Button
					{
						Text = "Go to Second Page",
						AutomationId = "Issue36853GoToSecond",
						Command = new Command(async () =>
							await Shell.Current.GoToAsync("Issue36853Second"))
					}
				}
			}
		};

		AddContentPage(mainPage, "Issue36853Main");
	}
}

// Registered as Singleton in DI — same instance returned every time the route resolves
public class Issue36853SecondPage : ContentPage
{
	public Issue36853SecondPage()
	{
		Title = "Second";
		Content = new VerticalStackLayout
		{
			Spacing = 20,
			Padding = 20,
			Children =
			{
				new Label
				{
					Text = "Second Page Content",
					AutomationId = "Issue36853SecondLabel",
					FontSize = 24
				},
				new Button
				{
					Text = "Go to Third Page",
					AutomationId = "Issue36853GoToThird",
					Command = new Command(async () =>
						await Shell.Current.GoToAsync("Issue36853Third"))
				}
			}
		};
	}
}

public class Issue36853ThirdPage : ContentPage
{
	public Issue36853ThirdPage()
	{
		Title = "Third";
		Content = new VerticalStackLayout
		{
			Spacing = 20,
			Padding = 20,
			Children =
			{
				new Label
				{
					Text = "Third Page Content",
					AutomationId = "Issue36853ThirdLabel",
					FontSize = 24
				},
				new Button
				{
					Text = "Reset to Root (///)",
					AutomationId = "Issue36853ResetToRoot",
					Command = new Command(async () =>
						await Shell.Current.GoToAsync("///Issue36853Main"))
				}
			}
		};
	}
}

static class Issue36853Extensions
{
	public static MauiAppBuilder Issue36853RegisterServices(this MauiAppBuilder builder)
	{
		// SecondPage is singleton — same instance reused across navigations (the bug scenario)
		builder.Services.AddSingleton<Issue36853SecondPage>();
		// ThirdPage is transient — new instance each time (normal behavior)
		builder.Services.AddTransient<Issue36853ThirdPage>();
		return builder;
	}
}
