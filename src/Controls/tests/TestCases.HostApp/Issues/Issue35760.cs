namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35760, "Shell title updates after switching tabs with context actions open", PlatformAffected.Android)]
public class Issue35760 : TestShell
{
	protected override void Init()
	{
		var list = new ListView
		{
			ItemsSource = new[] { "Message1", "Message2" },
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, ".");
				label.SetBinding(AutomationIdProperty, ".");
				var cell = new ViewCell { View = label };
				cell.ContextActions.Add(new MenuItem { Text = "Archive" });
				return cell;
			})
		};

		var button = new Button
		{
			Text = "Go to details",
			AutomationId = "GoToDetails"
		};
		button.Clicked += async (_, _) => await GoToAsync("//Details");

		AddTopTab(new ContentPage
		{
			Content = new StackLayout
			{
				Children = { list, button }
			}
		}, "Messages");

		AddTopTab(new ContentPage
		{
			Content = new Label
			{
				Text = "Message details",
				AutomationId = "DetailsContent"
			}
		}, "Details");
	}
}
