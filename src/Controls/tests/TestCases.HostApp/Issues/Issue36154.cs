namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 36154, "WebView inside SwipeView no longer responds to swipe gestures on Android", PlatformAffected.Android)]
public class Issue36154 : ContentPage
{
	public Issue36154()
	{
		var directionLabel = new Label
		{
			AutomationId = "DirectionLabel",
			Text = "Direction: —    Offset: 0",
			HorizontalOptions = LayoutOptions.Center,
			Margin = new Thickness(4, 8),
			FontSize = 13
		};

		var resultLabel = new Label
		{
			AutomationId = "ResultLabel",
			Text = "Swipe result will appear here",
			HorizontalOptions = LayoutOptions.Center,
			Margin = new Thickness(4, 0, 4, 12),
			FontSize = 13,
			TextColor = Colors.Gray
		};

		var webView = new WebView
		{
			AutomationId = "TheWebView",
			HorizontalOptions = LayoutOptions.Fill,
			VerticalOptions = LayoutOptions.Fill,
			// Long scrollable page so vertical scroll can be verified
			Source = new HtmlWebViewSource
			{
				Html = """
					<html>
					<body style="font-family:sans-serif;padding:16px">
					<h3>Issue 36154 – WebView scroll test</h3>
					<p>Scroll down to verify WebView vertical scrolling works inside SwipeView.</p>
					<p>Then swipe left/right to reveal swipe items.</p>
					<p>At the top edge, swipe down to reveal TopItems.</p>
					<p>At the bottom edge, swipe up to reveal BottomItems.</p>
					""" + string.Concat(Enumerable.Range(1, 40).Select(i =>
						$"<p>Line {i}: Lorem ipsum dolor sit amet consectetur adipiscing elit.</p>"))
					+ "</body></html>"
			}
		};

		var swipeView = new SwipeView
		{
			AutomationId = "TheSwipeView",
			Threshold = 80,

			LeftItems = new SwipeItems(new[]
			{
				new SwipeItem
				{
					AutomationId = "LeftItem",
					Text = "◀ LEFT",
					BackgroundColor = Colors.MediumSeaGreen,
					Command = new Command(() =>
					{
						resultLabel.Text = "✅ LEFT item invoked";
						Console.WriteLine("ISSUE36154: LEFT item invoked");
					})
				}
			})
			{ Mode = SwipeMode.Execute, SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.Close },

			RightItems = new SwipeItems(new[]
			{
				new SwipeItem
				{
					AutomationId = "RightItem",
					Text = "RIGHT ▶",
					BackgroundColor = Colors.CornflowerBlue,
					Command = new Command(() =>
					{
						resultLabel.Text = "✅ RIGHT item invoked";
						Console.WriteLine("ISSUE36154: RIGHT item invoked");
					})
				}
			})
			{ Mode = SwipeMode.Execute, SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.Close },

			TopItems = new SwipeItems(new[]
			{
				new SwipeItem
				{
					AutomationId = "TopItem",
					Text = "▲ TOP",
					BackgroundColor = Colors.Orchid,
					Command = new Command(() =>
					{
						resultLabel.Text = "✅ TOP item invoked";
						Console.WriteLine("ISSUE36154: TOP item invoked");
					})
				}
			})
			{ Mode = SwipeMode.Execute, SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.Close },

			BottomItems = new SwipeItems(new[]
			{
				new SwipeItem
				{
					AutomationId = "BottomItem",
					Text = "▼ BOTTOM",
					BackgroundColor = Colors.Tomato,
					Command = new Command(() =>
					{
						resultLabel.Text = "✅ BOTTOM item invoked";
						Console.WriteLine("ISSUE36154: BOTTOM item invoked");
					})
				}
			})
			{ Mode = SwipeMode.Execute, SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.Close },

			Content = webView
		};

		swipeView.SwipeStarted += (s, e) =>
		{
			directionLabel.Text = $"Direction: {e.SwipeDirection}    Offset: 0";
			Console.WriteLine($"ISSUE36154: SwipeStarted direction={e.SwipeDirection}");
		};

		swipeView.SwipeChanging += (s, e) =>
			directionLabel.Text = $"Direction: {e.SwipeDirection}    Offset: {e.Offset:F0}";

		swipeView.SwipeEnded += (s, e) =>
		{
			directionLabel.Text = $"Direction: {e.SwipeDirection}    Open: {e.IsOpen}";
			Console.WriteLine($"ISSUE36154: SwipeEnded direction={e.SwipeDirection} isOpen={e.IsOpen}");
		};

		Content = new Grid
		{
			RowDefinitions = Rows.Define(Auto, Star, Auto, Auto),
			Children =
			{
				new Label
				{
					Text = "Swipe on WebView · scroll mid-page · swipe at edges",
					HorizontalOptions = LayoutOptions.Center,
					Margin = new Thickness(8),
					FontSize = 13,
					FontAttributes = FontAttributes.Bold
				}.Row(0),

				swipeView.Row(1),
				directionLabel.Row(2),
				resultLabel.Row(3)
			}
		};
	}
}
