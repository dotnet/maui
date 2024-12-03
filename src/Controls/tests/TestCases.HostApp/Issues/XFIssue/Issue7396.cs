namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 7396, "Setting Shell.BackgroundColor overrides all colors of TabBar", PlatformAffected.Android)]

public class Issue7396 : TestShell
{
	const string CreateTopTabButton = "CreateTopTabButton";
	const string CreateBottomTabButton = "CreateBottomTabButton";
	const string ChangeShellColorButton = "ChangeShellBackgroundColorButton";

	protected override void Init()
	{
		var page = CreateContentPage();
		page.Title = "Main";
		page.Content = CreateEntryInsetView();

		CurrentItem = Items.Last();
	}

	View CreateEntryInsetView()
	{
		var random = new Random();
		ScrollView view = null;
		view = new ScrollView()
		{
			Content = new StackLayout()
			{
					new Button()
					{
						Text = "Top Tab",
						AutomationId = CreateTopTabButton,
						Command = new Command(() => AddTopTab("top"))
					},
					new Button()
					{
						Text = "Bottom Tab",
						AutomationId = CreateBottomTabButton,
						Command = new Command(() => AddBottomTab("bottom", "coffee.png"))
					},
					new Button()
					{
						Text = "Random Shell Background Color",
						AutomationId = ChangeShellColorButton,
						Command = new Command(() =>
							Shell.SetBackgroundColor(this, Colors.Red))
					},
			}
		};

		return view;
	}
}
