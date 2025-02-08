namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 27630, "Hidden ListView doesn't appear as expected", PlatformAffected.iOS | PlatformAffected.Android)]
	public class Issue27630 : TestContentPage
	{
		protected override void Init()
		{
			var listView = new ListView
			{
				HeightRequest = 100,
				IsVisible = false,
				ItemTemplate = new DataTemplate(() => new ViewCell()
				{
					View = new Label()
					{
						Text = "Hello",
						AutomationId = "ViewCellLabel"
					}
				}),
				ItemsSource = new string[] { "Item 1", "Item 2", "Item 3" }
			};

			var button = new Button
			{
				Text = "Show/Hide ListView",
				AutomationId = "Button",
				Command = new Command(() => listView.IsVisible = !listView.IsVisible)
			};

			Content = new VerticalStackLayout()
			{
				Children = { button, listView }
			};
		}
	}
}
