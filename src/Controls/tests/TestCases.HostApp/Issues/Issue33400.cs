namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33400, "Runtime Scrollbar visibility not updating correctly on Android platform", PlatformAffected.Android)]
public class Issue33400 : ContentPage
{
	readonly ScrollView scrollView;

	public Issue33400()
	{
		var grid = new Grid
		{
			RowDefinitions = {
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto }
			},
			RowSpacing = 10
		};
		scrollView = new ScrollView
		{
			Orientation = ScrollOrientation.Horizontal,
			AutomationId = "TestScrollView"
		};

		var horizontalStack = new HorizontalStackLayout();

		var neverButton = new Button { Text = "Never", AutomationId = "Issue33400HorizontalNeverButton" };
		neverButton.Clicked += (s, e) => { scrollView.HorizontalScrollBarVisibility = ScrollBarVisibility.Never; };

		var defaultButton = new Button { Text = "Default", AutomationId = "Issue33400HorizontalDefaultButton" };
		defaultButton.Clicked += (s, e) =>
		{
			scrollView.HorizontalScrollBarVisibility = ScrollBarVisibility.Default;
			scrollView.ScrollToAsync(100, 0, true);  //Slight scroll to show the scrollbar
		};

		var alwaysButton = new Button { Text = "Always", AutomationId = "Issue33400HorizontalAlwaysButton" };
		alwaysButton.Clicked += (s, e) => { scrollView.HorizontalScrollBarVisibility = ScrollBarVisibility.Always; };

		horizontalStack.Add(neverButton);
		horizontalStack.Add(defaultButton);
		horizontalStack.Add(alwaysButton);

		var label = new Label
		{
			Padding = new Thickness(10),
			Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum. Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt.",
		};

		scrollView.Content = label;

		grid.Add(horizontalStack);
		Grid.SetRow(horizontalStack, 0);
		grid.Add(scrollView);
		Grid.SetRow(scrollView, 1);

		Content = grid;

	}
}