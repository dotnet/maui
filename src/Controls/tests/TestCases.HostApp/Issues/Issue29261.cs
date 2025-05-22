using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 29261, "CarouselViewHandler2 for iOS does not properly bounce back when reaching the end with Loop=false", PlatformAffected.iOS)]
public class Issue29261 : ContentPage
{
	public Issue29261()
	{
		var verticalStackLayout = new VerticalStackLayout();
		var carouselItems = new ObservableCollection<string>
		{
			"First Item",
			"Middle Item",
			"Last Item",
		};

		CarouselView2 carousel = new CarouselView2
		{
			ItemsSource = carouselItems,
			AutomationId = "carouselview",
			HeightRequest = 200,
			Loop = false,
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
		};

		var label = new Label
		{
			Text = $"CarouselView Position - {carousel.Position}",
			AutomationId = "positionLabel",
			HorizontalOptions = LayoutOptions.Center,
		};

		carousel.PositionChanged += (s, e) =>
		{
			label.Text = $"CarouselView Position - {carousel.Position}";
		};

		verticalStackLayout.Children.Add(carousel);
		verticalStackLayout.Children.Add(label);
		Content = verticalStackLayout;
	}
}