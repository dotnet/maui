namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31063, "IndicatorView remains interactive even when IsEnabled is False", PlatformAffected.Android)]
public partial class Issue31063 : ContentPage
{
	CarouselView _carouselView;
	IndicatorView _indicatorView;

	public Issue31063()
	{
		// Create StackLayout
		var stackLayout = new StackLayout
		{
			Padding = 20,
			Spacing = 20
		};

		// CarouselView
		_carouselView = new CarouselView
		{
			HeightRequest = 200,
			ItemsSource = new string[]
			{
				"Item 1", "Item 2", "Item 3", "Item 4", "Item 5"
			},
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					FontSize = 30,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					Padding = 20
				};
				label.SetBinding(Label.TextProperty, ".");
				return label;
			})
		};

		// IndicatorView
		_indicatorView = new IndicatorView
		{
			AutomationId = "TestIndicatorView",
			IndicatorColor = Colors.LightGray,
			SelectedIndicatorColor = Colors.Blue,
			IndicatorSize = 25,
			IsEnabled = false,
			HorizontalOptions = LayoutOptions.Center
		};

		// Bind Indicator to Carousel
		_indicatorView.SetBinding(IndicatorView.ItemsSourceProperty, new Binding
		{
			Source = _carouselView,
			Path = "ItemsSource"
		});

		_indicatorView.SetBinding(IndicatorView.PositionProperty, new Binding
		{
			Source = _carouselView,
			Path = "Position",
			Mode = BindingMode.TwoWay
		});

		// Add children
		stackLayout.Children.Add(_carouselView);
		stackLayout.Children.Add(_indicatorView);

		// Set Content
		Content = stackLayout;
	}
}
