using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31361, "CarouselView content is scrolling vertically", PlatformAffected.iOS)]
public class Issue31361 : ContentPage
{
	public Issue31361()
	{
		var verticalStackLayout = new VerticalStackLayout();
		var carouselItems = new ObservableCollection<string>
		{
			"Item 1",
			"Item 2",
			"Item 3",
			"Item 4",
		};

		CarouselView2 carouselView = new CarouselView2
		{
			ItemsSource = carouselItems,
			AutomationId = "carouselview",
			ItemTemplate = new DataTemplate(() =>
			{
				var grid = new Grid
				{
					Margin = 50,
					BackgroundColor = Colors.LightGray
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
		};

		var contentLabel = new Label
		{
			AutomationId = "contentLabel",
			Text = "The content is not scrollable",
			HorizontalOptions = LayoutOptions.Center,
			Padding = new Thickness(20),
		};

		carouselView.Scrolled += (s, e) =>
		{
			if (e.VerticalDelta < 0)
			{
				contentLabel.Text = "The content is scrollable";
			}
		};

		verticalStackLayout.Children.Add(contentLabel);
		verticalStackLayout.Children.Add(carouselView);
		Content = verticalStackLayout;
	}
}