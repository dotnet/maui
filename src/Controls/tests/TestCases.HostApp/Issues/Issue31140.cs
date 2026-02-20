using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31140, "Setting both IndicatorSize and Shadow properties on IndicatorView causes some dots to be invisible", PlatformAffected.iOS)]
public class Issue31140 : ContentPage
{
	public Issue31140()
	{
		var verticalStackLayout = new VerticalStackLayout();
		var carouselItems = new ObservableCollection<string>
		{
			"Item 0",
			"Item 1",
			"Item 2",
		};

		CarouselView carouselView = new CarouselView
		{
			ItemsSource = carouselItems,
			AutomationId = "carouselview",
			HeightRequest = 300,
			ItemTemplate = new DataTemplate(() =>
			{
				var grid = new Grid
				{
					Padding = 10
				};

				var label = new Label
				{
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					FontSize = 18,
				};
				label.SetBinding(Label.TextProperty, ".");
				label.SetBinding(Label.AutomationIdProperty, ".");

				grid.Children.Add(label);
				return grid;
			}),
			HorizontalOptions = LayoutOptions.Fill,
		};

		var indicatorView = new IndicatorView
		{
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			IndicatorSize = 30,
			SelectedIndicatorColor = Colors.Blue,
			IndicatorColor = Colors.Orange,
		};
		indicatorView.Shadow = new Shadow
		{
			Radius = 10,
			Opacity = 0.7f,
			Brush = Colors.Black,
			Offset = new Point(5, 5)
		};
		carouselView.IndicatorView = indicatorView;

		verticalStackLayout.Children.Add(carouselView);
		verticalStackLayout.Children.Add(indicatorView);
		Content = verticalStackLayout;
	}
}