namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 36652, "SwipeView directly inside a Border crashes natively on Windows", PlatformAffected.UWP)]
public class Issue36652 : ContentPage
{
	public Issue36652()
	{
		var swipeItem = new SwipeItem
		{
			BackgroundColor = Colors.Red,
			Text = "Delete"
		};

		var swipeItems = new SwipeItems { swipeItem };

		var swipeContent = new Grid
		{
			BackgroundColor = Colors.LightBlue,
			HeightRequest = 60
		};

		swipeContent.Children.Add(new Label
		{
			AutomationId = "SwipeContentLabel",
			Text = "Swipe Me",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		});

		var swipeView = new SwipeView
		{
			AutomationId = "SwipeViewInsideBorder",
			RightItems = swipeItems,
			Content = swipeContent
		};

		// The crash reported in #36652 only occurs when the SwipeView is the direct
		// Content of a Border (Border > SwipeView), since WinUI applies a geometric
		// clip straight onto the SwipeControl's visual in that hierarchy.
		var border = new Border
		{
			AutomationId = "BorderWithSwipeViewContent",
			Stroke = Colors.Black,
			StrokeThickness = 2,
			StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
			Margin = new Thickness(24),
			Content = swipeView
		};

		var resultLabel = new Label
		{
			AutomationId = "ResultLabel",
			Text = "If you can see this, the app did not crash."
		};

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(12),
			Children =
			{
				resultLabel,
				border
			}
		};
	}
}
