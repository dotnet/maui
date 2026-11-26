namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24571, "RemainingItemsThresholdReached is not raised for a looping CarouselView", PlatformAffected.Android)]
public class Issue24571 : ContentPage
{
	public Issue24571()
	{
		var statusLabel = new Label
		{
			AutomationId = "StatusLabel",
			Text = "Waiting"
		};

		var carouselView = new CarouselView
		{
			AutomationId = "CarouselView",
			Loop = true,
			RemainingItemsThreshold = 0,
			HeightRequest = 200,
			ItemsSource = new[] { "Item 0", "Item 1", "Item 2", "Item 3", "Item 4" },
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.Center
				};
				label.SetBinding(Label.TextProperty, ".");
				return label;
			})
		};

		carouselView.RemainingItemsThresholdReached += (s, e) => statusLabel.Text = "Threshold reached";

		var scrollToLastButton = new Button
		{
			AutomationId = "ScrollToLastButton",
			Text = "Scroll to the last item",
			Command = new Command(() => carouselView.Position = 4)
		};

		Content = new VerticalStackLayout
		{
			Children = { statusLabel, scrollToLastButton, carouselView }
		};
	}
}
