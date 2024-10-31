namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 6258, "[Android] ContextActions icon not working", PlatformAffected.Android)]

public class Issue6258 : TestNavigationPage
{
	protected override void Init()
	{
		var page = new ContentPage();

		PushAsync(page);

		page.Content = new ListView()
		{
			ItemsSource = new[] { "1" },
			ItemTemplate = new DataTemplate(() =>
			{
				ViewCell cells = new ViewCell();

				cells.ContextActions.Add(new MenuItem()
				{
					IconImageSource = "coffee.png",
					AutomationId = "coffee.png"
				});

				cells.View = new StackLayout()
				{
					new Label()
					{
						Text = "Trigger context action on this row and you should see a coffee cup. Test only relevation on Android",
						AutomationId = "ListViewItem"
					}
				};

				return cells;
			})
		};
	}
}
