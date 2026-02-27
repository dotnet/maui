namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 12079, "SwipeView crash if Text not is set on SwipeItem", PlatformAffected.UWP)]
	public class Issue12079 : TestContentPage
	{
		const string SwipeViewId = "SwipeViewId";

		protected override void Init()
		{
			Title = "Issue 12079";

			var layout = new StackLayout
			{
				Margin = new Thickness(12)
			};

			var instructions = new Label
			{
				BackgroundColor = Colors.Black,
				TextColor = Colors.White,
				Text = "Swipe to the right, if can open the SwipeView the test has passed."
			};

			var swipeItem = new SwipeItem
			{
				BackgroundColor = Colors.Red,
				IconImageSource = "calculator.png"
			};

			swipeItem.Invoked += (sender, e) =>
			{
				DisplayAlertAsync("Issue12079", "Invoked", "Ok");
			};

			var swipeItems = new SwipeItems { swipeItem };

			swipeItems.Mode = SwipeMode.Reveal;

			var swipeContent = new Grid
			{
				BackgroundColor = Colors.Gray
			};

			var swipeLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Swipe to Right (No Text)"
			};

			swipeContent.Children.Add(swipeLabel);

			var swipeView = new SwipeView
			{
				AutomationId = SwipeViewId,
				HeightRequest = 60,
				WidthRequest = 300,
				LeftItems = swipeItems,
				Content = swipeContent
			};

			var result = new Label();

			swipeView.SwipeEnded += (sender, args) =>
			{
				result.Text = "Success";
			};

			layout.Children.Add(instructions);
			layout.Children.Add(swipeView);
			layout.Children.Add(result);

			Content = layout;
		}
	}
}