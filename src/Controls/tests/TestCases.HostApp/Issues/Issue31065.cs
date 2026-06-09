namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31065, "IndicatorView square shape does not update on load or dynamically", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue31065 : ContentPage
{
	IndicatorView _indicator;
	public Issue31065()
	{
		var carousel = new CarouselView
		{
			ItemsSource = new[] { "Item 1", "Item 2", "Item 3" },
			HeightRequest = 250,
		};

		_indicator = new IndicatorView
		{
			IndicatorsShape = IndicatorShape.Square,
			SelectedIndicatorColor = Colors.Blue,
			IndicatorSize = 30
		};

		Button button = new Button
		{
			Text = "Change indicator shape",
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "ChangeIndicatorShapeButton",
			Margin = new Thickness(0, 20, 0, 0)
		};

		button.Command = new Command(() =>
		{
			if (_indicator.IndicatorsShape == IndicatorShape.Square)
			{
				_indicator.IndicatorsShape = IndicatorShape.Circle;
			}
			else
			{
				_indicator.IndicatorsShape = IndicatorShape.Square;
			}
		});

		carousel.IndicatorView = _indicator;

		Content = new VerticalStackLayout
		{
			Children =
			{
				carousel,
				_indicator,
				button
			}
		};
	}
}