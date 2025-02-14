using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 27563, "[Windows] CarouselView Scrolling Issue", PlatformAffected.UWP)]
public partial class Issue27563 : ContentPage
{
	public Issue27563()
	{
		var verticalStackLayout = new VerticalStackLayout();
		verticalStackLayout.Padding = new Thickness(0,20,0,0);

		var carouselItems = new ObservableCollection<string>
		{
			"Remain View",
			"Actual View",
			"Percentage View",
		};

		CarouselView carousel = new CarouselView
		{
			ItemsSource = carouselItems,
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

				grid.Children.Add(label);
				return grid;
			}),
			HorizontalOptions = LayoutOptions.Fill,
			VerticalOptions = LayoutOptions.Fill
		};

		var indicatorView = new IndicatorView
		{
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		carousel.IndicatorView = indicatorView;
		var button = new Button
		{
			Text = "Change IndicatorView Position",
			AutomationId = "PositionButton",
			Margin = new Thickness(20),
			BackgroundColor = Colors.LightBlue
		};

		button.Clicked += (sender, e) =>
		{
			carousel.CurrentItem = carouselItems[2];
		};


		verticalStackLayout.Children.Add(carousel);
		verticalStackLayout.Children.Add(indicatorView);
		verticalStackLayout.Children.Add(button);

		Content = verticalStackLayout;
	}
}