#if IOS
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 39080, "Disabled RefreshView allows pull gesture on iOS", PlatformAffected.iOS)]
public class Issue39080 : ContentPage
{
	public Issue39080()
	{
		var result = new Label
		{
			AutomationId = "Result",
			Text = "No pull movement",
			HorizontalOptions = LayoutOptions.Center
		};
		var content = new VerticalStackLayout
		{
			Spacing = 10,
			Padding = 10
		};
		content.Children.Add(new Label
		{
			Text = "Pull-to-refresh is disabled",
			HorizontalOptions = LayoutOptions.Center
		});
		content.Children.Add(new Label
		{
			Text = "Dragging down must not move this content."
		});
		var scrollView = new ScrollView
		{
			AutomationId = "GestureTarget",
			Content = content
		};
		scrollView.Scrolled += (_, args) =>
		{
			if (args.ScrollY < -5)
				result.Text = "Pull movement detected";
		};
		var refreshView = new RefreshView
		{
			IsRefreshEnabled = false,
			Content = scrollView
		};
		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			}
		};
		Grid.SetRow(result, 0);
		grid.Add(result);
		Grid.SetRow(refreshView, 1);
		grid.Add(refreshView);
		Content = grid;
	}
}
#endif
