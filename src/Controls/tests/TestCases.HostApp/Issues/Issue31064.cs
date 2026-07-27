namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, "31064", "Indicator view size should update dynamically", PlatformAffected.iOS)]

public class Issue31064 : ContentPage
{
	IndicatorView _indicator;

	public Issue31064()
	{
		var carousel = new CarouselView
		{
			ItemsSource = new[] { "Item 1", "Item 2", "Item 3" },
			HeightRequest = 250,
			HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
		};

		_indicator = new IndicatorView
		{
			HorizontalOptions = LayoutOptions.Center
		};

		carousel.IndicatorView = _indicator;

		var button = new Button
		{
			AutomationId = "Issue31064Button",
			Text = "Increase Indicator Size",
			HorizontalOptions = LayoutOptions.Center
		};
		button.Clicked += (s, e) =>
		{
			_indicator.IndicatorSize = 20;
		};

		Content = new VerticalStackLayout
		{
			Spacing = 20,
			Padding = 20,
			Children =
			{
				new Label
				{
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					Text = "Test passes if IndicatorView size updates dynamically"
				},
				carousel,
				_indicator,
				button
			}
		};
	}
}