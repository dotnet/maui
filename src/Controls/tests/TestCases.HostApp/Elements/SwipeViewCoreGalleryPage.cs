namespace Maui.Controls.Sample
{
	public class SwipeViewCoreGalleryPage : ContentPage
	{
		const string SwipeViewToRightId = "SwipeViewToRightId";
		const string ResultToRightId = "ResultToRightId";
		const string SwipeViewToLeftId = "SwipeViewToLeftId";
		const string ResultToLeftId = "ResultToLeftId";

		public SwipeViewCoreGalleryPage()
		{
			Title = "Issue 12079";

			var layout = new StackLayout
			{
				Margin = new Thickness(12)
			};

			var swipeItemToRight = new SwipeItem
			{
				BackgroundColor = Colors.Red,
				Text = "SwipeItem",
			};

			swipeItemToRight.Invoked += (sender, e) =>
			{
				DisplayAlertAsync("SwipeView", "Invoked", "Ok");
			};

			var swipeToRightItems = new SwipeItems { swipeItemToRight };

			swipeToRightItems.Mode = SwipeMode.Reveal;

			var swipeToRightContent = new Grid
			{
				BackgroundColor = Colors.Gray
			};

			var swipeToRightLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Swipe to Right"
			};

			swipeToRightContent.Children.Add(swipeToRightLabel);

			var swipeToRightView = new SwipeView
			{
				AutomationId = SwipeViewToRightId,
				HeightRequest = 60,
				WidthRequest = 300,
				LeftItems = swipeToRightItems,
				Content = swipeToRightContent
			};

			var resultToRight = new Label
			{
				AutomationId = ResultToRightId
			};

			swipeToRightView.SwipeEnded += (sender, args) =>
			{
				resultToRight.Text = "Success";
			};

			var swipeItemToLeft = new SwipeItem
			{
				BackgroundColor = Colors.Green,
				Text = "SwipeItem",
			};

			swipeItemToLeft.Invoked += (sender, e) =>
			{
				DisplayAlertAsync("SwipeView", "Invoked", "Ok");
			};

			var swipeToLeftItems = new SwipeItems { swipeItemToLeft };

			swipeToLeftItems.Mode = SwipeMode.Reveal;

			var swipeToLeftContent = new Grid
			{
				BackgroundColor = Colors.Gray
			};

			var swipeToLeftLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Swipe to Left"
			};

			swipeToLeftContent.Children.Add(swipeToLeftLabel);

			var swipeToLeftView = new SwipeView
			{
				AutomationId = SwipeViewToLeftId,
				HeightRequest = 60,
				WidthRequest = 300,
				RightItems = swipeToLeftItems,
				Content = swipeToLeftContent
			};

			var resultToLeft = new Label
			{
				AutomationId = ResultToLeftId
			};

			swipeToLeftView.SwipeEnded += (sender, args) =>
			{
				resultToLeft.Text = "Success";
			};

			layout.Children.Add(swipeToRightView);
			layout.Children.Add(resultToRight);

			layout.Children.Add(swipeToLeftView);
			layout.Children.Add(resultToLeft);

			Content = layout;
		}
	}
}