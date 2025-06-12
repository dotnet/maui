using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29462, "CarouselView ItemTemplate Not Updating at Runtime", PlatformAffected.UWP)]
public class Issue29462 : ContentPage
{
	public Issue29462()
	{
		var verticalStackLayout = new VerticalStackLayout();
		var carouselItems = new ObservableCollection<string>
		{
			"Item 0",
			"Item 1",
			"Item 2",
			"Item 3",
			"Item 4",
		};

		CarouselView2 carouselView = new CarouselView2
		{
			ItemsSource = carouselItems,
			ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset,
			HeightRequest = 300,
			Loop = false,
			HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
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
			VerticalOptions = LayoutOptions.Center
		};
		carouselView.IndicatorView = indicatorView;

		var itemTempateButton = new Button
		{
			Text = "Change Item Template",
			AutomationId = "ChangeItemTemplate",
			Margin = new Thickness(20),
		};

		itemTempateButton.Clicked += (sender, e) =>
		{
			carouselView.ItemTemplate = new DataTemplate(() =>
			{
				Grid grid = new Grid
				{
					BackgroundColor = Colors.LightGray,
					HeightRequest = 200,
					WidthRequest = 300,
					Margin = new Thickness(10)
				};

				var label = new Label
				{
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					FontSize = 24,
					TextColor = Colors.DarkBlue
				};

				label.SetBinding(Label.TextProperty, new Binding(".")
				{
					StringFormat = "{0} (Grid Template)"
				});

				grid.Children.Add(label);
				return grid;
			});
		};

		verticalStackLayout.Children.Add(carouselView);
		verticalStackLayout.Children.Add(indicatorView);
		verticalStackLayout.Children.Add(itemTempateButton);
		Content = verticalStackLayout;
	}
}