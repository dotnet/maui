namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 10563, "[Bug] SwipeView Open methods does not work for RightItems", PlatformAffected.Android | PlatformAffected.iOS)]
	public class Issue10563 : TestContentPage
	{
		const string OpenLeftId = "OpenLeftId";
		const string OpenRightId = "OpenRightId";
		const string OpenTopId = "OpenTopId";
		const string OpenBottomId = "OpenBottomId";
		const string CloseId = "CloseId";

		public Issue10563()
		{

		}

		protected override void Init()
		{
			Title = "Issue 10563";

			var swipeLayout = new StackLayout
			{
				Margin = new Thickness(12)
			};

			// Use flat Background to disable ripple/animation artifacts in screenshots
			var buttonBackground = new SolidColorBrush(Colors.LightGray);

			var openLeftButton = new Button
			{
				AutomationId = OpenLeftId,
				Text = "Open Left SwipeItem",
				Background = buttonBackground
			};

			var openRightButton = new Button
			{
				AutomationId = OpenRightId,
				Text = "Open Right SwipeItem",
				Background = buttonBackground
			};

			var openTopButton = new Button
			{
				AutomationId = OpenTopId,
				Text = "Open Top SwipeItem",
				Background = buttonBackground
			};

			var openBottomButton = new Button
			{
				AutomationId = OpenBottomId,
				Text = "Open Bottom SwipeItem",
				Background = buttonBackground
			};

			var closeButton = new Button
			{
				AutomationId = CloseId,
				Text = "Close SwipeView",
				Background = buttonBackground
			};

			swipeLayout.Children.Add(openLeftButton);
			swipeLayout.Children.Add(openRightButton);
			swipeLayout.Children.Add(openTopButton);
			swipeLayout.Children.Add(openBottomButton);
			swipeLayout.Children.Add(closeButton);

			var swipeItem = new SwipeItem
			{
				BackgroundColor = Colors.Red,
				IconImageSource = "calculator.png",
				Text = "Issue 10563"
			};

			swipeItem.Invoked += (sender, e) => { DisplayAlertAsync("SwipeView", "SwipeItem Invoked", "Ok"); };

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
				Text = "Swipe to any direction"
			};

			swipeContent.Children.Add(swipeLabel);

			var swipeView = new SwipeView
			{
				HeightRequest = 60,
				WidthRequest = 300,
				LeftItems = swipeItems,
				RightItems = swipeItems,
				TopItems = swipeItems,
				BottomItems = swipeItems,
				Content = swipeContent,
				Margin = new Thickness(0, 48)
			};

			swipeLayout.Children.Add(swipeView);

			Content = swipeLayout;

			openLeftButton.Clicked += (sender, e) =>
			{
				swipeView.Open(OpenSwipeItem.LeftItems);
			};

			openRightButton.Clicked += (sender, e) =>
			{
				swipeView.Open(OpenSwipeItem.RightItems);
			};

			openTopButton.Clicked += (sender, e) =>
			{
				swipeView.Open(OpenSwipeItem.TopItems);
			};

			openBottomButton.Clicked += (sender, e) =>
			{
				swipeView.Open(OpenSwipeItem.BottomItems);
			};

			closeButton.Clicked += (sender, e) =>
			{
				swipeView.Close();
			};
		}
	}
}